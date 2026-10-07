using AutomationStation.Infrastructure.Integrations.Toj.Requests;
using System.Net.Http.Json;

namespace AutomationStation.Infrastructure.Integrations.Toj;

public sealed class TojClient(
    HttpClient httpClient)
{
    public async Task CreateTaskInSameRootAsync(
        Guid sourceTaskId,
        string title,
        CancellationToken cancellationToken)
    {
        var request = new CreateTojTaskInSameRootRequest(
            Title: title);

        var response = await httpClient.PostAsJsonAsync(
            $"api/automation/tasks/{sourceTaskId}",
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
        CancellationToken cancellationToken)
    {
        var request = new UpdateTojTaskStatusRequest(
            StatusId: statusId);
        var response = await httpClient.PatchAsJsonAsync(
            $"api/automation/tasks/{taskId}/status",
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