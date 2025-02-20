using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AJRAApis.Dtos.Employees;
using AJRAApis.Dtos.Payslips;
using AJRAApis.Interfaces;
using AJRAApis.Mappers;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;



namespace AJRAApis.Controllers
{
    [Route("api/payslip")]
    [ApiController]
    [EnableCors("MyPolicy")]
    public class PayslipController: ControllerBase
    {
        private readonly iPayslipRepository _payslipRepo;
        public PayslipController(iPayslipRepository payslipRepository)
        {
            _payslipRepo = payslipRepository;
            
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var payslips = await _payslipRepo.GetAllAsync();
            var payslipDtos = payslips.Select(p => p.ToPayslipDto());
            return Ok(payslipDtos);
        }


        [HttpPost("process")]
        public  async Task<IActionResult> ProcessWorkerData(IFormFile file,[FromQuery] string employeeId, [FromQuery] string workerName, [FromQuery] string designation, [FromQuery] float hourlyRate, [FromQuery] float Salary)
        {
            Console.WriteLine("Processing Payslip Data");
            var payslip_summary = await _payslipRepo.GetSummaryAsync(file, employeeId, workerName, designation, hourlyRate, Salary);
            return Ok(payslip_summary);
        }

        [HttpPost("generatepayslip")]
        public async Task<IActionResult> GeneratePayslip([FromBody] PayslipDto payslip, [FromQuery] string designation)
        {    
            var full_payslip = payslip.ToPayslipFromPayslipDTO();
            var payslip_pdf = await _payslipRepo.GeneratePaySlipPDF(payslip, designation);
            
            await _payslipRepo.AddAsync(full_payslip);

            return Ok(full_payslip);
            // return Ok(payslip_pdf);
        }

        [HttpGet("distinctpayslipcycles")]
        public async Task<IActionResult> GetDistinctPaySlipCycles()
        {
            var distinct_pay_slip_cycles = await _payslipRepo.GetDistinctPaySlipCyclesAsync();
            return Ok(distinct_pay_slip_cycles);
        }

        [HttpGet("payslipsummary")]
        public async Task<IActionResult> GetPayslipSummary([FromQuery] string payslipcycle)
        {
            var payslip_summary = await _payslipRepo.GetPayslipSummaryAsync(payslipcycle);
            var payslip_summary_pdf = await _payslipRepo.GetPayslipPDFAsync(payslip_summary,payslipcycle);
            return File(payslip_summary_pdf.OpenReadStream(), "application/pdf",payslip_summary_pdf.FileName);
        }

        [HttpPut("updatepayslip")]
        public async Task<IActionResult> UpdatePayslip([FromBody] PayslipDto payslip, [FromQuery] string designation)
        {
            var updated_payslip = await _payslipRepo.UpdatePayslip(payslip.ToPayslipFromPayslipDTO(),designation);
            return Ok(updated_payslip);
        }

        [HttpGet("getpayslipbyid")]
        public async Task<IActionResult> GetPayslipByID([FromQuery] string id)
        {
            var payslip = await _payslipRepo.GetPayslipByIDAsync(id);
            return Ok(payslip);
        }


    }
}