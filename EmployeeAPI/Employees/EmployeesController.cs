using EmployeeAPI.Abstractions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Employees;

public class EmployeesController : BaseController
{
    private readonly IRepository<Employee> _repository;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(
        IRepository<Employee> repository,
        ILogger<EmployeesController> logger
    )
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Get all employees.
    /// </summary>
    /// <returns>An array of all employees.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GetEmployeeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetAllEmployees()
    {
        return Ok(_repository.GetAll().Select(EmployeeToGetEmployeeResponse));
    }

    /// <summary>
    /// Gets an employee by ID.
    /// </summary>
    /// <param name="id">The ID of the employee.</param>
    /// <returns>The single employee record.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(GetEmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetEmployeeById([FromRoute] int id)
    {
        var employee = _repository.GetById(id);
        if (employee == null)
        {
            return NotFound();
        }

        return Ok(EmployeeToGetEmployeeResponse(employee));
    }

    /// <summary>
    /// Creates a new employee.
    /// </summary>
    /// <param name="employeeRequest">The employee to be created.</param>
    /// <returns>A link to the employee that was created.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(GetEmployeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult CreateEmployee([FromBody] CreateEmployeeRequest employeeRequest)
    {
        var newEmployee = new Employee
        {
            FirstName = employeeRequest.FirstName!,
            LastName = employeeRequest.LastName!,
            SocialSecurityNumber = employeeRequest.SocialSecurityNumber,
            Address1 = employeeRequest.Address1,
            Address2 = employeeRequest.Address2,
            City = employeeRequest.City,
            State = employeeRequest.State,
            ZipCode = employeeRequest.ZipCode,
            PhoneNumber = employeeRequest.PhoneNumber,
            Email = employeeRequest.Email
        };

        _repository.Create(newEmployee);
        return CreatedAtAction(nameof(GetEmployeeById), new { id = newEmployee.Id }, newEmployee);
    }

    /// <summary>
    /// Updates an employee.
    /// </summary>
    /// <param name="id">The ID of the employee to update.</param>
    /// <param name="employeeRequest">The employee data to update.</param>
    /// <returns></returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(GetEmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult UpdateEmployee([FromRoute] int id, [FromBody] UpdateEmployeeRequest employeeRequest)
    {
        _logger.LogInformation("Updating employee with ID: {EmployeeId}",id);
        var existingEmployee = _repository.GetById(id);
        if (existingEmployee == null)
        {
            return NotFound();
        }

        existingEmployee.Address1 = employeeRequest.Address1;
        existingEmployee.Address2 = employeeRequest.Address2;
        existingEmployee.City = employeeRequest.City;
        existingEmployee.State = employeeRequest.State;
        existingEmployee.ZipCode = employeeRequest.ZipCode;
        existingEmployee.PhoneNumber = employeeRequest.PhoneNumber;
        existingEmployee.Email = employeeRequest.Email;

        _repository.Update(existingEmployee);

        return Ok(existingEmployee);
    }

    /// <summary>
    /// Gets the benefits for an employee.
    /// </summary>
    /// <param name="employeeId">The ID to get the benefits for.</param>
    /// <returns>The benefits for that employee.</returns>
    [HttpGet("{employeeId}/benefits")]
    [ProducesResponseType(typeof(IEnumerable<GetEmployeeResponseEmployeeBenefit>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetBenefitsForEmployee(int employeeId)
    {
        var employee = _repository.GetById(employeeId);
        if (employee == null)
        {
            return NotFound();
        }
        return Ok(employee.Benefits.Select(BenefitToBenefitResponse));
    }

    private static GetEmployeeResponseEmployeeBenefit BenefitToBenefitResponse(EmployeeBenefits benefit)
    {
        return new GetEmployeeResponseEmployeeBenefit
        {
            Id = benefit.Id,
            EmployeeId = benefit.EmployeeId,
            BenefitType = benefit.BenefitType,
            Cost = benefit.Cost
        };
    }

    private GetEmployeeResponse EmployeeToGetEmployeeResponse(Employee employee)
    {
        return new GetEmployeeResponse
        {
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Address1 = employee.Address1,
            Address2 = employee.Address2,
            City = employee.City,
            State = employee.State,
            ZipCode = employee.ZipCode,
            PhoneNumber = employee.PhoneNumber,
            Email = employee.Email,
            Benefits = [.. employee.Benefits.Select(benefit => new GetEmployeeResponseEmployeeBenefit
            {
                Id = benefit.Id,
                EmployeeId = benefit.EmployeeId,
                BenefitType = benefit.BenefitType,
                Cost = benefit.Cost
            })]
        };
    }
}
