using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AJRAApis.Dtos;
using AJRAApis.Models;

namespace AJRAApis.Interfaces
{
    public interface iRedbookRepository
    {
        Task<List<Redbook>> GetAllAsync();
        Task<Redbook> AddAsync(RedbookDto redbook);
        Task<List<Redbook>> GetRedbookSummaryAsync(string employeeId, string startdate, string enddate);
    }
}