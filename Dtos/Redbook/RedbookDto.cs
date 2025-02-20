using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AJRAApis.Dtos
{
    public class RedbookDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public float Uniforms { get; set; } = 0;
        public float TillShortage { get; set; } = 0;
        public float Wastage { get; set; } = 0;
        public float OtherDeductions { get; set; } = 0;
        public DateTime Date { get; set; } = DateTime.Now;
        public string Reason { get; set; } = string.Empty;
    }
}