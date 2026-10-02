using System.Text;
using System.Text.Json;
using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AutomationStation.Worker;

public sealed class Worker(
    ILogger<Worker> logger,
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMq:HostName"]!,
            Port = int.Parse(_configuration["RabbitMq:Port"]!),
            UserName = _configuration["RabbitMq:UserName"]!,
            Password = _configuration["RabbitMq:Password"]!,
            VirtualHost = _configuration["RabbitMq:VirtualHost"]!
        };

        var exchange =
            _configuration["RabbitMq:Exchange"]!;

        var queue =
            _configuration["RabbitMq:Queue"]!;

        _logger.LogInformation(
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
            routingKey: "#",
            cancellationToken: stoppingToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: stoppingToken);

        var consumer =
            new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var json =
                    Encoding.UTF8.GetString(
                        args.Body.ToArray());

                var integrationEvent =
                    JsonSerializer.Deserialize<IntegrationEvent>(
                        json,
                        JsonSerializerOptions.Web)
                    ?? throw new JsonException(
                        "Could not deserialize integration event.");

                _logger.LogInformation(
                    "Received event '{EventType}' with id '{EventId}'",
                    integrationEvent.EventType,
                    integrationEvent.EventId);

                using var scope =
                    _scopeFactory.CreateScope();

                var processedMessages =
                    scope.ServiceProvider
                        .GetRequiredService<
                            IProcessedMessageRepository>();

                if (await processedMessages.HasProcessedAsync(
                        integrationEvent.EventId,
                        stoppingToken))
                {
                    _logger.LogInformation(
                        "Ignoring duplicate message '{EventId}'",
                        integrationEvent.EventId);

                    await channel.BasicAckAsync(
                        args.DeliveryTag,
                        multiple: false,
                        cancellationToken:
                            stoppingToken);

                    return;
                }

                var blockers =
                    scope.ServiceProvider
                        .GetServices<IEventBlocker>();

                var blockerContext =
                    new EventBlockerContext(
                        integrationEvent.EventId,
                        integrationEvent.EventType,
                        integrationEvent.Source,
                        integrationEvent.CorrelationId,
                        integrationEvent.CausationEventId);

                foreach (var blocker in blockers)
                {
                    if (!await blocker.IsBlockedAsync(
                            blockerContext,
                            stoppingToken))
                    {
                        continue;
                    }

                    _logger.LogWarning(
                        "Event '{EventId}' with correlationId '{CorrelationId}' blocked by '{Blocker}'",
                        integrationEvent.EventId,
                        integrationEvent.CorrelationId,
                        blocker.GetType().Name);

                    await channel.BasicAckAsync(
                        args.DeliveryTag,
                        multiple: false,
                        cancellationToken:
                            stoppingToken);

                    return;
                }

                var processor =
                    scope.ServiceProvider
                        .GetRequiredService<
                            IAutomationProcessor>();

                await processor.ProcessAsync(
                    integrationEvent,
                    stoppingToken);

                await processedMessages.MarkProcessedAsync(
                    integrationEvent.EventId,
                    stoppingToken);

                await channel.BasicAckAsync(
                    args.DeliveryTag,
                    multiple: false,
                    cancellationToken:
                        stoppingToken);

                _logger.LogInformation(
                    "Successfully processed event '{EventType}'",
                    integrationEvent.EventType);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                // Hello
            }
           
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to process RabbitMQ message.");

                await channel.BasicNackAsync(
                    args.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken:
                        stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: queue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "Listening for integration events...");

        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
    }
}