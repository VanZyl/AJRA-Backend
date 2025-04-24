using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AJRAApis.Data;
using AJRAApis.Dtos.EmployeeLeave;
using AJRAApis.Interfaces;
using AJRAApis.Mappers;
using AJRAApis.Models;
using Microsoft.EntityFrameworkCore;

namespace AJRAApis.Repository
{
    public class EmployeeLeaveRepository : iEmployeeLeave
    {

        private readonly ApplicationDBContext _context;
        public EmployeeLeaveRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<EmployeeLeave>> GetAllAsync()
        {
            return await _context.EmployeeLeave.ToListAsync();
        }

        public async Task<List<EmployeeLeaveDto>> GetAllByIdAsync(string Employeeid)
        {
            return await _context.EmployeeLeave
                        .Where(x => x.EmployeeId == Employeeid)
                        .OrderBy(x => Convert.ToInt32(x.Id))
                        .Select(x =>x.ToEmployeeLeaveDto())
                        .ToListAsync();

        }

        public async Task CreateAsync(EmployeeLeave employeeLeave)
        {
            _context.EmployeeLeave.Add(employeeLeave);
            await _context.SaveChangesAsync();
        }
    }
}