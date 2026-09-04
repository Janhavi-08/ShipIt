using Amazon.SQS;
using Amazon.SQS.Model;
using ShipIt.DeploymentWorker.Services;
using System.Text.Json;

namespace ShipIt.DeploymentWorker;

public class Worker : BackgroundService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<Worker> _logger;
private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _queueUrl;

    public Worker(
    IServiceScopeFactory scopeFactory,
        IAmazonSQS sqsClient,
        IConfiguration configuration,
        ILogger<Worker> logger)
    {
        _sqsClient = sqsClient;
        _configuration = configuration;
        _logger = logger;
            _scopeFactory = scopeFactory;


        _queueUrl = configuration[
            "AWS:SQS:DeploymentQueueUrl"
        ] ?? throw new InvalidOperationException(
            "Deployment queue URL is not configured.");
            _logger.LogInformation(
        "Deployment Queue URL configured: {Configured}",
        !string.IsNullOrWhiteSpace(_queueUrl));
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Deployment worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var response = await _sqsClient.ReceiveMessageAsync(
    new ReceiveMessageRequest
    {
        QueueUrl = _queueUrl,
        MaxNumberOfMessages = 1,
        WaitTimeSeconds = 20,
        VisibilityTimeout = 300
    },
    stoppingToken);

if (response == null)
{
    _logger.LogWarning("SQS returned a null response.");
    continue;
}

if (response.Messages == null || response.Messages.Count == 0)
{
    continue;
}

foreach (var message in response.Messages)
{
    _logger.LogInformation(
        "Received SQS message {MessageId}",
        message.MessageId);

    _logger.LogInformation(
        "Message body: {MessageBody}",
        message.Body);

    var deploymentMessage =
    JsonSerializer.Deserialize<DeploymentMessage>(
        message.Body);

if (deploymentMessage == null)
{
    _logger.LogError(
        "Unable to deserialize deployment message {MessageId}.",
        message.MessageId);

    continue;
}

try
{
    using var scope = _scopeFactory.CreateScope();

    var processor =
        scope.ServiceProvider
            .GetRequiredService<DeploymentProcessor>();

    await processor.ProcessAsync(
        deploymentMessage,
        stoppingToken);

    await _sqsClient.DeleteMessageAsync(
        new DeleteMessageRequest
        {
            QueueUrl = _queueUrl,
            ReceiptHandle = message.ReceiptHandle
        },
        stoppingToken);

    _logger.LogInformation(
        "Deployment {DeploymentId} processed successfully.",
        deploymentMessage.DeploymentId);
}
catch (Exception ex)
{
    _logger.LogError(
        ex,
        "Failed to process deployment {DeploymentId}.",
        deploymentMessage.DeploymentId);

    // IMPORTANT:
    // Do NOT delete the SQS message.
    // SQS will make it visible again after
    // the visibility timeout.
}}
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while processing deployment queue.");
            }
        }

        _logger.LogInformation(
            "Deployment worker stopped.");
    }
}