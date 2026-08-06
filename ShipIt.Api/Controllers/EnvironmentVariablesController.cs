using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShipIt.Core.DTOs.EnvironmentVariables;
using ShipIt.Core.Services;
using System.Security.Claims;

namespace ShipIt.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/deployment-configurations/{deploymentConfigurationId:guid}/environment-variables")]
public class EnvironmentVariablesController : ControllerBase
{
    private readonly IEnvironmentVariableService _service;

    public EnvironmentVariablesController(
        IEnvironmentVariableService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<EnvironmentVariableResponse>> Create(
        Guid deploymentConfigurationId,
        CreateEnvironmentVariableRequest request)
    {
        var response = await _service.CreateAsync(
            deploymentConfigurationId,
            GetCurrentUserId(),
            request);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                deploymentConfigurationId,
                environmentVariableId = response.EnvironmentVariableId
            },
            response);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EnvironmentVariableResponse>>> GetAll(
        Guid deploymentConfigurationId)
    {
        var response = await _service.GetAllAsync(
            deploymentConfigurationId,
            GetCurrentUserId());

        return Ok(response);
    }

    [HttpGet("{environmentVariableId:guid}")]
    public async Task<ActionResult<EnvironmentVariableResponse>> GetById(
        Guid deploymentConfigurationId,
        Guid environmentVariableId)
    {
        var response = await _service.GetByIdAsync(
            deploymentConfigurationId,
            environmentVariableId,
            GetCurrentUserId());

        if (response is null)
            return NotFound();

        return Ok(response);
    }

    [HttpPut("{environmentVariableId:guid}")]
    public async Task<ActionResult<EnvironmentVariableResponse>> Update(
        Guid deploymentConfigurationId,
        Guid environmentVariableId,
        UpdateEnvironmentVariableRequest request)
    {
        var response = await _service.UpdateAsync(
            deploymentConfigurationId,
            environmentVariableId,
            GetCurrentUserId(),
            request);

        return Ok(response);
    }

    [HttpDelete("{environmentVariableId:guid}")]
    public async Task<IActionResult> Delete(
        Guid deploymentConfigurationId,
        Guid environmentVariableId)
    {
        await _service.DeleteAsync(
            deploymentConfigurationId,
            environmentVariableId,
            GetCurrentUserId());

        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}