using System.Security.Cryptography;
using System.Text;
using Asp.Versioning;
using EmployeeAPI.Data;
using EmployeeAPI.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Features.v1.Auth;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion(1)]
public class AuthController : ControllerBase
{
    private readonly PasswordHasher<User> _hasher = new();
    private readonly AppDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AppDbContext context, ILogger<AuthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("generateAVeryInsecureToken_pleasedontusethisever")]
    public IActionResult GetToken([FromBody] GetTokenRequestBody request)
    {
        return Ok(HttpContext.GenerateJwt(request.Role, request.Username));
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = new User { 
            Username = request.Username!, 
            Password = string.Empty 
        };
        user.Password = _hasher.HashPassword(user,request.Password!);
        
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return Ok("User successfully registered.");
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Username.Equals(request.Username, StringComparison.CurrentCultureIgnoreCase));
        if (user is null) return Unauthorized("User not found.");

        var result = _hasher.VerifyHashedPassword(user,user.Password,request.Password!);

        if (result == PasswordVerificationResult.Failed)
        return Unauthorized("Incorrect password.");
        
        return Ok(/*new { token }*/);
    }
}
