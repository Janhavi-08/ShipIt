using Microsoft.AspNetCore.Mvc;
using ShipIt.Core.Services;

namespace ShipIt.Api.Controllers;

[ApiController]
[Route("api/deployment-options")]
public class DeploymentOptionsController : ControllerBase
{
    private readonly IDeploymentOptionsService _deploymentOptionsService;

    public DeploymentOptionsController(
        IDeploymentOptionsService deploymentOptionsService)
    {
        _deploymentOptionsService = deploymentOptionsService;
    }

    [HttpGet]
    public ActionResult<DeploymentOptionsResponse> Get()
    {
        return Ok(_deploymentOptionsService.GetDeploymentOptions());
    }
}