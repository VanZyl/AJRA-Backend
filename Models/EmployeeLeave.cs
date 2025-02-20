using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AJRAApis.Models
{
    public class EmployeeLeave
    {
        public string Id { get; set; } = String.Empty;
        public string TransCode { get; set; } = String.Empty;
        public string EmployeeId { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public DateOnly DateFrom { get; set; } = new DateOnly();
        public DateOnly DateTo { get; set; } = new DateOnly();
        public float DaysTaken { get; set; } = 0;
        public float DaysAccrued { get; set; } = 0;
        public float DaysDue { get; set; } = 0;
        public string Remarks { get; set; } = String.Empty;

    }
}