using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AJRAApis.Dtos.Employees;
using AJRAApis.Models;

namespace AJRAApis.Mappers
{
    public static class EmployeesMappers
    {
        public static EmployeesDto ToEmployeeDto( this Employee employees)
        {
            return new EmployeesDto
            {
                Id = employees.Id,
                Name = employees.Name,
                Surname = employees.Surname,
                IDNumber = employees.IDNumber,
                HourlyRate = employees.HourlyRate,
                Designation = employees.Designation,
                Leave = employees.Leave,
                SickLeave = employees.SickLeave,
                OtherLeave = employees.OtherLeave,
                Salary = employees.Salary,
                TaxRefNum = employees.TaxRefNum,
                WorkStatus = employees.WorkStatus,
                StatusChangeDate = employees.StatusChangeDate,
                StoreId = employees.StoreId,
                StoreDesignation = employees.StoreDesignation,
                BankName = employees.BankName,
                AccountNumber = employees.AccountNumber
            };
        }

        public static EmployeeSummaryDto ToEmployeeSummaryDto(this Employee employees)
        {
            return new EmployeeSummaryDto
            {
                Id = employees.Id,
                Name = employees.Name,
                Surname = employees.Surname,
                HourlyRate = employees.HourlyRate,
                Designation = employees.Designation,
                Leave = employees.Leave,
                Salary = employees.Salary,
            };
        }

        public static Employee ToEmployeeFromEmployeeDto(this EmployeesDto employeesDto)
        {
            return new Employee
            {
                Id = employeesDto.Id,
                Name = employeesDto.Name,
                Surname = employeesDto.Surname,
                IDNumber = employeesDto.IDNumber,
                HourlyRate = employeesDto.HourlyRate,
                Designation = employeesDto.Designation,
                Leave = employeesDto.Leave,
                SickLeave = employeesDto.SickLeave,
                OtherLeave = employeesDto.OtherLeave,
                Salary = employeesDto.Salary,
                TaxRefNum = employeesDto.TaxRefNum,
                WorkStatus = employeesDto.WorkStatus,
                StatusChangeDate = employeesDto.StatusChangeDate
            };
        }
    }
}