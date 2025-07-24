using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AJRAApis.Dtos.Payslips
{
    public class PayslipDeductionDto
    {
        public string? EmployeeId { get; set; }
        public float GrossAmount { get; set; }
        public float Uniforms { get; set; } 
        public float TillShortage { get; set; }
        public float Wastages { get; set; }
        public float OtherDeductions { get; set; }
        public float NetAmount { get; set; }
        public DateTime PaySlipDate { get; set; } = DateTime.Now;

    }
}

