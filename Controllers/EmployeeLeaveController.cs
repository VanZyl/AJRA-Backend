using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AJRAApis.Data;
using AJRAApis.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cors;
using AJRAApis.Models;
using AJRAApis.Interfaces;
using AJRAApis.Mappers;
using AJRAApis.Dtos.EmployeeLeave;

namespace AJRAApis.Controllers
{
    [Route("api/employeeleave")]
    [ApiController]
    [EnableCors("MyPolicy")]
    public class EmployeeLeaveController: ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly iEmployeeLeave _employeeRepo;
        public EmployeeLeaveController(ApplicationDBContext context, iEmployeeLeave employeeleaveRepository)
        {
            _context = context;
            _employeeRepo = employeeleaveRepository;
        }
 
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var employees = await _employeeRepo.GetAllAsync();
            return Ok(employees);
        }
        
        [HttpGet("{Employeeid}")]
        public async Task<IActionResult> GetAllByIdAsync([FromRoute] string Employeeid)
        {
            var employeeLeave = await _employeeRepo.GetAllByIdAsync(Employeeid);
            if (employeeLeave == null)
            {
                return NotFound();
            }
            return Ok(employeeLeave);    
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EmployeeLeaveDto employeeLeave)
        {
            // Check if the input object is null and return a BadRequest response if it is.
            if (employeeLeave == null)
            {
                return BadRequest();
            }

            // Retrieve all employee leaves from the database and check if the table is empty.
            if (!_context.EmployeeLeave.Any())
            {
                // If the database is empty, create a new EmployeeLeave object with an Id of "1".
                var newemployeeLeave = new EmployeeLeave
                {
                    Id = "1", // Set the initial Id to "1".
                    TransCode = employeeLeave.TransCode,
                    EmployeeId = employeeLeave.EmployeeId,
                    Description = employeeLeave.Description,
                    DateFrom = employeeLeave.DateFrom,
                    DateTo = employeeLeave.DateTo,
                    DaysTaken = employeeLeave.DaysTaken,
                    DaysAccrued = employeeLeave.DaysAccrued,
                    DaysDue = employeeLeave.DaysDue,
                    Remarks = employeeLeave.Remarks
                };

                // Save the new EmployeeLeave object to the database.
                await _employeeRepo.CreateAsync(newemployeeLeave);

                // Return an Ok response with the created EmployeeLeaveDto object as the response body.
                return Ok(employeeLeave);
            }
            else
            {
                // If the database is not empty, retrieve the last Id, increment it, and create the new object.
                
                // Retrieve the maximum Id from the database.
                // var lastId = _context.EmployeeLeave
                //                     .OrderByDescending(el => el.Id) // Order by Id in descending order.
                //                     .Select(el => int.Parse(el.Id)) // Parse the Id to an integer.
                //                     .FirstOrDefault(); // Get the first (largest) Id.
                // Console.WriteLine(lastId);
                var lastId = _context.EmployeeLeave
                    .OrderByDescending(el => el.Id.Length) // Order by length first for correct string comparison.
                    .ThenByDescending(el => el.Id)        // Then order lexicographically.
                    .Select(el => el.Id)                  // Select the Id as string.
                    .FirstOrDefault();                    // Get the first (largest) Id.


                // Increment the Id.
                var newId = (int.Parse(lastId) + 1).ToString(); // Convert the incremented Id back to a string.

                // Create a new EmployeeLeave object with the incremented Id and passed-in information.
                var newemployeeLeave = new EmployeeLeave
                {
                    Id = newId, // Use the incremented Id.
                    TransCode = employeeLeave.TransCode,
                    EmployeeId = employeeLeave.EmployeeId,
                    Description = employeeLeave.Description,
                    DateFrom = employeeLeave.DateFrom,
                    DateTo = employeeLeave.DateTo,
                    DaysTaken = employeeLeave.DaysTaken,
                    DaysAccrued = employeeLeave.DaysAccrued,
                    DaysDue = employeeLeave.DaysDue,
                    Remarks = employeeLeave.Remarks
                };

                // Save the new EmployeeLeave object to the database.
                await _employeeRepo.CreateAsync(newemployeeLeave);

                // Return an Ok response with the created EmployeeLeaveDto object as the response body.
                return Ok(employeeLeave);
            }
        }
    }
}