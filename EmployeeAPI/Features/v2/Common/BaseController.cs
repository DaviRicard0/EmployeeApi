using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Features.v2.Common;

[ApiController]
[Route("api/v2/[controller]")]
[Produces("application/json")]
public abstract class BaseController : Controller
{
}
