using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AJRAApis.Dtos.Employees
{
    public class UpdateEmployeeDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string IDNumber { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18, 2)")]
        public decimal HourlyRate { get; set; } 
        public string Designation { get; set; } = string.Empty;
        public float Leave { get; set; }
        public float SickLeave { get; set; }
        public float OtherLeave { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Salary { get; set; }
        public string TaxRefNum { get; set; } = string.Empty;
        public string WorkStatus { get; set; } = string.Empty;
        public string StatusChangeDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
    }
}