using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Auth;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    [HttpPost("generateAVeryInsecureToken_pleasedontusethisever")]
    public IActionResult GetToken([FromBody] GetTokenRequestBody request)
    {
        return Ok(HttpContext.GenerateJwt(request.Role, request.Username));
    }
}
