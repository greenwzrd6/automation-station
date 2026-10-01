using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;
using AutomationStation.Application.Events;
using AutomationStation.Application.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace AutomationStation.Worker;

public sealed class Worker(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<Worker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"]!,
            Port = configuration.GetValue<int>("RabbitMQ:Port", 5672),
            UserName = configuration["RabbitMQ:UserName"]!,
            Password = configuration["RabbitMQ:Password"]!,
            VirtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/"
        };

        var exchange = configuration["RabbitMQ:Exchange"]!;
        var queue = configuration["RabbitMQ:Queue"]!;

        logger.LogInformation(
            "Connecting to RabbitMQ: '{Host}:{Port}', User: '{User}', VirtualHost: '{VHost}'",
            factory.HostName,
            factory.Port,
            factory.UserName,
            factory.VirtualHost);

        await using var connection =
            await factory.CreateConnectionAsync(
                stoppingToken);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            exchange: exchange,
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: queue,
            exchange: exchange,
            routingKey: "placement.created",
            cancellationToken: stoppingToken);

        // await channel.QueueBindAsync(
        //     queue: queue,
        //     exchange: exchange,
        //     routingKey: "column.no-edge",
        //     cancellationToken: stoppingToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());

                var formattedJson = JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(json),
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                logger.LogInformation(
                    """
                    Received RabbitMQ message: 
                    {Message}
                    """,
                    formattedJson);

                using var document = JsonDocument.Parse(json);

                var eventId = document.RootElement
                    .GetProperty("eventId")
                    .GetGuid();

                var eventType = document.RootElement
                    .GetProperty("eventType")
                    .GetString();

                document.RootElement.TryGetProperty("correlationId", out var correlationElement);
                var correlationId = correlationElement.ValueKind == JsonValueKind.String
                    ? correlationElement.GetGuid()
                    : (Guid?)null;

                using var scope = scopeFactory.CreateScope();

                var processedMessages =
                    scope.ServiceProvider
                        .GetRequiredService<
                            IProcessedMessageRepository>();

                if (await processedMessages
                .HasProcessedAsync(
                    eventId,
                    stoppingToken))
                {
                    logger.LogInformation(
                        "Ignoring duplicate message '{EventId}'",
                        eventId);

                    await channel.BasicAckAsync(
                        args.DeliveryTag,
                        multiple: false,
                        cancellationToken:
                            stoppingToken);

                    return;
                }

                var blockers = scope.ServiceProvider.GetServices<IEventBlocker>();

                var context = new EventBlockerContext(
                    eventId,
                    eventType ?? string.Empty,
                    document.RootElement.GetProperty("source").GetString() ?? string.Empty,
                    correlationId,
                    null);

                foreach (var blocker in blockers)
                {
                    if (await blocker.IsBlockedAsync(context, stoppingToken))
                    {
                        logger.LogWarning(
                            "Event '{EventId}' with correlationId '{CorrelationId}' blocked by '{Blocker}'",
                            eventId,
                            correlationId,
                            blocker.GetType().Name);

                        await channel.BasicAckAsync(
                            args.DeliveryTag,
                            multiple: false,
                            cancellationToken: stoppingToken);

                        return;
                    }
                }

                switch (eventType)
                {
                    case "PlacementCreated":
                        await HandlePlacementCreatedAsync(
                            json,
                            scope.ServiceProvider,
                            stoppingToken);
                        break;

                    // case "ColumnHasNoEdge":
                    //     await HandleColumnHasNoEdgeAsync(
                    //         json,
                    //         scope.ServiceProvider,
                    //         stoppingToken);
                    //     break;

                    default:
                        throw new NotSupportedException(
                            $"Unsupported event type: {eventType}");
                }

                await processedMessages
                    .MarkProcessedAsync(
                        eventId,
                        stoppingToken);

                await channel.BasicAckAsync(
                    args.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);

                logger.LogInformation(
                    "Successfully processed event {EventType}",
                    eventType);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                // Worker is shutting down.
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Failed to process RabbitMQ message.");

                await channel.BasicNackAsync(
                    args.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: queue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        logger.LogInformation(
            "Listening for integration events...");

        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
    }

    //Move out of worker later
    private static async Task HandlePlacementCreatedAsync(
    string json,
    IServiceProvider serviceProvider,
    CancellationToken cancellationToken)
    {
        var integrationEvent =
            JsonSerializer.Deserialize<
                IntegrationEvent<PlacementCreatedPayload>>(
                    json,
                    JsonSerializerOptions.Web)
            ?? throw new JsonException(
                "Could not deserialize PlacementCreated event.");

        var handler = serviceProvider
            .GetRequiredService<
                ProcessPlacementCreatedEventHandler>();

        await handler.HandleAsync(
            integrationEvent,
            cancellationToken);
    }

    //Move out of worker later
    // private static async Task HandleColumnHasNoEdgeAsync(
    // string json,
    // IServiceProvider serviceProvider,
    // CancellationToken cancellationToken)
    // {
    //     var integrationEvent =
    //         JsonSerializer.Deserialize<
    //             IntegrationEvent<ColumnHasNoEdgePayload>>(
    //                 json,
    //                 JsonSerializerOptions.Web)
    //         ?? throw new JsonException(
    //             "Could not deserialize ColumnHasNoEdge event.");

    //     var handler = serviceProvider
    //         .GetRequiredService<
    //             ProcessColumnHasNoEdgeEventHandler>();

    //     await handler.HandleAsync(
    //         integrationEvent,
    //         cancellationToken);
    // }

    //Test

    //private async Task TestAutomationExecutionRepositoryAsync(
    //IServiceScopeFactory scopeFactory,
    //CancellationToken cancellationToken)
    //{
    //    using var scope = scopeFactory.CreateScope();

    //    var executionRepository =
    //        scope.ServiceProvider
    //            .GetRequiredService<IAutomationExecutionRepository>();

    //    var automationId =
    //        Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA");

    //    var eventId =
    //        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaac");

    //    var firstExecutionId =
    //        await executionRepository.GetOrCreateAsync(
    //            automationId,
    //            eventId,
    //            cancellationToken);

    //    var secondExecutionId =
    //        await executionRepository.GetOrCreateAsync(
    //            automationId,
    //            eventId,
    //            cancellationToken);

    //    logger.LogInformation(
    //        "First execution id: {ExecutionId}",
    //        firstExecutionId);

    //    logger.LogInformation(
    //        "Second execution id: {ExecutionId}",
    //        secondExecutionId);

    //    logger.LogInformation(
    //        "Same execution id: {Same}",
    //        firstExecutionId == secondExecutionId);
    //}
}