using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AJRAApis.Dtos.EmployeeLeave;
using AJRAApis.Models;

namespace AJRAApis.Interfaces
{
    public interface iEmployeeLeave
    {
        Task<List<EmployeeLeave>> GetAllAsync();
        Task<List<EmployeeLeaveDto>> GetAllByIdAsync(string Employeeid);

        Task CreateAsync(EmployeeLeave employeeLeave);
    }
}