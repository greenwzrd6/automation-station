using AutomationStation.Application.Abstractions;
using AutomationStation.Infrastructure.RateLimiting;
using Microsoft.Extensions.Time.Testing;
using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Unit.Tests.Infrastructure.RateLimiting
{
    public sealed class InMemoryActionRateLimiterTests
    {
        [Fact]
        public async Task WaitAsync_ShouldWaitForNextWindow_WhenLimitIsExceeded()
        {
            // Arrange
            var startTime =
                new DateTimeOffset(
                    year: 2026,
                    month: 10,
                    day: 1,
                    hour: 12,
                    minute: 0,
                    second: 0,
                    offset: TimeSpan.Zero);

            var timeProvider =
                new FakeTimeProvider(startTime);

            var rateLimiter =
                new InMemoryActionRateLimiter(timeProvider);

            var key = new ActionRateLimitKey(
                CompanyId: 1,
                ActionType: "CreatePlacement",
                TargetSystem: TargetSystem.Kanban);

            const int permittedActions = 100;

            var window = TimeSpan.FromMinutes(1);

            // Act: de första 100 ska släppas igenom direkt.
            for (var i = 0; i < permittedActions; i++)
            {
                await rateLimiter.WaitAsync(
                    key,
                    permittedActions,
                    window,
                    CancellationToken.None);
            }

            // Den 101:a actionen ska börja vänta.
            var actionNumber101 = rateLimiter.WaitAsync(
                key,
                permittedActions,
                window,
                CancellationToken.None);

            // Ge den asynkrona metoden möjlighet att nå Task.Delay.
            await Task.Yield();

            // Assert: den 101:a actionen har inte fått fortsätta.
            Assert.False(actionNumber101.IsCompleted);

            // Act: flytta den fejkade tiden till nästa tidsfönster.
            timeProvider.Advance(window);

            await actionNumber101;

            // Assert: actionen släpptes igenom i det nya tidsfönstret.
            Assert.True(actionNumber101.IsCompletedSuccessfully);
        }
    }
}