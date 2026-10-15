using AutomationStation.Application.Contracts;

namespace AutomationStation.Application.Abstractions
{
    /// <summary>
    /// Provides a TryGetValue-style resolver that retrieves a string value for a named field from an IntegrationEvent.
    /// </summary>
    /// <remarks>TryGetValue returns true and assigns the resolved string to value when the field is present;
    /// returns false and sets value to null when the field is absent. Implementations SHOULD be thread-safe and MUST
    /// NOT modify the supplied IntegrationEvent. Behavior such as case-sensitivity or normalization of field names is
    /// implementation-defined.</remarks>
    public interface IEventValueResolver
    {
        bool TryGetValue(
            IntegrationEvent integrationEvent,
            string field,
            out string? value);
    }
}
