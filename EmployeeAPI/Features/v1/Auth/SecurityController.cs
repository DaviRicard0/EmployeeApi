using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Features.v1.Auth;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion(1)]
public class AuthController : ControllerBase
{
    [HttpPost("generateAVeryInsecureToken_pleasedontusethisever")]
    public IActionResult GetToken([FromBody] GetTokenRequestBody request)
    {
        return Ok(HttpContext.GenerateJwt(request.Role, request.Username));
    }
}
