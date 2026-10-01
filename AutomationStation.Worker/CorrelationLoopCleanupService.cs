using AutomationStation.Application.Abstractions;

namespace AutomationStation.Worker;

public sealed class CorrelationLoopCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<CorrelationLoopCleanupService> logger)
    : BackgroundService
{
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan MaxAge = TimeSpan.FromMinutes(10);

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();

                var repository = scope.ServiceProvider
                    .GetRequiredService<ICorrelationLoopRepository>();

                await repository.CleanupOldEventsAsync(
                    MaxAge,
                    stoppingToken);

                logger.LogInformation(
                    "Cleaned up old correlation loop events older than {MaxAge}.",
                    MaxAge);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Failed to clean up old correlation loop events.");
            }

            await Task.Delay(
                CleanupInterval,
                stoppingToken);
        }
    }
}
