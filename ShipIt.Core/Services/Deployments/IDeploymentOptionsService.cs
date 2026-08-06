using ShipIt.Core.Deployment;

namespace ShipIt.Core.Services;

public interface IDeploymentOptionsService
{
       DeploymentOptionsResponse GetDeploymentOptions();
}