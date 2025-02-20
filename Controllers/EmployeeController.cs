using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AJRAApis.Data;
using AJRAApis.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cors;
using AJRAApis.Dtos.Employees;
using AJRAApis.Models;
using AJRAApis.Interfaces;




namespace AJRAApis.Controllers
{
    [Route("api/employee")]
    [ApiController]
    [EnableCors("MyPolicy")]
    public class EmployeeController : ControllerBase
    {

        private readonly ApplicationDBContext _context;
        private readonly iEmployeeRepository _employeeRepo;
        public EmployeeController(ApplicationDBContext context, iEmployeeRepository employeeRepository)
        {
            _context = context;
            _employeeRepo = employeeRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var employees = await _employeeRepo.GetAllAsync();
            var employeesDto = employees.Select(s => s.ToEmployeeDto());

            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            var employee = await _employeeRepo.GetByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee.ToEmployeeDto());    
        }

        [HttpGet("workstatus/{workstatus}")]
        public async Task<IActionResult> GetByWorkStatus([FromRoute] string workstatus)
        {
            var employees = await _employeeRepo.GetByWorkStatusAsync(workstatus);

            return Ok(employees);
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] EmployeesDto updateDto)
        {
            var employeeModel = await _employeeRepo.UpdateAsync(id, updateDto);

            if (employeeModel == null)
            {
                return NotFound();
            }
            return Ok(employeeModel);   
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EmployeesDto createDto)
        {
            var employeeModel = createDto.ToEmployeeFromEmployeeDto();
            await _employeeRepo.AddAsync(employeeModel);
            return CreatedAtAction(nameof(GetById), new { id = employeeModel.Id }, employeeModel.ToEmployeeDto());
           
        }
        
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            var employeeModel = await _employeeRepo.DeleteAsync(id);
            if (employeeModel == null)
            {
                return NotFound();
            }else{
                return Ok(employeeModel.ToEmployeeDto());
            }
    
        }

    
    }
}