using API.Services;
using API.Utils;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class EmployeesController : ControllerBase
{
    private readonly EmployeeService _employeeService;

    public EmployeesController(EmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetEmployees([FromQuery] string? fields = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = Constants.MAX_PAGES)
    {
        return await ResponseUtils.HandleResponseAsync(async () =>
        {
            var (employees, totalCount) = await _employeeService.GetFilteredEmployeesAsync(fields, pageNumber, pageSize);
            return new
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Employees = employees
            };
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmployee(int id)
    {
        return await ResponseUtils.HandleResponseAsync(async () => await _employeeService.GetEmployeeByIdAsync(id));
    }

    [HttpPost]
    public async Task<IActionResult> PostEmployee(Employee employee)
    {
        return await ResponseUtils.HandleResponseAsync(async () =>
        {
            var createdEmployee = await _employeeService.CreateEmployeeAsync(employee);
            return CreatedAtAction(nameof(GetEmployee), new { id = createdEmployee.Id }, createdEmployee);
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutEmployee(int id, Employee employee)
    {
        return await ResponseUtils.HandleResponseAsync(async () =>
        {
            await _employeeService.UpdateEmployeeAsync(id, employee);
            return NoContent();
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        return await ResponseUtils.HandleResponseAsync(async () =>
        {
            await _employeeService.DeleteEmployeeAsync(id);
            return NoContent();
        });
    }

    [HttpGet("exists/{email}")]
    public async Task<IActionResult> EmployeeExists(string email)
    {
        return await ResponseUtils.HandleResponseAsync(async () =>
        {
            var exists = await _employeeService.EmployeeExistsAsync(email);
            return new { Exists = exists };
        });
    }
}