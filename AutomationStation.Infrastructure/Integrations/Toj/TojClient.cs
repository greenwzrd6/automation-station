using AutomationStation.Infrastructure.Integrations.Toj.Requests;
using System.Net.Http.Json;

namespace AutomationStation.Infrastructure.Integrations.Toj;

public sealed class TojClient(
    HttpClient httpClient)
{
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

        var response = await httpClient.SendAsync(
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

        var response = await httpClient.SendAsync(
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