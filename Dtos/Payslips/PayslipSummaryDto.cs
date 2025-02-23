using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AJRAApis.Dtos.Payslips
{
    public class PayslipSummaryDto
    {


        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18, 2)")]
        public decimal HourlyRate { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public float NormalHoursWorked { get; set; }
        public float NormalAmountPaid { get; set; }
        public float OvertimeHoursWorked { get; set; }
        public float OvertimeAmountPaid { get; set; }
        public float PublicHolidayHoursWorked { get; set; }
        public float PublicHolidayAmountPaid { get; set; }
        public float GrossAmount { get; set; }
        public float UIFContribution { get; set; }
        public float BarganingCouncil { get; set; }
        public float Uniforms { get; set; } 
        public float TillShortage { get; set; }
        public float Wastages { get; set; }
        public float OtherDeductions { get; set; }
        public float NetAmount { get; set; }
        public float LeaveBF { get; set; }
        public float LeaveAcc { get; set; }
        public float LeaveHoursWorked { get; set; }
        public float LeaveAmountPaid { get; set; }
        public float LeaveTaken { get; set; }
        public DateTime PaySlipDate { get; set; } = DateTime.Now;
        public string PaySlipCycle { get; set; } = string.Empty;
        public List<string> workdays { get; set; } = new List<string>();
        public string EmployeeId { get; set; } = string.Empty;

    }
}