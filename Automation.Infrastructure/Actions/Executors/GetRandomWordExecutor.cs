using Automation.Application.Models;
using Automation.Core.Automations;
using Automation.Infrastructure.Integrations.Planning;
using Automation.Infrastructure.Integrations.Word;

namespace Automation.Infrastructure.Actions.Executors
{
    public sealed class GetRandomWordExecutor(
    WordClient dictionaryClient,
    PlanningClient planningClient)
    : IActionHandler<PlacementActionContext>
    {
        public string ActionType => "GetRandomWord";

        public async Task ExecuteAsync(
            Then then,
            PlacementActionContext context,
            CancellationToken cancellationToken)
        {
            var result =
                await dictionaryClient.GetRandomWordAsync(
                    cancellationToken);

            await planningClient.PublishRandomWordAsync(
                result.Word,
                result.Definition,
                context.CausationEventId,
                cancellationToken);
        }
    }
}
