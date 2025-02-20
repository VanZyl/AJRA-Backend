using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AJRAApis.Dtos.Payslips;
using AJRAApis.Models;

namespace AJRAApis.Mappers
{
    public static class PayslipMapper
    {
        public static PayslipDto ToPayslipDto(this PaySlips payslip)
        {
            return new PayslipDto
            {
                Id = payslip.Id,
                Name = payslip.Name,
                Surname = payslip.Surname,
                HourlyRate = payslip.HourlyRate,
                Salary = payslip.Salary,
                NormalHoursWorked = payslip.NormalHoursWorked,
                NormalAmountPaid = payslip.NormalAmountPaid,
                OvertimeHoursWorked = payslip.OvertimeHoursWorked,
                OvertimeAmountPaid = payslip.OvertimeAmountPaid,
                PublicHolidayHoursWorked = payslip.PublicHolidayHoursWorked,
                PublicHolidayAmountPaid = payslip.PublicHolidayAmountPaid,
                LeaveHoursWorked = payslip.LeaveHoursWorked,
                LeaveAmountPaid = payslip.LeaveAmountPaid,
                GrossAmount = payslip.GrossAmount,
                UIFContribution = payslip.UIFContribution,
                BarganingCouncil = payslip.BarganingCouncil,
                Uniforms = payslip.Uniforms,
                TillShortage = payslip.TillShortage,
                Wastages = payslip.Wastages,
                OtherDeductions = payslip.OtherDeductions,
                NetAmount = payslip.NetAmount,
                LeaveBF =  payslip.LeaveBF,
                LeaveAcc = payslip.LeaveAcc,
                LeaveTaken = payslip.LeaveTaken,
                SickLeaveTaken = payslip.SickLeaveTaken,
                OtherTaken = payslip.OtherTaken,
                PaySlipCycle = payslip.PaySlipCycle,   
                PaySlipDate = payslip.PaySlipDate,
                EmployeeId = payslip.EmployeeId
            };
        }

        public static PaySlips ToPayslipFromPayslipDTO(this PayslipDto payslipDto)
        {
            return new PaySlips
            {
                Id = payslipDto.Id,
                Name = payslipDto.Name,
                Surname = payslipDto.Surname,
                HourlyRate = payslipDto.HourlyRate,
                Salary = payslipDto.Salary,
                NormalHoursWorked = payslipDto.NormalHoursWorked,
                NormalAmountPaid = payslipDto.NormalAmountPaid,
                OvertimeHoursWorked = payslipDto.OvertimeHoursWorked,
                OvertimeAmountPaid = payslipDto.OvertimeAmountPaid,
                PublicHolidayHoursWorked = payslipDto.PublicHolidayHoursWorked,
                PublicHolidayAmountPaid = payslipDto.PublicHolidayAmountPaid,
                LeaveHoursWorked = payslipDto.LeaveHoursWorked,
                LeaveAmountPaid = payslipDto.LeaveAmountPaid,
                GrossAmount = payslipDto.GrossAmount,
                UIFContribution = payslipDto.UIFContribution,
                BarganingCouncil = payslipDto.BarganingCouncil,
                Uniforms = payslipDto.Uniforms,
                TillShortage = payslipDto.TillShortage,
                Wastages = payslipDto.Wastages,
                OtherDeductions = payslipDto.OtherDeductions,
                NetAmount = payslipDto.NetAmount,
                LeaveBF =  payslipDto.LeaveBF,
                LeaveAcc = payslipDto.LeaveAcc,
                LeaveTaken = payslipDto.LeaveTaken,
                SickLeaveTaken = payslipDto.SickLeaveTaken,
                OtherTaken = payslipDto.OtherTaken,
                PaySlipCycle = payslipDto.PaySlipCycle,   
                PaySlipDate = payslipDto.PaySlipDate,
                EmployeeId = payslipDto.EmployeeId
            };
        }
    }
}