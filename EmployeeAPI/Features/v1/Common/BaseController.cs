using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Features.v1.Common;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public abstract class BaseController : Controller
{
}
