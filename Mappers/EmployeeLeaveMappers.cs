using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AJRAApis.Dtos.EmployeeLeave;
using AJRAApis.Models;

namespace AJRAApis.Mappers
{
    public static class EmployeeLeaveMappers
    {
        public static EmployeeLeaveDto ToEmployeeLeaveDto( this EmployeeLeave employees)
        {
            return new EmployeeLeaveDto
            {
                TransCode = employees.TransCode,
                EmployeeId = employees.EmployeeId,
                Description = employees.Description,
                DateFrom = employees.DateFrom,
                DateTo = employees.DateTo,
                DaysTaken = employees.DaysTaken,
                DaysAccrued = employees.DaysAccrued,
                DaysDue = employees.DaysDue,
                Remarks = employees.Remarks
            };
        }

    }
}