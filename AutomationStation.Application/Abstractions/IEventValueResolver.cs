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
        /// <summary>
        /// Attempts to resolve a named field of the given integration event to a string.
        /// </summary>
        /// <param name="integrationEvent">The event to read; implementations must not modify it.</param>
        /// <param name="field">Name or dot-separated path of the field to resolve, for example "eventType",
        /// "actor.id" or "payload.columnId". Whitespace-only names must yield false.</param>
        /// <param name="value">When the method returns true, the resolved value; otherwise null.</param>
        /// <returns>true when the field was present and <paramref name="value"/> was assigned; false when the
        /// field is absent or cannot be read, in which case <paramref name="value"/> is set to null.</returns>
        bool TryGetValue(
            IntegrationEvent integrationEvent,
            string field,
            out string? value);
    }
}
