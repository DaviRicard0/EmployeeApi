using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Employees;

[Authorize]
public class EmployeesController : BaseController
{
    private readonly AppDbContext _context;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(AppDbContext context, ILogger<EmployeesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region GET: All Employees
    /// <summary>
    /// Get all employees.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GetEmployeeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllEmployees([FromQuery] GetAllEmployeesRequest request)
    {
        int page = request?.Page ?? 1;
        int pageSize = request?.RecordsPerPage ?? 100;

        var query = _context.Employees.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request?.FirstNameContains))
            query = query.Where(e => e.FirstName.Contains(request.FirstNameContains));

        if (!string.IsNullOrWhiteSpace(request?.LastNameContains))
            query = query.Where(e => e.LastName.Contains(request.LastNameContains));

        var employees = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync();

        return Ok(employees.Select(EmployeeToGetEmployeeResponse));
    }
    #endregion

    #region GET: Employee By ID
    /// <summary>
    /// Gets an employee by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(GetEmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEmployeeById(int id)
    {
        var employee = await _context.Employees.FindAsync(id);

        return employee == null
            ? NotFound()
            : Ok(EmployeeToGetEmployeeResponse(employee));
    }
    #endregion

    #region POST: Create Employee
    /// <summary>
    /// Creates a new employee.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(GetEmployeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request)
    {
        var newEmployee = new Employee
        {
            FirstName = request.FirstName!,
            LastName = request.LastName!,
            SocialSecurityNumber = request.SocialSecurityNumber,
            Address1 = request.Address1,
            Address2 = request.Address2,
            City = request.City,
            State = request.State,
            ZipCode = request.ZipCode,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email
        };

        await _context.Employees.AddAsync(newEmployee);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEmployeeById), new { id = newEmployee.Id }, EmployeeToGetEmployeeResponse(newEmployee));
    }
    #endregion

    #region PUT: Update Employee
    /// <summary>
    /// Updates an employee.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(GetEmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeRequest request)
    {
        _logger.LogInformation("Updating employee with ID: {EmployeeId}", id);

        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        _logger.LogDebug("Updating employee details for ID: {EmployeeId}", id);

        employee.Address1 = request.Address1;
        employee.Address2 = request.Address2;
        employee.City = request.City;
        employee.State = request.State;
        employee.ZipCode = request.ZipCode;
        employee.PhoneNumber = request.PhoneNumber;
        employee.Email = request.Email;

        try
        {
            _context.Entry(employee).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Employee with ID: {EmployeeId} successfully updated", id);
            return Ok(EmployeeToGetEmployeeResponse(employee));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating employee with ID: {EmployeeId}", id);
            return StatusCode(500, "An error occurred while updating the employee.");
        }
    }
    #endregion

    #region DELETE: Delete Employee
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var employee = await _context.Employees.FindAsync(id);

        if (employee == null)
            return NotFound();

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    #endregion

    #region GET: Employee Benefits
    /// <summary>
    /// Gets the benefits for an employee.
    /// </summary>
    [HttpGet("{employeeId}/benefits")]
    [ProducesResponseType(typeof(IEnumerable<GetEmployeeResponseEmployeeBenefit>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBenefitsForEmployee(int employeeId)
    {
        var employee = await _context.Employees
            .Include(e => e.Benefits)
            .ThenInclude(e => e.Benefit)
            .SingleOrDefaultAsync(e => e.Id == employeeId);

        if (employee == null)
            return NotFound();

        var benefits = employee.Benefits.Select(b => new GetEmployeeResponseEmployeeBenefit
        {
            Id = b.Id,
            Name = b.Benefit.Name,
            Description = b.Benefit.Description,
            Cost = b.CostToEmployee ?? b.Benefit.BaseCost
        });

        return Ok(benefits);
    }
    #endregion

    #region Private Helpers
    private static GetEmployeeResponse EmployeeToGetEmployeeResponse(Employee employee) => new()
    {
        FirstName = employee.FirstName,
        LastName = employee.LastName,
        Address1 = employee.Address1,
        Address2 = employee.Address2,
        City = employee.City,
        State = employee.State,
        ZipCode = employee.ZipCode,
        PhoneNumber = employee.PhoneNumber,
        Email = employee.Email
    };
    #endregion
}
