using System.Net.Http.Json;

namespace Automation.Infrastructure.Integrations.Word
{
    public sealed class WordClient(
    HttpClient httpClient)
    {
        public async Task<RandomWordResult> GetRandomWordAsync(
            CancellationToken cancellationToken)
        {
            var result = await httpClient.GetFromJsonAsync<RandomWordResult>(
                "/api/random-definition",
                cancellationToken);

            return result
                ?? throw new InvalidOperationException(
                    "Random word API returned no result.");
        }
    }
}
