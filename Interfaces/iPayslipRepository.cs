using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AJRAApis.Dtos.Payslips;
using AJRAApis.Models;

namespace AJRAApis.Interfaces
{
    public interface iPayslipRepository
    {
        Task<List<PaySlips>> GetAllAsync();
        Task<PayslipSummaryDto> GetSummaryAsync(IFormFile file,string employeeId, string workerName, string designation, float hourlyRate,float Salary);
        Task<PayslipDto> GeneratePaySlipPDF(PayslipDto payslip, string designation);
        Task<PaySlips?> AddAsync(PaySlips payslip);
        Task<List<string>> GetDistinctPaySlipCyclesAsync();
        Task<List<PayslipDto>> GetPayslipSummaryAsync(string payslipcycle);
        Task<IFormFile> GetPayslipPDFAsync(List<PayslipDto> payslips,string payslipcycle);
        Task<PayslipDto> UpdatePayslip(PaySlips payslip, string designation);
        Task<PayslipDto?> GetPayslipByIDAsync(string id);
    }
}