using System.Text;
using System.Text.Json;
using AutomationStation.Application.Contracts;
using AutomationStation.Application.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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
            "Connecting to RabbitMQ: {Host}:{Port}, User: {User}, VirtualHost: {VHost}",
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

        await channel.QueueBindAsync(
            queue: queue,
            exchange: exchange,
            routingKey: "column.no-edge",
            cancellationToken: stoppingToken);

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
                var json = Encoding.UTF8.GetString(
                    args.Body.ToArray());

                logger.LogInformation(
                    "Received RabbitMQ message: {Message}",
                    json);

                using var document = JsonDocument.Parse(json);

                var eventType = document.RootElement
                    .GetProperty("eventType")
                    .GetString();

                using var scope = scopeFactory.CreateScope();

                switch (eventType)
                {
                    case "PlacementCreated":
                        await HandlePlacementCreatedAsync(
                            json,
                            scope.ServiceProvider,
                            stoppingToken);
                        break;

                    case "ColumnHasNoEdge":
                        await HandleColumnHasNoEdgeAsync(
                            json,
                            scope.ServiceProvider,
                            stoppingToken);
                        break;

                    default:
                        throw new NotSupportedException(
                            $"Unsupported event type: {eventType}");
                }

                await channel.BasicAckAsync(
                    args.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);

                logger.LogInformation(
                    "Successfully processed event type {EventType}",
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
    private static async Task HandleColumnHasNoEdgeAsync(
    string json,
    IServiceProvider serviceProvider,
    CancellationToken cancellationToken)
    {
        var integrationEvent =
            JsonSerializer.Deserialize<
                IntegrationEvent<ColumnHasNoEdgePayload>>(
                    json,
                    JsonSerializerOptions.Web)
            ?? throw new JsonException(
                "Could not deserialize ColumnHasNoEdge event.");

        var handler = serviceProvider
            .GetRequiredService<
                ProcessColumnHasNoEdgeEventHandler>();

        await handler.HandleAsync(
            integrationEvent,
            cancellationToken);
    }
}