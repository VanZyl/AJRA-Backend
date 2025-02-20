using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AJRAApis.Models
{
    public class Redbook
    {
        public string Id { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public float Uniforms { get; set; } = 0;
        public float TillShortage { get; set; } = 0;
        public float Wastage { get; set; } = 0;
        public float OtherDeductions { get; set; } = 0;
        public DateTime Date { get; set; } = DateTime.Now;
        public string Reason { get; set; } = string.Empty;
    }
}