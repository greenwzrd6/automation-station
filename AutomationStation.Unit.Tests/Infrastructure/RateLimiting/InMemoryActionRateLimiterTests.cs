using Microsoft.Extensions.Time.Testing;

using AutomationStation.Application.Abstractions;
using AutomationStation.Infrastructure.RateLimiting;
using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Unit.Tests.Infrastructure.RateLimiting
{
    public sealed class InMemoryActionRateLimiterTests
    {
        [Fact]
        public async Task WaitAsync_WhenUnderLimit_IsAllowed()
        {
            // Arrange
            var limiter = CreateLimiter();
            var key = CreateKey();

            // Act
            var action = limiter.WaitAsync(
                key,
                permittedActions: 1,
                window: _window,
                CancellationToken.None);

            // Assert
            Assert.True(action.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task WaitAsync_WhenOverLimit_IsBlocked()
        {
            // Arrange
            var limiter = CreateLimiter();
            var key = CreateKey();

            await limiter.WaitAsync(
                key,
                permittedActions: 1,
                window: _window,
                CancellationToken.None);

            // Act
            var blockedTask = limiter.WaitAsync(
                key,
                permittedActions: 1,
                window: _window,
                CancellationToken.None);

            // Assert
            Assert.False(blockedTask.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task WaitAsync_WhenOverLimitAndWindowExpires_IsAllowed()
        {
            // Arrange
            var limiter = CreateLimiter();
            var key = CreateKey();

            await limiter.WaitAsync(
                key,
                permittedActions: 1,
                window: _window,
                CancellationToken.None);

            // Act
            _timeProvider.Advance(_window);

            var task = limiter.WaitAsync(
                key,
                permittedActions: 1,
                window: _window,
                CancellationToken.None);

            // Assert
            Assert.True(task.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task WaitAsync_WithDifferentKeys_AreAllowed()
        {
            // Arrange
            var limiter = CreateLimiter();
            var key1 = CreateKey();
            var key2 = new ActionRateLimitKey(
                CompanyId: 1,
                ActionType: "CreatePlacement",
                TargetSystem: TargetSystem.Kanban);

            // Act
            var task1 = limiter.WaitAsync(
                key1,
                permittedActions: 1,
                window: _window,
                CancellationToken.None);

            var task2 = limiter.WaitAsync(
                key2,
                permittedActions: 1,
                window: _window,
                CancellationToken.None);

            // Assert
            Assert.True(task1.IsCompletedSuccessfully);
            Assert.True(task2.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task WaitAsync_WhenCancelled_ThrowsException()
        {
            // Arrange
            var limiter = CreateLimiter();
            var key = CreateKey();

            using var cts = new CancellationTokenSource();

            await limiter.WaitAsync(
                key,
                permittedActions: 1,
                window: _window,
                CancellationToken.None);

            // Act
            var task = limiter.WaitAsync(
                key,
                permittedActions: 1,
                window: _window,
                cts.Token);

            cts.Cancel();

            // Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await task);
        }

        private static readonly TimeSpan _window = TimeSpan.FromMinutes(1);

        private readonly FakeTimeProvider _timeProvider = new(DateTimeOffset.Parse("2026-01-01T12:00:00Z"));

        private InMemoryActionRateLimiter CreateLimiter()
        {
            return new InMemoryActionRateLimiter(_timeProvider);
        }

        private static ActionRateLimitKey CreateKey()
        {
            return new ActionRateLimitKey(
                CompanyId: 0,
                ActionType: "CreatePlacement",
                TargetSystem: TargetSystem.Kanban);
        }
    }
}