using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AJRAApis.Data;
using AJRAApis.Dtos;
using AJRAApis.Interfaces;
using AJRAApis.Models;
using Microsoft.EntityFrameworkCore;

namespace AJRAApis.Repository
{
    public class RedbookRepository : iRedbookRepository
    {
        public readonly ApplicationDBContext _context;
        public RedbookRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<Redbook> AddAsync(RedbookDto redbook)
        {
            // Add the redbook to the database
                // Retrieve the last ID from the database
            var lastRedbook = await _context.Redbook
                .OrderByDescending(r => r.Id)
                .FirstOrDefaultAsync();

            // Generate the new ID
            string newId;
            if (lastRedbook != null)
            {
                // Extract numeric part of the ID
                int lastNumber = int.Parse(lastRedbook.Id.Substring(3)); // Skip "INV"
                newId = $"INV{(lastNumber + 1):D3}"; // Increment and format as 3 digits
            }
            else
            {
                // No existing IDs, start with INV001
                newId = "INV001";
            }

            var redbookItem = new Redbook
            {
                Id = newId,
                EmployeeId = redbook.EmployeeId,
                Uniforms = redbook.Uniforms,
                TillShortage = redbook.TillShortage,
                Wastage = redbook.Wastage,
                OtherDeductions = redbook.OtherDeductions,
                Date = redbook.Date,
                Reason = redbook.Reason
            };
        
            await _context.Redbook.AddAsync(redbookItem);
            // Save the changes
            await _context.SaveChangesAsync();
            return redbookItem;
        }

        public async Task<List<Redbook>> GetAllAsync()
        {
            return await _context.Redbook.ToListAsync();
        }

        public async Task<List<Redbook>> GetByIdAsync(string id)
        {
            return await _context.Redbook.Where(p => p.EmployeeId == id).ToListAsync();
        }

        public async Task<List<Redbook>> GetRedbookSummaryAsync(string employeeId, string startdate, string enddate)
        {
            return await _context.Redbook.Where(r => r.EmployeeId == employeeId && r.Date >= DateTime.Parse(startdate) && r.Date <= DateTime.Parse(enddate)).ToListAsync();
        }
    }
}