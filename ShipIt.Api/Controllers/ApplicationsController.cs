using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ShipIt.Core.Services;

namespace ShipIt.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;

    public ApplicationsController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    [HttpPost]
    public async Task<ActionResult<ApplicationResponse>> Create(
        CreateApplicationRequest request)
    {
        var ownerId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var response = await _applicationService.CreateAsync(ownerId, request);

        return CreatedAtAction(
            nameof(GetById),
            new { applicationId = response.ApplicationId },
            response);
    }

    [HttpGet("{applicationId:guid}")]
    public async Task<ActionResult<ApplicationResponse>> GetById(
        Guid applicationId)
    {
        var application = await _applicationService.GetByIdAsync(applicationId);

        if (application == null)
            return NotFound();

        return Ok(application);
    }
}