using System;
using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace EmployeeAPI.Employees;

public class CreateEmployeeRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? SocialSecurityNumber { get; set; }

    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}

public class CreateEmployeeRequestValidator : AbstractValidator<CreateEmployeeRequest> 
{
    public CreateEmployeeRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required.");
        RuleFor(x => x.LastName).NotEmpty();
    }
}

/*
public class CreateEmployeeRequestValidator : AbstractValidator<CreateEmployeeRequest>
{
    private readonly EmployeeRepository _repository;

    public CreateEmployeeRequestValidator(EmployeeRepository repository)
    {
        this._repository = repository;
        
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.SocialSecurityNumber).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("SSN cannot be empty.")
            .MustAsync(BeUnique).WithMessage("SSN must be unique.");

        When(r => r.Address1 != null, () => {
            RuleFor(x => x.Address1).NotEmpty();
            RuleFor(x => x.City).NotEmpty();
            RuleFor(x => x.State).NotEmpty();
            RuleFor(x => x.ZipCode).NotEmpty();
        });
    }

    private async Task<bool> BeUnique(string ssn, CancellationToken token)
    {
        return await _repository.GetEmployeeBySsn(ssn) != null;
    }
}
*/