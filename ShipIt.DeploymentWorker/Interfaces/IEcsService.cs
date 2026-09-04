namespace ShipIt.DeploymentWorker.Services;

public interface IEcsService
{
    Task<string> EnsureClusterExistsAsync(
        string clusterName,
        CancellationToken cancellationToken);
    Task<string> RegisterTaskDefinitionAsync(
      EcsTaskDefinitionInput input,
      CancellationToken cancellationToken);

    Task<string> EnsureServiceAsync(
  string clusterName,
  string serviceName,
  string taskDefinitionArn,
  int desiredCount,
  string targetGroupArn,
  string containerName,
  int containerPort,
  string[] subnetIds,
  string securityGroupId,
  CancellationToken cancellationToken);
Task<DeploymentHealthResult> WaitForServiceHealthyAsync(
    string clusterName,
    string serviceName,
    string targetGroupArn,
    int desiredCount,
    CancellationToken cancellationToken);
    Task<string> EnsureTargetGroupAsync(
    string targetGroupName,
    string vpcId,
    int containerPort,
    CancellationToken cancellationToken);

    Task<string> EnsureListenerRuleAsync(
    string listenerArn,
    string hostHeader,
    string targetGroupArn,
    CancellationToken cancellationToken);
    Task<string?> GetServiceFailureReasonAsync(
    string clusterName,
    string serviceName,
    CancellationToken cancellationToken);
    Task<string> GetLoadBalancerDnsNameAsync(
    string loadBalancerArn,
    CancellationToken cancellationToken);

    Task ConfigureAutoScalingAsync(
    string clusterName,
    string serviceName,
    int minimumInstances,
    int maximumInstances,
    CancellationToken cancellationToken);
  Task ConfigureCpuScalingPolicyAsync(
  string clusterName,
  string serviceName,
  double targetCpuUtilization,
  CancellationToken cancellationToken);
  Task EnsureDnsRecordAsync(
  string hostedZoneId,
  string hostName,
  string albDnsName,
  string albZoneId,
  CancellationToken cancellationToken);
}