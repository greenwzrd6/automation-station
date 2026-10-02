using AutomationStation.Application.Abstractions;

namespace AutomationStation.Worker;

public sealed class CorrelationLoopCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<CorrelationLoopCleanupService> logger)
    : BackgroundService
{
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan MaxAge = TimeSpan.FromMinutes(10);

    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<CorrelationLoopCleanupService> _logger = logger;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var repository = scope.ServiceProvider
                    .GetRequiredService<ICorrelationLoopRepository>();

                await repository.CleanupOldEventsAsync(
                    MaxAge,
                    stoppingToken);

                _logger.LogInformation(
                    "Deleted correlation loop events older than {MaxAge} minutes.",
                    MaxAge.Minutes);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to clean up old correlation loop events.");
            }

            await Task.Delay(
                CleanupInterval,
                stoppingToken);
        }
    }
}
