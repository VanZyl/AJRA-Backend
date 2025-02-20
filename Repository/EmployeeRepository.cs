using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AJRAApis.Data;
using AJRAApis.Dtos.Employees;
using AJRAApis.Interfaces;
using AJRAApis.Mappers;
using AJRAApis.Models;
using Microsoft.EntityFrameworkCore;

namespace AJRAApis.Repository
{
    public class EmployeeRepository : iEmployeeRepository
    {
        private readonly ApplicationDBContext _context;
        public EmployeeRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<Employee> AddAsync(Employee employee)
        {
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task<Employee?> DeleteAsync(string id)
        {
            var employeeModel = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (employeeModel == null)
            {
                return null;
            }
            _context.Employees.Remove(employeeModel);
            await _context.SaveChangesAsync();
            return employeeModel;
        }

        public async Task<List<Employee>> GetAllAsync()
        {
            return await _context.Employees.ToListAsync();
        }

        public async Task<Employee?> GetByIdAsync(string id)
        {
            return await _context.Employees.FindAsync(id);
        }

        public async Task<List<EmployeesDto>> GetByWorkStatusAsync(string workstatus)
        {
            return await _context.Employees
                .Where(e => EF.Functions.Like(e.WorkStatus, $"%{workstatus}%"))
                .Select(s => s.ToEmployeeDto()).ToListAsync();
        }

        public async Task<EmployeesDto?> UpdateAsync(string id, EmployeesDto employee)
        {
            var employeeModel = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);

            if (employeeModel == null)
            {
                return null;
            }

            employeeModel.Name = employee.Name;
            employeeModel.Surname = employee.Surname;
            employeeModel.IDNumber = employee.IDNumber;
            employeeModel.HourlyRate = employee.HourlyRate;
            employeeModel.Designation = employee.Designation;
            employeeModel.Leave = employee.Leave;
            employeeModel.SickLeave = employee.SickLeave;
            employeeModel.OtherLeave = employee.OtherLeave;
            employeeModel.Salary = employee.Salary;
            employeeModel.TaxRefNum = employee.TaxRefNum;
            employeeModel.WorkStatus = employee.WorkStatus;
            employeeModel.StatusChangeDate = employee.StatusChangeDate;
            employeeModel.StoreId = employee.StoreId;
            employeeModel.StoreDesignation = employee.StoreDesignation;
            employeeModel.BankName = employee.BankName;
            employeeModel.AccountNumber = employee.AccountNumber;

            await _context.SaveChangesAsync();
            return employeeModel.ToEmployeeDto();
        }
    }
}