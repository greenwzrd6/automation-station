using System.Collections.Concurrent;

using AutomationStation.Application.Abstractions;

namespace AutomationStation.Infrastructure.Limiting
{
    /// <summary>
    /// Rate limiter that admits a fixed number of actions per key within a fixed window, held in process memory.
    /// </summary>
    /// <remarks>Window state is kept in a <see cref="ConcurrentDictionary{TKey,TValue}"/> keyed by
    /// <see cref="ActionRateLimitKey"/> and is shared by every caller of this instance, so limits apply per process
    /// only: they are not coordinated between instances and are lost when the process restarts. A window opens when
    /// the key is first used (or when the previous window has elapsed, measured on the attempt that follows it) and
    /// admits up to the permitted number of actions; once exhausted, callers wait rather than fail. All timestamps
    /// come from the injected <see cref="TimeProvider"/>, which lets tests control the clock.</remarks>
    /// <param name="timeProvider">Clock used for window timestamps and for the delay between retries.</param>
    public sealed class InMemoryActionRateLimiter(
        TimeProvider timeProvider) : IActionRateLimiter
    {
        private readonly TimeProvider _timeProvider = timeProvider;

        private readonly ConcurrentDictionary<
            ActionRateLimitKey,
            WindowState> _windows = new();

        /// <summary>
        /// Waits until a slot is available for the given key, then consumes it.
        /// </summary>
        /// <remarks>When the current window is exhausted the call delays until the window ends and retries, so
        /// callers are throttled instead of rejected; slots are taken under a per-key lock, which makes concurrent
        /// callers of the same key serialize rather than exceed the limit. The delay uses the injected time
        /// provider, so it honours a test clock.</remarks>
        /// <param name="key">Identifies the rate-limit bucket: company, action type and target system.</param>
        /// <param name="permittedActions">Number of actions the bucket may admit per window; must be positive,
        /// otherwise no slot can ever be granted.</param>
        /// <param name="window">Length of the fixed window during which <paramref name="permittedActions"/> actions
        /// are allowed.</param>
        /// <param name="cancellationToken">A token used to cancel the wait.</param>
        /// <returns>A task that completes once a slot for the key has been acquired.</returns>
        /// <exception cref="OperationCanceledException">
        /// The wait is canceled through <paramref name="cancellationToken"/> before a slot was acquired.
        /// </exception>
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

        /// <summary>
        /// Tries to take one slot for the key, resetting the window first if it has elapsed; reports when to retry.
        /// </summary>
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

        /// <summary>
        /// Mutable per-key window state; callers must hold <see cref="SyncRoot"/> while reading or writing it.
        /// </summary>
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
}