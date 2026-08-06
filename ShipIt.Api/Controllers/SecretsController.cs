using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShipIt.Core.DTOs.Secrets;
using ShipIt.Core.Services;
using System.Security.Claims;

namespace ShipIt.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/deployment-configurations/{deploymentConfigurationId:guid}/secrets")]
public class SecretsController : ControllerBase
{
    private readonly ISecretService _service;

    public SecretsController(ISecretService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<SecretResponse>> Create(
        Guid deploymentConfigurationId,
        CreateSecretRequest request)
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
                secretId = response.SecretId
            },
            response);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SecretResponse>>> GetAll(
        Guid deploymentConfigurationId)
    {
        var response = await _service.GetAllAsync(
            deploymentConfigurationId,
            GetCurrentUserId());

        return Ok(response);
    }

    [HttpGet("{secretId:guid}")]
    public async Task<ActionResult<SecretResponse>> GetById(
        Guid deploymentConfigurationId,
        Guid secretId)
    {
        var response = await _service.GetByIdAsync(
            deploymentConfigurationId,
            secretId,
            GetCurrentUserId());

        if (response is null)
            return NotFound();

        return Ok(response);
    }

    [HttpPut("{secretId:guid}")]
    public async Task<ActionResult<SecretResponse>> Update(
        Guid deploymentConfigurationId,
        Guid secretId,
        UpdateSecretRequest request)
    {
        var response = await _service.UpdateAsync(
            deploymentConfigurationId,
            secretId,
            GetCurrentUserId(),
            request);

        return Ok(response);
    }

    [HttpDelete("{secretId:guid}")]
    public async Task<IActionResult> Delete(
        Guid deploymentConfigurationId,
        Guid secretId)
    {
        await _service.DeleteAsync(
            deploymentConfigurationId,
            secretId,
            GetCurrentUserId());

        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}