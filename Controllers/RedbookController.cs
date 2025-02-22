using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AJRAApis.Dtos;
using AJRAApis.Interfaces;
using AJRAApis.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

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
            var newRedbook = await _redbookRepo.AddAsync(redbook);
            return Ok(newRedbook);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] string employeeId, [FromQuery] string startdate, [FromQuery] string enddate)
        {
            var redbookSummary = await _redbookRepo.GetRedbookSummaryAsync(employeeId, startdate, enddate);
            return Ok(redbookSummary);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var redbook = await _redbookRepo.GetByIdAsync(id);
            return Ok(redbook);
        }
    }
}