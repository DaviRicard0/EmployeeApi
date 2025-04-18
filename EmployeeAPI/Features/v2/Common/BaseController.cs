using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Features.v2.Common;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion(2)]
[Produces("application/json")]
public abstract class BaseController : Controller
{
}
