using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using AutomationStation.Application.Contracts;
using AutomationStation.Application.Events;

namespace AutomationStation.Worker
{
    /// <summary>
    /// Background service that consumes integration events from RabbitMQ and feeds them to the automation event
    /// handler.
    /// </summary>
    /// <remarks>The service declares a durable topic exchange and a durable queue bound with the "#" routing key
    /// (both names come from configuration), sets a prefetch count of one and uses manual acknowledgements: a message
    /// is acked only after its handler completed successfully, and nacked without requeue when handling fails, so
    /// failed messages are logged and dropped instead of redelivered. Shutdown requested while a message is being
    /// handled neither acks nor nacks that delivery. Messages are handled one at a time by an
    /// <see cref="EventMessageHandler"/> resolved from a new scope per message.</remarks>
    /// <param name="logger">Logger used for connection, message and failure reporting.</param>
    /// <param name="configuration">Provides the required RabbitMq settings (HostName, Port, UserName, Password,
    /// VirtualHost, Exchange and Queue); missing values are not validated up front and will fail when read.</param>
    /// <param name="scopeFactory">Creates the dependency scope used to resolve <see cref="EventMessageHandler"/> for
    /// each delivered message.</param>
    public sealed class Worker(
        ILogger<Worker> logger,
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory)
        : BackgroundService
    {
    #pragma warning disable CS9124 // Parameter is captured into the state of the enclosing type and its value is also used to initialize a field, property, or event.
        private readonly ILogger<Worker> _logger = logger;
    #pragma warning restore CS9124 // Parameter is captured into the state of the enclosing type and its value is also used to initialize a field, property, or event.
        private readonly IConfiguration _configuration = configuration;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

        /// <summary>
        /// Connects to RabbitMQ, declares the exchange, queue and binding, and consumes messages until the host
        /// requests shutdown.
        /// </summary>
        /// <remarks>Each delivery is deserialized as an <see cref="IntegrationEvent"/> using web naming conventions
        /// and string-enum parsing, logged in indented form, handled, and then acknowledged. Any exception other than
        /// shutdown cancellation is logged and results in a negative acknowledgement without requeue. The returned task
        /// completes only when <paramref name="stoppingToken"/> is canceled, because the consumer is kept alive by an
        /// infinite delay afterwards.</remarks>
        /// <param name="stoppingToken">Token canceled when the host is stopping; observed by the connection, channel,
        /// consumer and handler operations.</param>
        /// <returns>A task that completes when the host requests the service to stop.</returns>
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

                    var options = new JsonSerializerOptions(JsonSerializerOptions.Web);

                    options.Converters.Add(
                        new JsonStringEnumConverter());

                    var integrationEvent =
                        JsonSerializer.Deserialize<IntegrationEvent>(
                            json,
                            options)
                        ?? throw new JsonException(
                            "Could not deserialize integration event.");

                    var formattedJson = JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(json),
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });

                    logger.LogInformation("""
            Received RabbitMQ message: 
            {Message}
            """,
                        formattedJson);

                    using var scope =
                        _scopeFactory.CreateScope();

                    var handler = scope.ServiceProvider
                        .GetRequiredService<EventMessageHandler>();

                    await handler.HandleAsync(
                        integrationEvent,
                        stoppingToken);

                    await channel.BasicAckAsync(
                        args.DeliveryTag,
                        multiple: false,
                        cancellationToken:
                            stoppingToken);
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
                        cancellationToken: stoppingToken);
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
}