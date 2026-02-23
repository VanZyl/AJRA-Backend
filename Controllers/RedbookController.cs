using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AJRAApis.Dtos;
using AJRAApis.Interfaces;
using AJRAApis.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Mono.Unix.Native;

namespace AJRAApis.Controllers
{
    [Route("api/redbook")]
    [ApiController]
    [EnableCors("MyPolicy")]
    public class RedbookController: ControllerBase
    {
        private readonly iRedbookRepository _redbookRepo;
        public RedbookController(iRedbookRepository redbookRepository)
        {
            _redbookRepo = redbookRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var redbooks = await _redbookRepo.GetAllAsync();
            return Ok(redbooks);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] RedbookDto redbook)
        {
            Console.WriteLine(redbook);
            // Check if the input object is null and return a BadRequest response if it is.
            if (redbook == null)
            {
                Console.WriteLine("Invalid redbook data.");
                return BadRequest("Redbook data is empty.");
            }
            var newRedbook = await _redbookRepo.AddAsync(redbook);
            Console.WriteLine("Redbook entry added successfully.");
            if (newRedbook == null)
            {
                Console.WriteLine("Failed to add redbook entry.");
                return BadRequest("Failed to add redbook entry.");
            }
            return Ok(newRedbook);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] string employeeId, [FromQuery] string startdate, [FromQuery] string enddate)
        {
            if (string.IsNullOrEmpty(employeeId) || string.IsNullOrEmpty(startdate) || string.IsNullOrEmpty(enddate))
            {
                return BadRequest("Employee ID, start date, and end date are required");
            }
            var redbookSummary = await _redbookRepo.GetRedbookSummaryAsync(employeeId, startdate, enddate);
            if (redbookSummary == null)
            {
                return NotFound("No redbook entries found for the specified criteria.");
            }
            return Ok(redbookSummary);
        }

        [HttpGet("employee")]
        public async Task<IActionResult> GetById([FromQuery] string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                return BadRequest("Employee ID is required");
            }
            var redbook = await _redbookRepo.GetBySpecificID(employeeId);
            return Ok(redbook);
        }
    }
}