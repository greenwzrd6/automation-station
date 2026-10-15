using AutomationStation.Infrastructure.Integrations.Toj.Requests;
using System.Net.Http.Json;

namespace AutomationStation.Infrastructure.Integrations.Toj;

/// <summary>
/// Client for interacting with the TOJ automation API (for now) to (just) create and update tasks.
/// </summary>
/// <remarks>Methods are asynchronous and throw HttpRequestException when the TOJ service returns a non-success
/// status code. Caller must provide a configured HttpClient and observe cancellation tokens.</remarks>
/// <param name="httpClient">HTTP client used to send requests to the TOJ automation endpoints.</param>
public sealed class TojClient(
    HttpClient httpClient)
{
    /// <summary>
    /// Creates a task under the same root as the specified source task by POSTing a JSON body to the automation API
    /// endpoint.
    /// </summary>
    /// <remarks>Sends a POST to api/automation/tasks/{sourceTaskId} with a JSON body containing Title and the
    /// Automation-Executor-Id header.</remarks>
    /// <param name="sourceTaskId">Source task identifier whose root will be used for the new task.</param>
    /// <param name="title">Title of the task to create.</param>
    /// <param name="executorId">Executor identifier sent as the Automation-Executor-Id header.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous HTTP operation.</param>
    /// <returns>A Task that represents the asynchronous operation.</returns>
    /// <exception cref="HttpRequestException">Thrown when the API returns a non-success status code; 
    /// the exception message includes the numeric status code,
    /// status phrase, and response body.</exception>
    public async Task CreateTaskInSameRootAsync(
        Guid sourceTaskId,
        string title,
        Guid executorId,
        CancellationToken cancellationToken)
    {
        var body = new CreateTojTaskInSameRootRequest(
            Title: title);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"api/automation/tasks/{sourceTaskId}");

        request.Headers.Add(
            "Automation-Executor-Id",
            executorId.ToString());

        request.Content = JsonContent.Create(body);

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"TOJ returned {(int)response.StatusCode} " +
                $"({response.StatusCode}). Body: {responseBody}");
        }
    }

    /// <summary>
    /// Updates the status of the specified task in the automation service.
    /// </summary>
    /// <remarks>Sends a PATCH request to api/automation/tasks/{taskId}/status with the new status in the JSON
    /// request body.</remarks>
    /// <param name="taskId">The identifier of the task to update.</param>
    /// <param name="statusId">The identifier of the status to apply to the task.</param>
    /// <param name="executorId">The identifier of the executor performing the update, 
    /// sent in the Automation-Executor-Id header.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="HttpRequestException">Thrown if the service responds with a non-success HTTP status code; 
    /// the exception message includes the status
    /// code and response body.</exception>
    public async Task UpdateTaskStatusAsync(
        Guid taskId,
        int statusId,
        Guid executorId,
        CancellationToken cancellationToken)
    {
        var body = new UpdateTojTaskStatusRequest(
            StatusId: statusId);

        using var request = new HttpRequestMessage(
            HttpMethod.Patch,
            $"api/automation/tasks/{taskId}/status");

        request.Headers.Add(
            "Automation-Executor-Id",
            executorId.ToString());

        request.Content = JsonContent.Create(body);

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"TOJ returned {(int)response.StatusCode} " +
                $"({response.StatusCode}). Body: {responseBody}");
        }
    }
}