using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Asp.Versioning;
using EmployeeAPI.Data;
using EmployeeAPI.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
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

    /// <summary>
    /// Creates a new user.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = new User { 
            Username = request.Username!, 
            Password = string.Empty 
        };
        user.Password = _hasher.HashPassword(user,request.Password!);
        
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return Ok(new LoginResponse(){
            Id = user.Id,
            Username = user.Username
        });
    }

    /// <summary>
    /// Gets the token jwt.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Username.ToLower() == request.Username!.ToLower());
        
        if (user is null) return NotFound("User not found.");

        var result = _hasher.VerifyHashedPassword(user,user.Password,request.Password!);

        if (result == PasswordVerificationResult.Failed) return Unauthorized("Incorrect password.");
        
        return Ok(HttpContext.GenerateJwt("Admin", user.Username));
    }

    [HttpGet("google-login")]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleCallback") };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!authenticateResult.Succeeded || authenticateResult?.Principal == null)
            return BadRequest("Google authentication failed");

        var email = authenticateResult.Principal.FindFirst(ClaimTypes.Email)?.Value;
        var name = authenticateResult.Principal.FindFirst(ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(email))
            return BadRequest("Unable to retrieve email from Google.");

        var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == email);

        if (user is null) return NotFound("User not found.");
        
        return Ok(HttpContext.GenerateJwt("Admin", user.Username));
    }
}
