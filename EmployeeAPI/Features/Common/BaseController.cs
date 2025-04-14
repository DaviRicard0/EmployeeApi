using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Features.Common;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseController : Controller
{
}
