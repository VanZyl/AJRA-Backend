using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AJRAApis.Data;
using AJRAApis.Dtos.Payslips;
using AJRAApis.Interfaces;
using AJRAApis.Models;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;
using AJRAApis.Mappers;
using System.ComponentModel;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Org.BouncyCastle.Pqc.Crypto.Frodo;
using AJRAApis.Dtos.EmployeeLeave;


namespace AJRAApis.Repository
{
    public class PayslipRepository : iPayslipRepository
    {
        public readonly ApplicationDBContext _context;
        public PayslipRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<List<PaySlips>> GetAllAsync()
        {
            return await _context.PaySlips.ToListAsync();
        }

        public async Task<PayslipSummaryDto?> GetSummaryAsync(IFormFile file, string employeeId, string workerName, string designation, float hourlyRate, float Salary)
        {
            Console.WriteLine("Processing Payslip Data: " + workerName + " " + designation + " " + hourlyRate + " " + Salary + file.FileName);
            // if (file == null || file.Length == 0)
            //     return Task.FromResult<PayslipSummaryDto?>(null);

            if (string.IsNullOrEmpty(workerName) || string.IsNullOrEmpty(designation))
                return await Task.FromResult<PayslipSummaryDto?>(null);

            try
            {

                var uploadDir = @"/tmp/"; // Make sure this directory exists
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadDir, uniqueFileName);

                // Ensure file is fully written before opening
                await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await file.CopyToAsync(fileStream);
                }

                Console.WriteLine($"File saved at: {filePath}");

                // Ensure file exists and is not empty
                if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
                {
                    throw new Exception("File does not exist or is empty.");
                }

                // using var stream = file.OpenReadStream();
                // using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                // file.CopyToAsync(stream);
                
                // Open file for reading
                await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                Console.WriteLine($"File length: {stream.Length} bytes");

                stream.Position = 0;
                var workbook = new XSSFWorkbook(stream);
                var sheet = workbook.GetSheetAt(0);

                Console.WriteLine("File opened");

                // Parse the worker data
                Console.WriteLine("Worker Name: " + workerName);
                var workerData = ExtractWorkerData(sheet, workerName);
                if (workerData == null)
                    return null;

                var holidays = GetSouthAfricanHolidays();               
                List<string> DaysWorked = new List<string>();

                // Get Employee Days worked and times worked
                Console.WriteLine("Processing worker data");
                var hoursWorked = ProcessData(workerData, designation, holidays, DaysWorked);
                Console.WriteLine("Days worked: ");

                // Calculate the hours worked and the pay for each type of work
                var NormalHoursWorked = 0.0;
                var OvertimeHoursWorked = 0.0;
                var PublicHolidayHoursWorked = 0.0;
                foreach (var entry in hoursWorked)
                {
                    var time = (entry.Hour + (entry.Minute/60.0));
                    if (isSunday(entry)){
                        OvertimeHoursWorked += time;
                    }else if(isHoliday(entry, holidays)){
                        PublicHolidayHoursWorked += time;
                    }else{
                        NormalHoursWorked += time;
                    }
                }
                Console.WriteLine("Calculating Data");
                // Calculate the pay for each type of work
                var NormalHoursPay = Math.Round(NormalHoursWorked * hourlyRate,2,MidpointRounding.ToEven);
                var OvertimeHoursPay = Math.Round(OvertimeHoursWorked * hourlyRate * 1.5,2,MidpointRounding.ToEven);
                var PublicHolidayHoursPay = Math.Round(PublicHolidayHoursWorked * hourlyRate * 2.0,2,MidpointRounding.ToEven);

                // Calculate the leave pay

                // Get the current month and year for database lookup
                Console.WriteLine("Employee ID: " + employeeId);
                Console.WriteLine("Getting Readbook data");

                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                // Handle the case where currentMonth is January
                var previousMonth = currentMonth == 1 ? 12 : currentMonth - 1;
                var previousYear = currentMonth == 1 ? currentYear - 1 : currentYear;

                var startdate = new DateTime(previousYear, previousMonth, 24).ToString("yyyy-MM-dd");
                Console.WriteLine("Start Date: " + startdate);

                var enddate = new DateTime(currentYear, currentMonth, 23).ToString("yyyy-MM-dd");
                Console.WriteLine("End Date: " + enddate);

                var leavetaken = _context.EmployeeLeave
                                .Where(l => l.EmployeeId == employeeId &&
                                            l.TransCode == "010" && // Leave type is leave Taken
                                            l.DateTo >= DateOnly.Parse(startdate) && // Leave ends after the start date
                                            l.DateFrom <= DateOnly.Parse(enddate))  // Leave starts before the end date
                                .ToList();
                Console.WriteLine("Leave Taken: " + leavetaken.Count);

                int totalLeaveDays = leavetaken.Sum(leave =>
                {
                    // Adjust leave dates to fit within the range
                    var adjustedStart = leave.DateFrom > DateOnly.Parse(startdate) ? leave.DateFrom : DateOnly.Parse(startdate);
                    var adjustedEnd = leave.DateTo < DateOnly.Parse(enddate) ? leave.DateTo : DateOnly.Parse(enddate);

                    // Calculate the number of days
                    return (adjustedEnd.ToDateTime(TimeOnly.MinValue) - adjustedStart.ToDateTime(TimeOnly.MinValue)).Days + 1;
                });


                Console.WriteLine($"Total Leave Days: {totalLeaveDays}");

                var leaveHours = totalLeaveDays * 7;
                var leaveAmount = leaveHours * hourlyRate;
                
                // Calculate the deductions for each of the employees


                var redlooklist = _context.Redbook.Where(r => r.EmployeeId == employeeId && r.Date >= DateTime.Parse(startdate) && r.Date <= DateTime.Parse(enddate)).ToList();
                var uniforms = 0.0;
                var tillshortage = 0.0;
                var wastage = 0.0;
                var otherdeductions = 0.0;
                foreach (var entry in redlooklist)
                {
                    uniforms += entry.Uniforms;
                    tillshortage += entry.TillShortage;
                    wastage += entry.Wastage;
                    otherdeductions += entry.OtherDeductions;
                }
                Console.WriteLine("Uniforms: " + uniforms);
                Console.WriteLine("Till Shortage: " + tillshortage);
                Console.WriteLine("Wastage: " + wastage);
                Console.WriteLine("Other Deductions: " + otherdeductions);



                var GrossIncome = Math.Round(NormalHoursPay + OvertimeHoursPay + PublicHolidayHoursPay + leaveAmount,2,MidpointRounding.ToEven);

                var UIF = Math.Round(GrossIncome * 0.01,2,MidpointRounding.ToEven);

                var BargainingCouncil = 8.0;

                var NetIncome = Math.Round(GrossIncome - UIF - BargainingCouncil - uniforms - tillshortage - wastage - otherdeductions,2,MidpointRounding.ToEven);

                float LeaveBF = _context.EmployeeLeave
                                .Where(e => e.EmployeeId == employeeId)
                                .OrderByDescending(e => e.Id)
                                .Select(e => e.DaysDue)
                                .FirstOrDefault();

                var LeaveAccumulated = Math.Round(((NormalHoursWorked + OvertimeHoursWorked + PublicHolidayHoursWorked)/8.0)/17.0,2,MidpointRounding.ToEven);


                // Generate the payslip ID
                var payslip_id = GenerateNextPaySlipId(employeeId);
                NormalHoursWorked = Math.Round(NormalHoursWorked,2,MidpointRounding.ToEven);
                OvertimeHoursWorked = Math.Round(OvertimeHoursWorked,2,MidpointRounding.ToEven);
                PublicHolidayHoursWorked = Math.Round(PublicHolidayHoursWorked,2,MidpointRounding.ToEven);
                var nameofperson = workerName.Split(" ");
                string FirstName = string.Empty;
                string Surname = string.Empty;
                if (nameofperson.Length > 2) {
                    FirstName = nameofperson[0] + " " + nameofperson[1];
                    Surname = nameofperson[2];
                }else{
                    FirstName = nameofperson[0];
                    Surname = nameofperson[1];
                }

                

                PayslipSummaryDto? payslip = null;
                
                if(designation == "Supervisor" || designation == "Catering Assistant"){
                    payslip = new PayslipSummaryDto
                    {
                        Id = payslip_id,
                        Name = FirstName,
                        Surname = Surname,
                        HourlyRate = (decimal)hourlyRate,
                        NormalHoursWorked = (float)NormalHoursWorked,
                        NormalAmountPaid = (float)NormalHoursPay,
                        OvertimeHoursWorked = (float)OvertimeHoursWorked,
                        OvertimeAmountPaid = (float)OvertimeHoursPay,
                        PublicHolidayHoursWorked = (float)PublicHolidayHoursWorked,
                        PublicHolidayAmountPaid = (float)PublicHolidayHoursPay,
                        GrossAmount = (float)GrossIncome,
                        UIFContribution = (float)UIF,
                        Uniforms = (float)uniforms,
                        TillShortage = (float)tillshortage,
                        Wastages = (float)wastage,
                        OtherDeductions = (float)otherdeductions,
                        BarganingCouncil = (float)BargainingCouncil,
                        NetAmount = (float)NetIncome,
                        LeaveAcc = (float)LeaveAccumulated,
                        LeaveBF = (float)LeaveBF,
                        LeaveTaken = (float)totalLeaveDays,
                        LeaveAmountPaid = (float)leaveAmount,
                        LeaveHoursWorked = (float)leaveHours,
                        PaySlipCycle = payslip_id.Split('-')[1],
                        workdays = DaysWorked,
                        EmployeeId = employeeId
                    };
                }else if(designation == "Independant Contractor"){
                    payslip = new PayslipSummaryDto
                    {
                        Id = payslip_id,
                        Name = FirstName,
                        Surname = Surname,
                        HourlyRate = 0,
                        NormalHoursWorked = 0,
                        NormalAmountPaid = 0,
                        OvertimeHoursWorked = 0,
                        OvertimeAmountPaid = 0,
                        PublicHolidayHoursWorked = 0,
                        PublicHolidayAmountPaid = 0,
                        Uniforms = (float)uniforms,
                        TillShortage = (float)tillshortage,
                        Wastages = (float)wastage,
                        OtherDeductions = (float)otherdeductions,
                        GrossAmount = Salary,
                        UIFContribution = 0,
                        BarganingCouncil = 0,
                        NetAmount = (float)(Salary -uniforms - tillshortage - wastage - otherdeductions),
                        LeaveAcc = 0,
                        LeaveBF = 0,
                        LeaveAmountPaid = 0,
                        LeaveHoursWorked = 0,
                        PaySlipCycle = payslip_id.Split('-')[1],
                        workdays = DaysWorked,
                        EmployeeId = employeeId
                    };
                }
                File.Delete(filePath);
                return payslip;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string GenerateNextPaySlipId(string employeeId)
        {
            // Fetch all Payslip IDs for the employee
            var lastPayslip = _context.PaySlips
                .Where(p => p.EmployeeId == employeeId)
                .OrderByDescending(p => p.PaySlipCycle) // Ensure we get the latest one
                .Select(p => p.Id)
                .FirstOrDefault();

            // If no payslip exists, return the first one
            if (lastPayslip == null)
            {
                return $"{employeeId}-01/{DateTime.Now.Year}";
            }

            // Extract the cycle part (MM/YYYY)
            var cyclePart = lastPayslip.Split('-')[1];
            var parts = cyclePart.Split('/');
            int month = int.Parse(parts[0]);
            int year = int.Parse(parts[1]);

            // Increment the cycle
            month++;
            if (month > 12)
            {
                month = 1;
                year++;
            }

            // Format the new cycle as MM/YYYY
            string newCycle = $"{month:D2}/{year}";

            // Create the new ID
            return $"{employeeId}-{newCycle}";
        }

        private List<string> readRow(ISheet sheet, int rowNumber)
        {
            var firstRow = sheet.GetRow(rowNumber); // Get the first row
            var rowData = new List<string>();
            bool isTimeRow = false;
            if (firstRow != null)
            {
                // Iterate over each cell in the row and add it to the rowData list as a string
                for (int i = 0; i < firstRow.Cells.Count; i++)
                {
                    var cell = firstRow.GetCell(i);
                    if (cell != null)
                    {
                        string cellValue = string.Empty;
                        switch (cell.CellType)
                        {
                            case CellType.String:
                                // If the cell is a string, just use its value
                                cellValue = Regex.Replace(cell.ToString(), @"\n", "");
                                break;

                            case CellType.Numeric:
                                // If the cell contains a number, check if it's a date
                                if (DateUtil.IsCellDateFormatted(cell))
                                {
                                    isTimeRow = true;
                                    // Format the date to string (customize the format as needed)
                                    cellValue = Regex.Replace(cell.DateCellValue.ToString(), @"\n", "");
                                }
                                else
                                {
                                    // Otherwise, treat it as a regular numeric value
                                    cellValue = Regex.Replace(cell.ToString(), @"\n", "");
                                }
                                break;

                            case CellType.Boolean:
                                // If the cell contains a boolean, convert it to a string
                                cellValue = cell.BooleanCellValue.ToString();
                                break;

                            case CellType.Formula:
                                // If the cell contains a formula, evaluate the formula and get its value
                                cellValue = cell.CellFormula.ToString();
                                break;

                            default:
                                // For other types, just use an empty string
                                cellValue = "";
                                break;
                        }

                        // Remove any newline characters from the value (as per your original code)
                        rowData.Add(Regex.Replace(cellValue, @"\n", ""));
                    }
                    else
                    {
                        rowData.Add(""); // Add an empty string if the cell is null
                    }
                }
            }
            if(isTimeRow)
            {
                // return string.Join("|",rowData[0],rowData[2]);
                return rowData;
            }else{
                // return string.Join(" ",rowData);
                return rowData;
            }
            
        }

        static bool isSunday(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Sunday;
        }

        static bool isHoliday(DateTime date, HashSet<DateTime> holidays)
        {
            return holidays.Contains(date.Date);
        }

        private List<Dictionary<string, string>> ExtractWorkerData(ISheet sheet, string workerName)
        {
            Console.WriteLine("Extracting worker data");
            Console.WriteLine("Worker Name: " + workerName);
            Console.WriteLine("Sheet Rows: " + sheet.LastRowNum);
            var workerData = new List<Dictionary<string, string>>();
            int? startIndex = null, endIndex = null;

            for (int i = 0; i <= sheet.LastRowNum; i++)
            {
                var rowData = readRow(sheet, i);
                if (rowData.Count == 0)
                {
                    continue;
                }else if(rowData.Count > 1){
                    if (rowData[1].Contains(workerName))
                    {
                        // Console.WriteLine("Found worker data");
                        startIndex = i + 2;
                    }
                    else if (rowData[0].Contains(workerName))
                    {
                        // Console.WriteLine("Found end of worker data");
                        endIndex = i-1;
                    }
                }
            }

            // Console.WriteLine($"Start Index: {startIndex}, End Index: {endIndex}");
            if (startIndex.HasValue && endIndex.HasValue)
            {
                for (int i = startIndex.Value; i < endIndex.Value; i++)
                {
                    var row = readRow(sheet, i);                   
                    var ClockinTime = "";
                    var ClockinDate = "";
                    if(row[0].Length > 0){
                        ClockinDate = row[0].Split(" ")[0];
                        ClockinTime = row[0].Split(" ")[1];
                    }
                    var ClockoutTime = "";
                    var ClockoutDate = "";
                    if(row[2].Length > 0){
                        ClockoutDate = row[2].Split(" ")[0];
                        ClockoutTime = row[2].Split(" ")[1];
                    }
                    var TotalHours = row[3];
                    var rowData = new Dictionary<string, string>
                    {
                        { "ClockinDate", ClockinDate },
                        { "ClockinTime", ClockinTime },
                        { "ClockoutDate", ClockoutDate },
                        { "ClockoutTime", ClockoutTime },
                        { "TotalHours", TotalHours.Split(" ")[1] }
                    };
                    workerData.Add(rowData);
                }
            }

            return workerData;
        }

        static List<DateTime> ProcessData(List<Dictionary<string, string>> rowData, string designation, HashSet<DateTime> holidays, List<string> DaysWorked)
            {

                Console.WriteLine("Processing worker data 1");                
                var hoursWorked = new List<DateTime>();

                DateTime? currentDay = null;
                DateTime? firstClockIn = null;
                DateTime? lastClockOut = null;

                DateTime? endCheckIn = null;
                DateTime? endCheckOut = null;

                foreach (var entry in rowData)
                {
                    Console.WriteLine("Start of for loop: " + entry["ClockinDate"] + " " + entry["ClockinTime"] + " " + entry["ClockoutDate"] + " " + entry["ClockoutTime"]);
                    var start = ParseDateTime(entry["ClockinDate"], entry["ClockinTime"]);
                    DateTime? end = string.IsNullOrEmpty(entry["ClockoutDate"]) || string.IsNullOrEmpty(entry["ClockoutTime"]) ? (DateTime?) null: ParseDateTime(entry["ClockoutDate"], entry["ClockoutTime"]);
                    if (end == null){
                        endCheckOut = lastClockOut;
                        endCheckIn = start;
                    }else{
                        endCheckOut = null;
                        endCheckIn = null;
                    }
                    Console.WriteLine("Processing worker data 2");

                    if(currentDay == null){
                        currentDay = start.Date;
                        firstClockIn = start;
                        lastClockOut = end;
                    }else if(currentDay != start.Date){
                        Console.WriteLine("Processing worker data 3");
                        if (lastClockOut == null){
                            if (IsSundayorHoliday(currentDay.Value, holidays)){
                                lastClockOut = new DateTime(currentDay.Value.Year, currentDay.Value.Month, currentDay.Value.Day, 20, 30, 0);
                            }else{
                                lastClockOut = new DateTime(currentDay.Value.Year, currentDay.Value.Month, currentDay.Value.Day, 21, 0, 0);
                            }
                        }
                        firstClockIn = adjust_time_morning(currentDay.Value, firstClockIn.Value, designation, holidays);
                        lastClockOut = adjust_time_evening(currentDay.Value, lastClockOut.Value, designation, holidays);

                        TimeSpan? total_hours = null;
                        DateTime? workday = null;

                        if (firstClockIn.Value.TimeOfDay < TimeSpan.FromHours(13) ){
                            if(lastClockOut.Value.TimeOfDay < TimeSpan.FromHours(13)){
                                total_hours = lastClockOut.Value - firstClockIn.Value;
                                // Console.WriteLine("Current Day: " + currentDay.Value + " Clockin: " + firstClockIn.Value + " Clockout: " + lastClockOut.Value + " Total Hours: " + total_hours);
                                DaysWorked.Add("Current Day: " + currentDay.Value.ToString().Split(" ")[0] + " Clockin: " + firstClockIn.Value + " Clockout: " + lastClockOut.Value + " Total Hours: " + total_hours + " IsSunday:"+isSunday(currentDay.Value) + " IsHoliday:"+isHoliday(currentDay.Value, holidays));
                            }else{
                                total_hours = lastClockOut.Value - firstClockIn.Value - TimeSpan.FromHours(1);
                                // Console.WriteLine("Current Day: " + currentDay.Value + " Clockin: " + firstClockIn.Value + " Clockout: " + lastClockOut.Value + " Total Hours: " + total_hours);
                                DaysWorked.Add("Current Day: " + currentDay.Value.ToString().Split(" ")[0] + " Clockin: " + firstClockIn.Value + " Clockout: " + lastClockOut.Value + " Total Hours: " + total_hours + " IsSunday:"+isSunday(currentDay.Value) + " IsHoliday:"+isHoliday(currentDay.Value, holidays));
                            }
                        }else if(firstClockIn.Value.TimeOfDay >= TimeSpan.FromHours(13)){
                            if(firstClockIn.Value.TimeOfDay < TimeSpan.FromHours(18)){
                                total_hours = lastClockOut.Value - firstClockIn.Value;
                                // Console.WriteLine("Current Day: " + currentDay.Value + " Clockin: " + firstClockIn.Value + " Clockout: " + lastClockOut.Value + " Total Hours: " + total_hours);
                                DaysWorked.Add("Current Day: " + currentDay.Value.ToString().Split(" ")[0] + " Clockin: " + firstClockIn.Value + " Clockout: " + lastClockOut.Value + " Total Hours: " + total_hours + " IsSunday:"+isSunday(currentDay.Value) + " IsHoliday:"+isHoliday(currentDay.Value, holidays));
                            }else{
                                total_hours = TimeSpan.FromHours(0);
                            }
                        }

                        Console.WriteLine("Processing worker data 4");

                        /*TODO: Need to provide a case where the time is less than an hour*/
                        if (total_hours == TimeSpan.FromHours(0) || total_hours == null || total_hours < TimeSpan.FromHours(1)){
                            workday = new DateTime(currentDay.Value.Year, currentDay.Value.Month, currentDay.Value.Day, 0, 0, 0);
                        }
                        else{
                            workday = new DateTime(currentDay.Value.Year, currentDay.Value.Month, currentDay.Value.Day, total_hours.Value.Hours, total_hours.Value.Minutes, total_hours.Value.Seconds);
                        }
                        if (workday.Value.TimeOfDay > TimeSpan.FromHours(0.5)){
                            hoursWorked.Add(workday.Value);
                        }
                        currentDay = start.Date;
                        firstClockIn = start;
                        lastClockOut = end;
                        Console.WriteLine("Processing worker data 4.1");
                    }else{
                        Console.WriteLine("Processing worker data 4.2");
                        if(endCheckOut != null){
                            if(endCheckIn.Value.TimeOfDay > TimeSpan.FromHours(19.5)){
                                lastClockOut = endCheckIn;
                            }else{
                                lastClockOut = end;
                            }
                        }else{
                            lastClockOut = end;
                        }
                        Console.WriteLine("Processing worker data 4.3");
                    }
                
                }
                Console.WriteLine("Processing worker data 5");
                if(firstClockIn != null){
                    if (lastClockOut == null){
                        if (IsSundayorHoliday(currentDay.Value, holidays)){
                            lastClockOut = new DateTime(currentDay.Value.Year, currentDay.Value.Month, currentDay.Value.Day, 20, 30, 0);
                        }else{
                            lastClockOut = new DateTime(currentDay.Value.Year, currentDay.Value.Month, currentDay.Value.Day, 21, 0, 0);
                        }
                    }
                    firstClockIn = adjust_time_morning(currentDay.Value, firstClockIn.Value, designation, holidays);
                    lastClockOut = adjust_time_evening(currentDay.Value, lastClockOut.Value, designation, holidays);

                    TimeSpan? total_hours = null;
                    DateTime? workday = null;
                    Console.WriteLine("Processing worker data 6");
                    if (firstClockIn.Value.TimeOfDay < TimeSpan.FromHours(13) ){
                        if(lastClockOut.Value.TimeOfDay < TimeSpan.FromHours(13)){
                            total_hours = lastClockOut.Value - firstClockIn.Value;
                            // Console.WriteLine("Current Day: " + currentDay.Value + " Clockin: " + firstClockIn.Value + " Clockout: " + lastClockOut.Value + " Total Hours: " + total_hours);
                            DaysWorked.Add("Current Day: " + currentDay.Value.ToString().Split(" ")[0] + " Clockin: " + firstClockIn.Value + " Clockout: " + lastClockOut.Value + " Total Hours: " + total_hours + " IsSunday:"+isSunday(currentDay.Value) + " IsHoliday:"+isHoliday(currentDay.Value, holidays));
                        }else{
                            total_hours = lastClockOut.Value - firstClockIn.Value - TimeSpan.FromHours(1);
                            DaysWorked.Add("Current Day: " + currentDay.Value.ToString().Split(" ")[0] + " Clockin: " + firstClockIn.Value + " Clockout: " + lastClockOut.Value + " Total Hours: " + total_hours + " IsSunday:"+isSunday(currentDay.Value) + " IsHoliday:"+isHoliday(currentDay.Value, holidays));
                            // Console.WriteLine("Current Day: " + currentDay.Value + " Clockin: " + firstClockIn.Value + " Clockout: " + lastClockOut.Value + " Total Hours: " + total_hours);
                        }
                    }else if(firstClockIn.Value.TimeOfDay >= TimeSpan.FromHours(13)){
                        if(firstClockIn.Value.TimeOfDay < TimeSpan.FromHours(18)){
                            total_hours = lastClockOut.Value - firstClockIn.Value;
                            // Console.WriteLine("Current Day: " + currentDay.Value + " Clockin: " + firstClockIn.Value + " Clockout: " + lastClockOut.Value + " Total Hours: " + total_hours);
                            DaysWorked.Add("Current Day: " + currentDay.Value.ToString().Split(" ")[0] + " Clockin: " + firstClockIn.Value + " Clockout: " + lastClockOut.Value + " Total Hours: " + total_hours + " IsSunday:"+isSunday(currentDay.Value) + " IsHoliday:"+isHoliday(currentDay.Value, holidays));
                        }else{
                            total_hours = TimeSpan.FromHours(0);
                        }
                    }
                    Console.WriteLine("Processing worker data 1");
                    if (total_hours == TimeSpan.FromHours(0)){
                        workday = new DateTime(currentDay.Value.Year, currentDay.Value.Month, currentDay.Value.Day, 0, 0, 0);
                    }
                    else{
                        workday = new DateTime(currentDay.Value.Year, currentDay.Value.Month, currentDay.Value.Day, total_hours.Value.Hours, total_hours.Value.Minutes, total_hours.Value.Seconds);
                    }
                    if (workday.Value.TimeOfDay > TimeSpan.FromHours(0.5)){
                        hoursWorked.Add(workday.Value);
                    }
                }
                return hoursWorked;
            }
 
        static DateTime adjust_time_morning(DateTime currentday, DateTime firstclockin, string designation, HashSet<DateTime> holidays){
            if(IsSundayorHoliday(currentday, holidays)){
                if (designation == "Catering Assistant"){
                    if(firstclockin.TimeOfDay < TimeSpan.FromHours(8)){
                        return new DateTime(currentday.Year, currentday.Month, currentday.Day, 8, 0, 0);
                    }
                }else if(designation == "Supervisor"){
                    if(firstclockin.TimeOfDay < TimeSpan.FromHours(7.5)){
                        return new DateTime(currentday.Year, currentday.Month, currentday.Day, 7, 30, 0);
                    }
                }
            }else{
                if (designation == "Catering Assistant"){
                    if(firstclockin.TimeOfDay < TimeSpan.FromHours(8)){
                        return new DateTime(currentday.Year, currentday.Month, currentday.Day, 8, 0, 0);
                    }
                }else if(designation == "Supervisor"){
                    if(firstclockin.TimeOfDay < TimeSpan.FromHours(7.5)){
                        return new DateTime(currentday.Year, currentday.Month, currentday.Day, 7, 30, 0);
                    }
                }
            }
            return firstclockin;
        }

        static DateTime adjust_time_evening(DateTime currentday, DateTime lastclockin, string designation, HashSet<DateTime> holidays){
            if(IsSundayorHoliday(currentday, holidays)){
                if(lastclockin.TimeOfDay > TimeSpan.FromHours(21)){
                    return new DateTime(currentday.Year, currentday.Month, currentday.Day, 21, 0, 0);
                }
            }else{
                if(lastclockin.TimeOfDay > TimeSpan.FromHours(21.5)){
                    return new DateTime(currentday.Year, currentday.Month, currentday.Day, 21, 30, 0);
                }
            }
            return lastclockin;
        }

        static bool IsSundayorHoliday(DateTime date, HashSet<DateTime> holidays)
        {
            return holidays.Contains(date.Date) || date.DayOfWeek == DayOfWeek.Sunday;
        }

        static HashSet<DateTime> GetSouthAfricanHolidays()
        {

            // This needs to be changed to an API call that can do current years

            var holidays = new HashSet<DateTime>();
            var yearIndependentDates = new List<(int Month, int Day)>
            {
                (1, 1),   // New Year's Day
                (3, 21),  // Human Rights Day
                (4, 18),  // Good Friday
                (4, 28),  // Freedom Day
                (5, 1),   // Workers' Day
                (6, 16),  // Youth Day
                (8, 9),   // National Women's Day
                (9, 24),  // Heritage Day
                (12, 16), // Day of Reconciliation
                (12, 25), // Christmas Day
                (12, 26)  // Day of Goodwill
            };

            foreach (var date in yearIndependentDates)
            {
                holidays.Add(new DateTime(DateTime.Now.Year , date.Month, date.Day));
            }

            return holidays;
        }

        static DateTime ParseDateTime(string date, string time)
        {
            // Console.WriteLine("Parsing Date: " + date + " Time: " + time);
            // return DateTime.ParseExact($"{date} {time}", "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            // Try different formats to cover common regional variations
            string[] formats = { "yyyy/MM/dd HH:mm:ss", "MM/dd/yyyy HH:mm:ss", "dd/MM/yyyy HH:mm:ss" };

            // Attempt to parse with multiple format strings
            if (DateTime.TryParseExact($"{date} {time}", formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDateTime))
            {
                return parsedDateTime;
            }
            else
            {
                throw new FormatException($"Unable to parse date/time: {date} {time}");
            }
        }

        static QuestPDF.Infrastructure.IContainer CellWithMargin(QuestPDF.Infrastructure.IContainer container)
        {
            return container.PaddingBottom(3);
        }

        static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer container)
        {
            return container.Border(1)
                .Padding(5)
                .AlignCenter()
                .AlignMiddle();
        }

        static QuestPDF.Infrastructure.IContainer CellWithLargeMargin(QuestPDF.Infrastructure.IContainer container)
        {
            return container.PaddingBottom(10);
        }
        public async Task<PayslipDto> GeneratePaySlipPDF(PayslipDto payslip, string designation)
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            // Handle the case where currentMonth is January
            var previousMonth = currentMonth == 1 ? 12 : currentMonth - 1;
            var previousYear = currentMonth == 1 ? currentYear - 1 : currentYear;

            var startdate = new DateTime(previousYear, previousMonth, 24).ToString("yyyy-MM-dd");
            // Console.WriteLine("Start Date: " + startdate);

            var enddate = new DateTime(currentYear, currentMonth, 23).ToString("yyyy-MM-dd");
            // Console.WriteLine("End Date: " + enddate);
            // Add leave accumulated to the database and update the amount of leave due this must be done by sorting the leave by id and EmployeeId and taking the last entries leave balance and adding the leave accumulated to it
            // Add the leave accumulated to the database

            // Find the last entry for the specific employee
            var lastEntry = _context.EmployeeLeave
                .Where(l => l.EmployeeId == payslip.EmployeeId) // Filter for leave taken transactions
                .OrderByDescending(l => l.Id) // Order by DateTo in descending order
                .FirstOrDefault(); // Get the most recent entry or null if no records

            if (lastEntry != null)
            {
                var leaveDue = lastEntry.DaysDue; // Extract the LeaveDue field (or equivalent field name)
                // Console.WriteLine($"Last Leave Due: {leaveDue}");
            }
            else
            {
                var LeaveDue = 0.0;
                // Console.WriteLine("No leave records found for the specified employee.");
            }
                            // Retrieve the maximum Id from the database.
            var lastId = _context.EmployeeLeave
                                .OrderByDescending(el => el.Id) // Order by Id in descending order.
                                .Select(el => int.Parse(el.Id)) // Parse the Id to an integer.
                                .FirstOrDefault(); // Get the first (largest) Id.

            // Increment the Id.
            var newId = (lastId + 1).ToString(); // Convert the incremented Id back to a string.
           
            QuestPDF.Settings.License = LicenseType.Community;
            string employeeName = payslip.Name + " " + payslip.Surname;
            string baseDirectory = @"/usr/pdfs";

            string folderName = payslip.PaySlipDate.ToString("MMMM yyyy");
            string FolderPath = Path.Combine(baseDirectory, folderName);
            string fileName = employeeName + ".pdf";
            string filePath = Path.Combine(FolderPath, fileName);
            // Console.WriteLine("Generating Payslip for: " + employeeName);
            // Console.WriteLine("File Path: " + filePath);

            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
                // Console.WriteLine("Directory Created: " + FolderPath);
            }

            QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Content()
                        .Column(column =>
                        {
                            column.Spacing(10);

                            // Header
                            column.Item().Row(row =>
                            {
                                row.RelativeColumn().Text("AJRA INVESTMENTS PTY LTD").Bold().FontSize(18);
                            });

                            column.Item().Row(row =>
                            {
                                row.RelativeColumn().Text("DATE: " + payslip.PaySlipDate).FontSize(12);
                                row.ConstantColumn(100).AlignRight().Text(payslip.Id);
                            });


                            column.Spacing(12);

                            // Employee Details
                            column.Item().Border(1).BorderColor(Colors.Black).Padding(10).Column(details => {

                                    details.Spacing(8);
                                    details.Item().Text(text =>
                                        {
                                            text.Span("EMPLOYEE Name: ").Bold().FontSize(12);
                                            text.Span(payslip.Name + " " + payslip.Surname).FontSize(12);
                                        });
                                        details.Item().Text(text =>
                                        {
                                            text.Span("DESIGNATION: ").Bold().FontSize(12);
                                            text.Span(designation).FontSize(12);
                                        });
                                        details.Item().Text(text =>
                                        {
                                            text.Span("PERIOD: ").Bold().FontSize(12);
                                            text.Span(payslip.PaySlipCycle).FontSize(12);
                                        });
                                        details.Item().Text(text =>
                                        {
                                            text.Span("HOURLY RATE: ").Bold().FontSize(12);
                                            text.Span("R " + payslip.HourlyRate.ToString()).FontSize(12);
                                        });
                                
                            });

                            column.Item().Border(1).BorderColor(Colors.Black).Padding(10).Column(income =>{

                                income.Spacing(10);
                                

                                // Income Table
                                income.Item().Text("INCOME").Bold().FontSize(12);
                                income.Item().Table(table =>
                                {
                                    
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                    if (payslip.Salary == 0){
                                        table.Header(header =>
                                        {
                                            header.Cell().Element(CellWithMargin).Text("DESCRIPTION").Bold();
                                            header.Cell().Element(CellWithMargin).Text("QUANTITY").Bold();
                                            header.Cell().Element(CellWithMargin).Text("RATE").Bold();
                                            header.Cell().Element(CellWithMargin).Text("AMOUNT").Bold();
                                        });


                                        table.Cell().Element(CellWithMargin).Text("Hourly Wage");
                                        table.Cell().Element(CellWithMargin).Text(payslip.NormalHoursWorked.ToString());
                                        table.Cell().Element(CellWithMargin).Text("R " +payslip.HourlyRate.ToString());
                                        table.Cell().Element(CellWithMargin).Text("R " +payslip.NormalAmountPaid.ToString());

                                        table.Cell().Element(CellWithMargin).Text("Overtime @ 1.5x");
                                        table.Cell().Element(CellWithMargin).Text(payslip.OvertimeHoursWorked.ToString());
                                        table.Cell().Element(CellWithMargin).Text("R " +Math.Round(payslip.HourlyRate * (decimal)1.5, 2, MidpointRounding.ToEven).ToString());
                                        table.Cell().Element(CellWithMargin).Text("R " +payslip.OvertimeAmountPaid.ToString());

                                        table.Cell().Element(CellWithMargin).Text("Public holiday @ 2x");
                                        table.Cell().Element(CellWithMargin).Text(payslip.PublicHolidayHoursWorked.ToString());
                                        table.Cell().Element(CellWithMargin).Text("R " +Math.Round(payslip.HourlyRate * (decimal)2.0, 2, MidpointRounding.ToEven).ToString());
                                        table.Cell().Element(CellWithMargin).Text("R " +payslip.PublicHolidayAmountPaid.ToString());

                                        table.Cell().Element(CellWithLargeMargin).Text("Leave Pay");
                                        table.Cell().Element(CellWithLargeMargin).Text(payslip.LeaveHoursWorked.ToString());
                                        table.Cell().Element(CellWithLargeMargin).Text("R " +payslip.HourlyRate.ToString());
                                        table.Cell().Element(CellWithLargeMargin).Text("R " +payslip.LeaveAmountPaid.ToString());

                                        table.Cell().ColumnSpan(4).BorderBottom(1).BorderColor(Colors.Black).Height(5).Element(CellWithMargin);

                                        table.Cell().ColumnSpan(3).Element(CellWithMargin).Text("GROSS EARNINGS").Bold();
                                        table.Cell().Element(CellWithMargin).Text("R " +payslip.GrossAmount.ToString());
                                    }else{
                                        table.Header(header =>
                                        {
                                            header.Cell().ColumnSpan(3).Element(CellWithMargin).Text("DESCRIPTION").Bold();
                                            header.Cell().Element(CellWithMargin).Text("AMOUNT").Bold();
                                        });

                                        table.Cell().ColumnSpan(3).Element(CellWithMargin).Text("Salary");
                                        table.Cell().Element(CellWithLargeMargin).Text("R " +payslip.Salary.ToString());
                                    }
                                });
                            });


                            column.Item().Border(1).BorderColor(Colors.Black).Padding(10).Column(deductions =>{
                                deductions.Spacing(10);

                                // Deductions
                                deductions.Item().Text("DEDUCTIONS").Bold().FontSize(12);
                                deductions.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                    table.Cell().ColumnSpan(3).Element(CellWithMargin).Text("UIF contributions");
                                    table.Cell().Element(CellWithMargin).Text("R " +payslip.UIFContribution.ToString());

                                    table.Cell().ColumnSpan(3).Element(CellWithMargin).Text("Barganing Council");
                                    table.Cell().Element(CellWithMargin).Text("R " +payslip.BarganingCouncil.ToString());
                                    
                                    if(payslip.Uniforms > 0){
                                        table.Cell().ColumnSpan(3).Element(CellWithMargin).Text("Uniforms");
                                        table.Cell().Element(CellWithMargin).Text("R " +payslip.Uniforms.ToString());
                                    }

                                    if(payslip.TillShortage > 0){
                                        table.Cell().ColumnSpan(3).Element(CellWithMargin).Text("Till Shortage");
                                        table.Cell().Element(CellWithMargin).Text("R " +payslip.TillShortage.ToString());
                                    }
                                    
                                    if(payslip.Wastages > 0){
                                        table.Cell().ColumnSpan(3).Element(CellWithMargin).Text("Wastage");
                                        table.Cell().Element(CellWithMargin).Text("R " +payslip.Wastages.ToString());
                                    }

                                    if(payslip.OtherDeductions > 0){
                                        table.Cell().ColumnSpan(3).Element(CellWithLargeMargin).Text("Other Deductions");
                                        table.Cell().Element(CellWithMargin).Text("R " +payslip.OtherDeductions.ToString());
                                    }

                                    table.Cell().ColumnSpan(4).BorderBottom(1).BorderColor(Colors.Black).Height(5).Element(CellWithMargin);

                                    var total_deductions = Math.Round((payslip.UIFContribution + payslip.BarganingCouncil + payslip.Uniforms + payslip.TillShortage + payslip.Wastages + payslip.OtherDeductions),2, MidpointRounding.ToEven);
                                    table.Cell().ColumnSpan(3).Element(CellWithMargin).Text("TOTAL DEDUCTIONS").Bold();
                                    table.Cell().Element(CellWithMargin).Text("R " +total_deductions.ToString());
                                });
                            });


                            column.Item().Border(1).BorderColor(Colors.Black).Padding(10).Column(compnaycontributions =>{
                                compnaycontributions.Spacing(10);

                                // Deductions
                                compnaycontributions.Item().Text("COMPANY CONTRIBUTIONS").Bold().FontSize(12);
                                compnaycontributions.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                    table.Cell().ColumnSpan(3).Element(CellWithMargin).Text("UIF contributions");
                                    table.Cell().Element(CellWithMargin).Text("R " +payslip.UIFContribution.ToString());

                                    table.Cell().ColumnSpan(3).Element(CellWithMargin).Text("Barganing Council");
                                    table.Cell().Element(CellWithMargin).Text("R " +payslip.BarganingCouncil.ToString());
                                });
                            });

                            column.Spacing(10);

                            // Nett Pay
                            column.Item().Text("NETT PAY").Bold().FontSize(16).FontColor(Colors.Green.Darken2).AlignCenter();
                            column.Item().Text("R " +payslip.NetAmount.ToString()).FontSize(18).Bold().AlignCenter();


                            column.Item().Border(1).BorderColor(Colors.Black).Padding(10).Column(leave =>{
                                leave.Spacing(10);

                                // Leave Balances
                                leave.Item().Text("LEAVE BALANCES").Bold().FontSize(12);
                                leave.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellWithMargin).Text("TYPE").Bold();
                                        header.Cell().Element(CellWithMargin).Text("DUE - MONTH START").Bold();
                                        header.Cell().Element(CellWithMargin).Text("TAKEN - DURING MONTH").Bold();
                                    });

                                    table.Cell().Element(CellWithMargin).Text("Leave");
                                    table.Cell().Element(CellWithMargin).Text((payslip.LeaveBF + payslip.LeaveAcc).ToString());
                                    table.Cell().Element(CellWithMargin).Text(payslip.LeaveTaken.ToString());

                                    table.Cell().Element(CellWithMargin).Text("Sick Leave");
                                    table.Cell().Element(CellWithMargin).Text("0.00");
                                    table.Cell().Element(CellWithMargin).Text(payslip.SickLeaveTaken.ToString());

                                    table.Cell().Element(CellWithMargin).Text("Other Leave");
                                    table.Cell().Element(CellWithMargin).Text("0.00");
                                    table.Cell().Element(CellWithMargin).Text(payslip.OtherTaken.ToString());
                                });
                            });
                        });
                });
            })
            .GeneratePdf(filePath);

              // Retrieve the maximum Id from the database.
            var leaveid = _context.EmployeeLeave
                .OrderByDescending(el => el.Id.Length) // Order by length first for correct string comparison.
                .ThenByDescending(el => el.Id)        // Then order lexicographically.
                .Select(el => el.Id)                  // Select the Id as string.
                .FirstOrDefault();                    // Get the first (largest) Id.


            // Increment the Id.
            var newleaveid = (int.Parse(leaveid) + 1).ToString(); // Convert the incremented Id back to a string.

            var leaveentry = new EmployeeLeave
            {
                Id = newleaveid,
                EmployeeId = payslip.EmployeeId,
                TransCode = "001",
                Description = "Leave Accured",
                DateFrom = DateOnly.Parse(startdate),
                DateTo = DateOnly.Parse(enddate),
                DaysAccrued = payslip.LeaveAcc,
                DaysDue = (float)(payslip.LeaveBF + payslip.LeaveAcc),
                DaysTaken = (int)payslip.LeaveTaken,
                Remarks = "Leave Accrued for the month of " + DateTime.Now.ToString("MMMM yyyy")
            };

            await _context.EmployeeLeave.AddAsync(leaveentry);
            await _context.SaveChangesAsync();

            Console.WriteLine("PDF generated successfully!");
            return payslip;
        }
        public async Task<PaySlips?> AddAsync(PaySlips payslip)
        {
            await _context.PaySlips.AddAsync(payslip);
            await _context.SaveChangesAsync();

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == payslip.EmployeeId);
            var leave = payslip.LeaveAcc + payslip.LeaveBF; //- payslip.LeaveTaken;
            if(employee == null)
                return null;
            employee.Leave = leave;
            await _context.SaveChangesAsync();
            return payslip;
        }
        public async Task<List<string>> GetDistinctPaySlipCyclesAsync()
        {
            return await _context.PaySlips.Select(p => p.PaySlipCycle).Distinct().ToListAsync();
        }

        public async Task<List<PayslipDto>> GetPayslipSummaryAsync(string payslipcycle)
        {
            // Fetch all Payslips for a payslip cycle
            var allPayslips = await _context.PaySlips
                .Where(p => p.PaySlipCycle == payslipcycle)
                .OrderBy(p => p.Name)
                .Select(p => p.ToPayslipDto())
                .ToListAsync();
                

            return allPayslips;
        }

        public async Task<IFormFile> GetPayslipPDFAsync(List<PayslipDto> payslips, string payslipcycle){
            QuestPDF.Settings.License = LicenseType.Community;
            var stream = new MemoryStream();
            string baseDirectory = @"/usr/pdfs";
            string folderName = payslips[0].PaySlipDate.ToString("MMMM yyyy");
            string FolderPath = Path.Combine(baseDirectory, folderName);
            var pdfFiles = Directory.GetFiles(FolderPath, "*.pdf");

            QuestPDF.Fluent.Document.Create(container =>
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header()
                    .Text("Payslip Summary " + payslipcycle)
                    .FontSize(28)
                    .Bold();
                    
                    page.Content()
                    .PaddingVertical(8)
                        .Table(table =>
                        {
                            // Table Header
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(100); // Name
                                columns.ConstantColumn(100); // Surname
                                columns.RelativeColumn();   // Gross Pay
                                columns.RelativeColumn();   // Net Pay
                                columns.RelativeColumn();   // UIF Contribution
                                columns.RelativeColumn();   // Payslip Date
                            });

                            // Add Header Row
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Name").Bold();
                                header.Cell().Element(CellStyle).Text("Surname").Bold();
                                header.Cell().Element(CellStyle).Text("Gross Pay").Bold();
                                header.Cell().Element(CellStyle).Text("Nett Pay").Bold();
                                header.Cell().Element(CellStyle).Text("UIF Contribution(1%)").Bold();
                                header.Cell().Element(CellStyle).Text("Payslip Date").Bold();
                            });

                            // Add Data Rows
                            foreach (var summary in payslips)
                            {
                                table.Cell().Element(CellStyle).Text(summary.Name);
                                table.Cell().Element(CellStyle).Text(summary.Surname);
                                table.Cell().Element(CellStyle).Text(summary.GrossAmount.ToString("C", CultureInfo.CurrentCulture));
                                table.Cell().Element(CellStyle).Text(summary.NetAmount.ToString("C", CultureInfo.CurrentCulture));
                                table.Cell().Element(CellStyle).Text(summary.UIFContribution.ToString("C", CultureInfo.CurrentCulture));
                                table.Cell().Element(CellStyle).Text(summary.PaySlipDate.ToString("yyyy-MM-dd"));
                            }
                        });
                })

            ).GeneratePdf(stream);

            var outputDocument = new PdfDocument();

            var generatedPDF = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
            for(int i = 0; i < generatedPDF.PageCount; i++){
                var page = generatedPDF.Pages[i];
                outputDocument.AddPage(page);
            }

            foreach(var file in pdfFiles){
                var existingPDF = PdfReader.Open(file, PdfDocumentOpenMode.Import);
                for(int i = 0; i < existingPDF.PageCount; i++){
                    var page = existingPDF.Pages[i];
                    outputDocument.AddPage(page);
                }
            }

            // outputDocument.Save(FolderPath + "\\PayslipSummary.pdf");
            stream.Position = 0;
            var outputStream = new MemoryStream();
            outputDocument.Save(outputStream, false);
            outputStream.Position = 0;
            // How can I also save the file to the server?
            var filepath = Path.Combine(FolderPath, "PayslipSummary.pdf");
            using (var fileStream = new FileStream(filepath, FileMode.Create, FileAccess.Write))
            {
                outputStream.CopyTo(fileStream);
            }

            return new FormFile(outputStream, 0, outputStream.Length, "file", "PayslipSummary " + payslipcycle + ".pdf");
        }
    
        public async Task<PayslipDto> UpdatePayslip(PaySlips payslip, string designation){
            var payslipToUpdate = await _context.PaySlips.FirstOrDefaultAsync(p => p.EmployeeId == payslip.EmployeeId && p.PaySlipCycle == payslip.PaySlipCycle);
            if(payslipToUpdate == null)
                return null;
            payslipToUpdate.NormalHoursWorked = payslip.NormalHoursWorked;
            payslipToUpdate.NormalAmountPaid = payslip.NormalAmountPaid;
            payslipToUpdate.OvertimeHoursWorked = payslip.OvertimeHoursWorked;
            payslipToUpdate.OvertimeAmountPaid = payslip.OvertimeAmountPaid;
            payslipToUpdate.PublicHolidayHoursWorked = payslip.PublicHolidayHoursWorked;
            payslipToUpdate.PublicHolidayAmountPaid = payslip.PublicHolidayAmountPaid;
            payslipToUpdate.LeaveHoursWorked = payslip.LeaveHoursWorked;
            payslipToUpdate.LeaveAmountPaid = payslip.LeaveAmountPaid;
            payslipToUpdate.GrossAmount = payslip.GrossAmount;
            payslipToUpdate.UIFContribution = payslip.UIFContribution;
            payslipToUpdate.BarganingCouncil = payslip.BarganingCouncil;
            payslipToUpdate.Uniforms = payslip.Uniforms;
            payslipToUpdate.TillShortage = payslip.TillShortage;
            payslipToUpdate.Wastages = payslip.Wastages;
            payslipToUpdate.OtherDeductions = payslip.OtherDeductions;
            payslipToUpdate.NetAmount = payslip.NetAmount;
            payslipToUpdate.LeaveAcc = payslip.LeaveAcc;
            payslipToUpdate.LeaveBF = payslip.LeaveBF;
            payslipToUpdate.LeaveTaken = payslip.LeaveTaken;
            payslipToUpdate.SickLeaveTaken = payslip.SickLeaveTaken;
            payslipToUpdate.OtherTaken = payslip.OtherTaken;
            await _context.SaveChangesAsync();

            var payslipDto = payslipToUpdate.ToPayslipDto();
            await GeneratePaySlipPDF(payslipDto, designation);
            return payslipDto;
        }

        public Task<PayslipDto?> GetPayslipByIDAsync(string id){
            return _context.PaySlips
                .Where(p => p.Id == id)
                .Select(p => p.ToPayslipDto())
                .FirstOrDefaultAsync();

        }

    }


}