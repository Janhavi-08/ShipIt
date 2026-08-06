using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShipIt.Core.DTOs.DeploymentConfiguration;
using ShipIt.Core.Services;
using System.Security.Claims;

namespace ShipIt.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/applications/{applicationId:guid}/deployment-configuration")]
public class DeploymentConfigurationController : ControllerBase
{
    private readonly IDeploymentConfigurationService _service;

    public DeploymentConfigurationController(
        IDeploymentConfigurationService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<DeploymentConfigurationResponse>> Create(
        Guid applicationId,
        CreateDeploymentConfigurationRequest request)
    {
        var userId = GetCurrentUserId();

        var response = await _service.CreateAsync(
            applicationId,
            userId,
            request);

        return CreatedAtAction(
            nameof(Get),
            new { applicationId },
            response);
    }

    [HttpGet]
    public async Task<ActionResult<DeploymentConfigurationResponse>> Get(
        Guid applicationId)
    {
        var userId = GetCurrentUserId();

        var response = await _service.GetAsync(
            applicationId,
            userId);

        if (response == null)
            return NotFound();

        return Ok(response);
    }

    [HttpPut]
    public async Task<ActionResult<DeploymentConfigurationResponse>> Update(
        Guid applicationId,
        UpdateDeploymentConfigurationRequest request)
    {
        var userId = GetCurrentUserId();

        var response = await _service.UpdateAsync(
            applicationId,
            userId,
            request);

        return Ok(response);
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}