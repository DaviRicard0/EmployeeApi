using EmployeeAPI.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Features.v1.Auth;

public class RegisterRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    private readonly AppDbContext _context;

    public RegisterRequestValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.Username)
            .NotEmpty()
            .MustAsync(BeUniqueUsernameAsync).WithMessage("Username already exists.");

        RuleFor(x => x.Password)
            .NotEmpty();
    }

    private async Task<bool> BeUniqueUsernameAsync(string? username, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(username)) return false;

        return !await _context.Users
            .AnyAsync(u => u.Username.Equals(username, StringComparison.CurrentCultureIgnoreCase), token);
    }
}
