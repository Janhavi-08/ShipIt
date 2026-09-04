using Amazon.ApplicationAutoScaling;
using Amazon.ApplicationAutoScaling.Model;
using Amazon.ECS;
using Amazon.ECS.Model;
using Amazon.ElasticLoadBalancingV2;
using Amazon.ElasticLoadBalancingV2.Model;
using Amazon.Route53;
using Amazon.Route53.Model;

namespace ShipIt.DeploymentWorker.Services;

public class EcsService : IEcsService
{
    private readonly IAmazonECS _ecsClient;
    private readonly ILogger<EcsService> _logger;
    private readonly IAmazonElasticLoadBalancingV2 _elbv2Client;
    private readonly IConfiguration _configuration;
    private readonly IAmazonApplicationAutoScaling _autoScalingClient;
    private readonly IAmazonRoute53 _route53Client;
        public EcsService(
        IAmazonECS ecsClient,
        IAmazonElasticLoadBalancingV2 elbv2Client,
        IAmazonApplicationAutoScaling autoScalingClient,
        IConfiguration configuration,
IAmazonRoute53 amazonRoute53,
        ILogger<EcsService> logger)
    {
        _ecsClient = ecsClient;
        _elbv2Client = elbv2Client;
        _autoScalingClient = autoScalingClient;
        _configuration = configuration;
        _logger = logger;
        _route53Client = amazonRoute53;
    }

    public async Task<string> EnsureClusterExistsAsync(
        string clusterName,
        CancellationToken cancellationToken)
    {
        var response =
            await _ecsClient.DescribeClustersAsync(
                new DescribeClustersRequest
                {
                    Clusters =
                    [
                        clusterName
                    ]
                },
                cancellationToken);

        var cluster = response.Clusters.FirstOrDefault();

        _logger.LogInformation(
        "ECS cluster lookup: Name={ClusterName}, Found={Found}, Status={Status}, Arn={Arn}",
        clusterName,
        cluster != null,
        cluster?.Status,
        cluster?.ClusterArn);

        if (cluster != null &&
               cluster.Status == "ACTIVE")
        {
            _logger.LogInformation(
                "ECS cluster {ClusterName} is ACTIVE.",
                clusterName);


            return cluster.ClusterArn;
        }
        _logger.LogInformation(
           "ECS cluster {ClusterName} does not have an ACTIVE cluster. Creating a new cluster.",
           clusterName);

        // if (cluster != null)
        // {
        //     _logger.LogInformation(
        //         "ECS cluster {ClusterName} already exists.",
        //         clusterName);

        //     return cluster.ClusterArn;
        // }

        // _logger.LogInformation(
        //     "Creating ECS cluster {ClusterName}.",
        //     clusterName);

        var createResponse =
            await _ecsClient.CreateClusterAsync(
                new CreateClusterRequest
                {
                    ClusterName = clusterName,

                    Tags =
                    [
                        new Amazon.ECS.Model.Tag
                        {
                            Key = "ManagedBy",
                            Value = "ShipIt"
                        }
                    ]
                },
                cancellationToken);

        _logger.LogInformation(
            "ECS cluster {ClusterName} created successfully.",
            clusterName);

        return createResponse.Cluster.ClusterArn;
    }
    public async Task<string> RegisterTaskDefinitionAsync(
        EcsTaskDefinitionInput input,
        CancellationToken cancellationToken)
    {
        var request = new RegisterTaskDefinitionRequest
        {
            Family = input.FamilyName,

            RequiresCompatibilities =
            [
                Compatibility.FARGATE
            ],

            NetworkMode = NetworkMode.Awsvpc,

            Cpu = input.Cpu.ToString(),

            Memory = input.Memory.ToString(),

            ExecutionRoleArn = input.ExecutionRoleArn,

            ContainerDefinitions =
            [
                new ContainerDefinition
                    {
                        Name = "application",

                        Image = input.ImageUri,

                        Essential = true,

                        PortMappings =
                        [
                            new PortMapping
                            {
                                ContainerPort = input.ContainerPort,

                                Protocol = TransportProtocol.Tcp
                            }
                        ],

                        Environment =
                            input.EnvironmentVariables
                                .Select(x =>
                                    new Amazon.ECS.Model.KeyValuePair
                                    {
                                        Name = x.Name,
                                        Value = x.Value
                                    })
                                .ToList(),

                        Secrets =
                            input.Secrets
                                .Select(x =>
                                    new Amazon.ECS.Model.Secret
                                    {
                                        Name = x.Name,
                                        ValueFrom = x.ValueFrom
                                    })
                                .ToList(),
                        LogConfiguration = new LogConfiguration
                            {
                                LogDriver = LogDriver.Awslogs,

                                Options = new Dictionary<string, string>
                                {
                                    ["awslogs-group"] = "/shipit/ecs",
                                    ["awslogs-region"] = _configuration["AWS:Region"]!,
                                    ["awslogs-stream-prefix"] = "ecs"
                                }
                            }

                    }
            ]
        };

        var response =
            await _ecsClient.RegisterTaskDefinitionAsync(
                request,
                cancellationToken);

        var arn = response.TaskDefinition.TaskDefinitionArn;

        _logger.LogInformation(
            "Registered ECS task definition {TaskDefinitionArn}.",
            arn);

        return arn;
    }
    public async Task<string> EnsureServiceAsync(
    string clusterName,
    string serviceName,
    string taskDefinitionArn,
    int desiredCount,
    string targetGroupArn,
    string containerName,
    int containerPort,
    string[] subnetIds,
    string securityGroupId,
    CancellationToken cancellationToken)
    {
        var describeResponse =
            await _ecsClient.DescribeServicesAsync(
                new DescribeServicesRequest
                {
                    Cluster = clusterName,
                    Services = [serviceName]
                },
                cancellationToken);

        var existingService =
            describeResponse.Services
                .FirstOrDefault();

        if (existingService != null)
        {
            _logger.LogInformation(
                "ECS service {ServiceName} already exists. Updating it.",
                serviceName);

            var updateResponse =
                await _ecsClient.UpdateServiceAsync(
                    new UpdateServiceRequest
                    {
                        Cluster = clusterName,
                        Service = serviceName,
                        TaskDefinition = taskDefinitionArn,
                        DesiredCount = desiredCount
                    },
                    cancellationToken);

            return updateResponse.Service.ServiceArn;
        }

        _logger.LogInformation(
            "Creating ECS service {ServiceName}.",
            serviceName);

        var createResponse =
            await _ecsClient.CreateServiceAsync(
                new CreateServiceRequest
                {
                    Cluster = clusterName,

                    ServiceName = serviceName,

                    TaskDefinition =
                        taskDefinitionArn,

                    DesiredCount =
                        desiredCount,

                    LaunchType =
                        LaunchType.FARGATE,

                    NetworkConfiguration =
                        new NetworkConfiguration
                        {
                            AwsvpcConfiguration =
                                new AwsVpcConfiguration
                                {
                                    Subnets =
                                        subnetIds.ToList(),

                                    SecurityGroups =
                                    [
                                        securityGroupId
                                    ],

                                    AssignPublicIp = AssignPublicIp.DISABLED
                                }
                        },

                    LoadBalancers =
                    [
                        new Amazon.ECS.Model.LoadBalancer
                    {
                        TargetGroupArn = targetGroupArn,

                        ContainerName =  containerName,

                        ContainerPort = containerPort
                    }
                    ]
                },
                cancellationToken);

        _logger.LogInformation(
            "ECS service {ServiceName} created successfully.",
            serviceName);



        var serviceResponse =
            await _ecsClient.DescribeServicesAsync(
                new DescribeServicesRequest
                {
                    Cluster = clusterName,
                    Services = [serviceName]
                },
                cancellationToken);
        var service = serviceResponse.Services.FirstOrDefault();

        if (service == null)
        {
            throw new InvalidOperationException(
                $"ECS service '{serviceName}' was not found.");
        }

        return createResponse.Service.ServiceArn;

    }
    public async Task<DeploymentHealthResult> WaitForServiceHealthyAsync(
    string clusterName,
    string serviceName,
    string targetGroupArn,
    int desiredCount,
    CancellationToken cancellationToken)
    {
        var timeout = TimeSpan.FromMinutes(5);
        var interval = TimeSpan.FromSeconds(10);

        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < timeout)
        {
            var response =
                await _ecsClient.DescribeServicesAsync(
                    new DescribeServicesRequest
                    {
                        Cluster = clusterName,
                        Services = [serviceName]
                    },
                    cancellationToken);

            var service =
                response.Services.FirstOrDefault();

            if (service == null)
            {
                _logger.LogWarning(
                    "ECS service {ServiceName} was not found.",
                    serviceName);

                return new DeploymentHealthResult
                {
                    IsHealthy = false,
                    ErrorMessage =
                        $"ECS service '{serviceName}' was not found."
                };
            }

            _logger.LogInformation(
                "ECS service {ServiceName}: Desired={Desired}, Running={Running}, Pending={Pending}",
                serviceName,
                service.DesiredCount,
                service.RunningCount,
                service.PendingCount);
            if (service.RunningCount == 0 &&
                    service.PendingCount == 0)
            {
                var failureReason =
                    await GetServiceFailureReasonAsync(
                        clusterName,
                        serviceName,
                        cancellationToken);

                if (!string.IsNullOrWhiteSpace(failureReason))
                {
                    _logger.LogError(
                        "ECS service {ServiceName} failed: {FailureReason}",
                        serviceName,
                        failureReason);

                    return new DeploymentHealthResult
                    {
                        IsHealthy = false,
                        ErrorMessage = failureReason
                    };
                }
            }

            if (service.RunningCount >= desiredCount)
            {
                var targetHealthResponse =
                    await _elbv2Client.DescribeTargetHealthAsync(
                        new DescribeTargetHealthRequest
                        {
                            TargetGroupArn = targetGroupArn
                        },
                        cancellationToken);

                var healthyTargets =
                    targetHealthResponse.TargetHealthDescriptions
                        .Count(x =>
                            x.TargetHealth?.State ==
                            TargetHealthStateEnum.Healthy);

                _logger.LogInformation(
                    "ALB target health for {TargetGroupArn}: Healthy={HealthyTargets}, Desired={DesiredTargets}",
                    targetGroupArn,
                    healthyTargets,
                    desiredCount);
                foreach (var target in
                        targetHealthResponse.TargetHealthDescriptions)
                {
                    _logger.LogInformation(
                        "ALB target {TargetId}: State={State}, Reason={Reason}, Description={Description}",
                        target.Target.Id,
                        target.TargetHealth.State,
                        target.TargetHealth.Reason,
                        target.TargetHealth.Description);
                }

                if (healthyTargets >= desiredCount)
                {
                    _logger.LogInformation(
                        "ECS service {ServiceName} is healthy behind the ALB.",
                        serviceName);

                    return new DeploymentHealthResult
                    {
                        IsHealthy = true
                    };
                }
            }

            await System.Threading.Tasks.Task.Delay(
                interval,
                cancellationToken);
        }

        _logger.LogWarning(
            "ECS service {ServiceName} did not reach the desired running count within the timeout.",
            serviceName);
        _logger.LogError(
            "Deployment health check timed out for ECS service {ServiceName}.",
            serviceName);
        return new DeploymentHealthResult
        {
            IsHealthy = false,
            ErrorMessage =
                $"ECS service '{serviceName}' did not become healthy within the timeout."
        };
    }
    public async Task<string> EnsureTargetGroupAsync(
        string targetGroupName,
        string vpcId,
        int containerPort,
        CancellationToken cancellationToken)
    {
        try
        {
            var existingResponse =
                await _elbv2Client.DescribeTargetGroupsAsync(
                    new DescribeTargetGroupsRequest
                    {
                        Names = [targetGroupName]
                    },
                    cancellationToken);

            var existingTargetGroup =
                existingResponse.TargetGroups.FirstOrDefault();

            if (existingTargetGroup != null)
            {
                _logger.LogInformation(
                    "Target group {TargetGroupName} already exists.",
                    targetGroupName);

                return existingTargetGroup.TargetGroupArn;
            }
        }
        catch (TargetGroupNotFoundException)
        {
            // Target group doesn't exist. We'll create it below.
        }

        _logger.LogInformation(
            "Creating target group {TargetGroupName}.",
            targetGroupName);

        var createResponse =
            await _elbv2Client.CreateTargetGroupAsync(
                new CreateTargetGroupRequest
                {
                    Name = targetGroupName,

                    Protocol = ProtocolEnum.HTTP,

                    Port = containerPort,

                    VpcId = vpcId,

                    TargetType = TargetTypeEnum.Ip,

                    HealthCheckProtocol = ProtocolEnum.HTTP,

                    HealthCheckPath = "/health",

                    HealthCheckPort = "traffic-port",

                    HealthCheckEnabled = true
                },
                cancellationToken);

        var targetGroup =
            createResponse.TargetGroups.First();

        _logger.LogInformation(
            "Target group {TargetGroupName} created. ARN: {TargetGroupArn}",
            targetGroupName,
            targetGroup.TargetGroupArn);

        return targetGroup.TargetGroupArn;
    }

    public async Task<string> EnsureListenerRuleAsync(
        string listenerArn,
        string hostHeader,
        string targetGroupArn,
        CancellationToken cancellationToken)
    {
        var existingRulesResponse =
            await _elbv2Client.DescribeRulesAsync(
                new DescribeRulesRequest
                {
                    ListenerArn = listenerArn
                },
                cancellationToken);

        var existingRule =
            existingRulesResponse.Rules
                .FirstOrDefault(rule =>
                    rule.Conditions != null &&
                    rule.Conditions.Any(condition =>
                        condition.Field == "host-header" &&
                        condition.HostHeaderConfig?.Values?
                            .Contains(hostHeader) == true));

        if (existingRule != null)
        {
            _logger.LogInformation(
                "Listener rule for {HostHeader} already exists.",
                hostHeader);

            return existingRule.RuleArn;
        }

        _logger.LogInformation(
            "Creating listener rule for {HostHeader}.",
            hostHeader);

        var response =
            await _elbv2Client.CreateRuleAsync(
                new CreateRuleRequest
                {
                    ListenerArn = listenerArn,

                    Priority = await GetNextListenerRulePriorityAsync(
                                    listenerArn,
                                    cancellationToken),

                    Conditions =
                    [
                        new RuleCondition
                    {
                        Field = "host-header",

                        HostHeaderConfig =
                            new HostHeaderConditionConfig
                            {
                                Values =
                                [
                                    hostHeader
                                ]
                            }
                    }
                    ],

                    Actions =
                    [
                        new Amazon.ElasticLoadBalancingV2.Model.Action
                    {
                        Type = ActionTypeEnum.Forward,

                        TargetGroupArn =
                            targetGroupArn
                    }
                    ]
                },
                cancellationToken);

        var rule =
            response.Rules.First();

        _logger.LogInformation(
            "Listener rule created for {HostHeader}. ARN: {RuleArn}",
            hostHeader,
            rule.RuleArn);

        return rule.RuleArn;
    }

    public async Task<string?> GetServiceFailureReasonAsync(
        string clusterName,
        string serviceName,
        CancellationToken cancellationToken)
    {
        var serviceResponse =
            await _ecsClient.DescribeServicesAsync(
                new DescribeServicesRequest
                {
                    Cluster = clusterName,
                    Services = [serviceName]
                },
                cancellationToken);

        var service =
            serviceResponse.Services.FirstOrDefault();

        if (service == null)
        {
            return $"ECS service '{serviceName}' was not found.";
        }

        var taskResponse =
            await _ecsClient.ListTasksAsync(
                new ListTasksRequest
                {
                    Cluster = clusterName,
                    ServiceName = serviceName,
                    DesiredStatus = DesiredStatus.STOPPED,
                    MaxResults = 1
                },
                cancellationToken);

        var taskArn =
            taskResponse.TaskArns.FirstOrDefault();

        if (taskArn != null)
        {
            var taskDetails =
                await _ecsClient.DescribeTasksAsync(
                    new DescribeTasksRequest
                    {
                        Cluster = clusterName,
                        Tasks = [taskArn]
                    },
                    cancellationToken);

            var task =
                taskDetails.Tasks.FirstOrDefault();

            if (task != null)
            {
                if (!string.IsNullOrWhiteSpace(
                        task.StoppedReason))
                {
                    return task.StoppedReason;
                }

                var container =
                    task.Containers.FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(
                        container?.Reason))
                {
                    return container.Reason;
                }

                if (container?.ExitCode != null)
                {
                    return
                        $"Container exited with code {container.ExitCode}.";
                }
            }
        }

        // Fall back to ECS service events.
        var latestEvent =
            service.Events.FirstOrDefault();

        return latestEvent?.Message;
    }
    public async Task<string> GetLoadBalancerDnsNameAsync(
        string loadBalancerArn,
        CancellationToken cancellationToken)
    {
        var response =
            await _elbv2Client.DescribeLoadBalancersAsync(
                new DescribeLoadBalancersRequest
                {
                    LoadBalancerArns = [loadBalancerArn]
                },
                cancellationToken);

        var loadBalancer =
            response.LoadBalancers.FirstOrDefault();

        if (loadBalancer == null)
        {
            throw new InvalidOperationException(
                $"Load balancer '{loadBalancerArn}' was not found.");
        }

        return loadBalancer.DNSName;
    }
    public async System.Threading.Tasks.Task ConfigureAutoScalingAsync(
        string clusterName,
        string serviceName,
        int minimumInstances,
        int maximumInstances,
        CancellationToken cancellationToken)
    {
        var resourceId =
            $"service/{clusterName}/{serviceName}";

        await _autoScalingClient.RegisterScalableTargetAsync(
            new RegisterScalableTargetRequest
            {
                ServiceNamespace =
                    ServiceNamespace.Ecs,

                ResourceId =
                    resourceId,

                ScalableDimension =
                    ScalableDimension.EcsServiceDesiredCount,

                MinCapacity =
                    minimumInstances,

                MaxCapacity =
                    maximumInstances
            },
            cancellationToken);

        _logger.LogInformation(
            "Configured ECS Auto Scaling for {ServiceName}: Min={Min}, Max={Max}",
            serviceName,
            minimumInstances,
            maximumInstances);

    }
    public async System.Threading.Tasks.Task ConfigureCpuScalingPolicyAsync(
        string clusterName,
        string serviceName,
        double targetCpuUtilization,
        CancellationToken cancellationToken)
    {
        var resourceId =
            $"service/{clusterName}/{serviceName}";

        await _autoScalingClient.PutScalingPolicyAsync(
            new PutScalingPolicyRequest
            {
                ServiceNamespace = ServiceNamespace.Ecs,

                ResourceId = resourceId,

                ScalableDimension =
                    ScalableDimension.EcsServiceDesiredCount,

                PolicyName =
                    $"shipit-{serviceName}-cpu-scaling",

                PolicyType =
                    PolicyType.TargetTrackingScaling,

                TargetTrackingScalingPolicyConfiguration =
                    new TargetTrackingScalingPolicyConfiguration
                    {
                        TargetValue = targetCpuUtilization,

                        PredefinedMetricSpecification =
                            new PredefinedMetricSpecification
                            {
                                PredefinedMetricType =
                                    MetricType.ECSServiceAverageCPUUtilization
                            },

                        ScaleInCooldown =
                            60,

                        ScaleOutCooldown =
                            60
                    }
            },
            cancellationToken);

        _logger.LogInformation(
            "Configured CPU Auto Scaling for {ServiceName}. Target CPU={TargetCpu}%",
            serviceName,
            targetCpuUtilization);
    }


    private async Task<int> GetNextListenerRulePriorityAsync(
        string listenerArn,
        CancellationToken cancellationToken)
    {
        var response =
            await _elbv2Client.DescribeRulesAsync(
                new DescribeRulesRequest
                {
                    ListenerArn = listenerArn
                },
                cancellationToken);

        var priorities =
            response.Rules
                .Where(rule => rule.Priority != "default")
                .Select(rule => int.TryParse(
                    rule.Priority,
                    out var priority)
                        ? priority
                        : 0)
                .Where(priority => priority > 0);

        return priorities.Any()
            ? priorities.Max() + 1
            : 1;
    }
    public async System.Threading.Tasks.Task EnsureDnsRecordAsync(
        string hostedZoneId,
        string hostName,
        string albDnsName,
        string albZoneId,
        CancellationToken cancellationToken)
    {
        await _route53Client.ChangeResourceRecordSetsAsync(
            new ChangeResourceRecordSetsRequest
            {
                HostedZoneId = hostedZoneId,

                ChangeBatch = new ChangeBatch
                {
                    Changes =
                    [
                        new Change
                    {
                        Action = ChangeAction.UPSERT,

                        ResourceRecordSet =
                            new ResourceRecordSet
                            {
                                Name = hostName,
                                Type = RRType.A,

                                AliasTarget =
                                    new AliasTarget
                                    {
                                        DNSName = albDnsName,
                                        HostedZoneId = albZoneId,
                                        EvaluateTargetHealth = true
                                    }
                            }
                    }
                    ]
                }
            },
            cancellationToken);
    }

}