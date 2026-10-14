using System.Net;

using AutomationStation.Infrastructure.Integrations.Toj;

namespace AutomationStation.Unit.Tests.Infrastructure.Systems
{
    public class TojClientTests
    {
        [Fact]
        public async Task CreateTaskInSameRootAsync_WithValidEndpoint_SendsPostRequest()
        {
            // Arrange
            var handler = CreateHandler();

            using var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost")
            };

            var tc = new TojClient(httpClient);

            var sourceTaskId = Guid.NewGuid();

            // Act
            await tc.CreateTaskInSameRootAsync(
                sourceTaskId,
                "Test Task",
                Guid.NewGuid(),
                CancellationToken.None);

            // Assert
            Assert.Equal(HttpMethod.Post, handler.Method);
            Assert.Equal(
                $"/api/automation/tasks/{sourceTaskId}",
                handler.RequestUri?.AbsolutePath);
        }

        [Fact]
        public async Task UpdateTaskStatusAsync_WithValidEndpoint_SendsPatchRequest()
        {
            // Arrange
            var handler = CreateHandler();

            using var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost")
            };

            var tc = new TojClient(httpClient);

            var taskId = Guid.NewGuid();

            // Act
            await tc.UpdateTaskStatusAsync(
                taskId,
                1,
                Guid.NewGuid(),
                CancellationToken.None);

            // Assert
            Assert.Equal(HttpMethod.Patch, handler.Method);
            Assert.Equal(
                $"/api/automation/tasks/{taskId}/status",
                handler.RequestUri?.AbsolutePath);
        }

        [Fact]
        public async Task CreateTaskInSameRootAsync_WhenResponseIsUnsuccessful_ThrowsException()
        {
            // Arrange
            var handler = CreateHandler();

            handler.Response = new HttpResponseMessage(
                HttpStatusCode.InternalServerError);

            using var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost")
            };

            var tc = new TojClient(httpClient);

            // Act
            var exception = await Assert.ThrowsAsync<HttpRequestException>(
                () => tc.CreateTaskInSameRootAsync(
                    Guid.NewGuid(),
                    "Test Task",
                    Guid.NewGuid(),
                    CancellationToken.None));

            // Assert
            Assert.Contains("500", exception.Message);
        }

        [Fact]
        public async Task UpdateTaskStatusAsync_WhenResponseIsUnsuccessful_ThrowsException()
        {
            // Arrange
            var handler = CreateHandler();

            handler.Response = new HttpResponseMessage(
                HttpStatusCode.InternalServerError);

            using var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost")
            };

            var tc = new TojClient(httpClient);

            // Act
            var exception = await Assert.ThrowsAsync<HttpRequestException>(
                () => tc.UpdateTaskStatusAsync(
                    Guid.NewGuid(),
                    1,
                    Guid.NewGuid(),
                    CancellationToken.None));

            // Assert
            Assert.Contains("500", exception.Message);
        }

        [Fact]
        public async Task CreateTaskInSameRootAsync_WhenCalled_SendsHeaders()
        {
            // Arrange
            var handler = CreateHandler();

            using var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost")
            };

            var tc = new TojClient(httpClient);

            var executorId = Guid.NewGuid();

            // Act
            await tc.CreateTaskInSameRootAsync(
                Guid.NewGuid(),
                "Test Task",
                executorId,
                CancellationToken.None);

            // Assert
            Assert.Equal(
                executorId.ToString(),
                handler.Request.Headers.GetValues("Automation-Executor-Id").Single());
        }

        [Fact]
        public async Task UpdateTaskStatusAsync_WhenCalled_SendsHeaders()
        {
            // Arrange
            var handler = CreateHandler();

            using var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost")
            };

            var tc = new TojClient(httpClient);

            var executorId = Guid.NewGuid();

            // Act
            await tc.UpdateTaskStatusAsync(
                Guid.NewGuid(),
                1,
                executorId,
                CancellationToken.None);

            // Assert
            Assert.Equal(
                executorId.ToString(),
                handler.Request.Headers.GetValues("Automation-Executor-Id").Single());
        }

        private static TestHttpMessageHandler CreateHandler()
        {
            return new TestHttpMessageHandler();
        }
    }
}
