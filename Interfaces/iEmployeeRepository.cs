using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AJRAApis.Dtos.Employees;
using AJRAApis.Models;

namespace AJRAApis.Interfaces
{
    public interface iEmployeeRepository
    {
        Task<List<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(string id);
        Task<List<EmployeesDto>> GetByWorkStatusAsync(string workstatus);
        Task<Employee> AddAsync(Employee employee);
        Task<EmployeesDto?> UpdateAsync(string id, EmployeesDto employee);
        Task<Employee?> DeleteAsync(string id);

    }
}