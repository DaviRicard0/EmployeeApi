using EmployeeAPI.Features.v2.Common;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Features.v2.Companies;

public class CompaniesController : BaseController
{
    [HttpGet]
    public IActionResult GetAllCompanies (){
        return Ok();
    }
}
