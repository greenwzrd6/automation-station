namespace Automation.Core.Automations
{
    public sealed record Then(
        string Type,
        IReadOnlyDictionary<string, string> Parameters);
}
