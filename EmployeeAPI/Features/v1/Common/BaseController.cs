using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Features.v1.Common;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion(1)]
[Produces("application/json")]
public abstract class BaseController : Controller
{
}
