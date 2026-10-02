using System.Collections.Concurrent;
using AutomationStation.Application.Abstractions;

namespace AutomationStation.Infrastructure.RateLimiting;

public sealed class InMemoryActionRateLimiter
    : IActionRateLimiter
{
    private readonly TimeProvider _timeProvider;

    private readonly ConcurrentDictionary<
        ActionRateLimitKey,
        WindowState> _windows = new();

    public InMemoryActionRateLimiter(
        TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public async Task WaitAsync(
        ActionRateLimitKey key,
        int permittedActions,
        TimeSpan window,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            var result = TryAcquire(
                key,
                permittedActions,
                window);

            if (result.IsAllowed)
            {
                return;
            }

            var now = _timeProvider.GetUtcNow();

            var delay =
                result.RetryAt!.Value - now;

            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(
                    delay,
                    _timeProvider,
                    cancellationToken);
            }
        }
    }

    private RateLimitResult TryAcquire(
        ActionRateLimitKey key,
        int permittedActions,
        TimeSpan window)
    {
        var now = _timeProvider.GetUtcNow();

        var state = _windows.GetOrAdd(
            key,
            _ => new WindowState(now));

        lock (state.SyncRoot)
        {
            var windowHasExpired =
                now - state.WindowStartedAt >= window;

            if (windowHasExpired)
            {
                state.WindowStartedAt = now;
                state.ExecutedActions = 0;
            }

            if (state.ExecutedActions >= permittedActions)
            {
                return new RateLimitResult(
                    IsAllowed: false,
                    RetryAt: state.WindowStartedAt + window);
            }

            state.ExecutedActions++;

            return new RateLimitResult(
                IsAllowed: true,
                RetryAt: null);
        }
    }

    private sealed record RateLimitResult(
        bool IsAllowed,
        DateTimeOffset? RetryAt);

    private sealed class WindowState
    {
        public WindowState(
            DateTimeOffset windowStartedAt)
        {
            WindowStartedAt = windowStartedAt;
        }

        public object SyncRoot { get; } = new();

        public DateTimeOffset WindowStartedAt { get; set; }

        public int ExecutedActions { get; set; }
    }
}