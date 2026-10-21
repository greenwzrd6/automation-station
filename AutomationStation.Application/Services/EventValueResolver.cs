using System.Text.Json;

using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;

namespace AutomationStation.Application.Services
{
    /// <summary>
    /// Resolves dotted field paths against an integration event to obtain string values for automation conditions.
    /// </summary>
    /// <remarks>Field paths are case-insensitive. Paths starting with "payload" are evaluated as a property path
    /// inside the event's JSON payload (for example, "payload.card.title"). Well-known envelope fields such as
    /// "eventtype", "source", "companyid", "eventid", "correlationid", "causationeventid", and "actor.id" or
    /// "actor.type" are resolved directly from the event. JSON values are converted to strings: numbers and complex
    /// values use their raw JSON text; booleans become "true" or "false"; a JSON null resolves to
    /// <see langword="null"/> while still reporting success.</remarks>
    public sealed class EventValueResolver : IEventValueResolver
    {
        /// <summary>
        /// Attempts to resolve the value of the given field path on the given integration event.
        /// </summary>
        /// <param name="integrationEvent">The integration event that supplies the envelope fields and JSON payload.
        /// </param>
        /// <param name="field">A dotted field path such as "eventtype", "actor.id", or "payload.some.property".
        /// Empty or whitespace-only paths are rejected.</param>
        /// <param name="value">When this method returns, contains the resolved value as a string, or
        /// <see langword="null"/> if the resolved JSON value is null; never set when the method returns
        /// <see langword="false"/>.</param>
        /// <returns><see langword="true"/> if the field was found and resolved; otherwise,
        /// <see langword="false"/>.</returns>
        public bool TryGetValue(
            IntegrationEvent integrationEvent,
            string field,
            out string? value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(field))
                return false;

            var parts = field.Split(
                '.',
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                return false;

            if (parts[0].Equals(
                "payload",
                StringComparison.OrdinalIgnoreCase))
            {
                return TryReadJsonPath(
                    integrationEvent.Payload,
                    parts.Skip(1),
                    out value);
            }

            value = parts[0].ToLowerInvariant() switch
            {
                "eventtype" => integrationEvent.EventType,

                "source" => integrationEvent.Source.ToString(),

                "companyid" => integrationEvent.CompanyId.ToString(),

                "eventid" => integrationEvent.EventId.ToString(),

                "correlationid" => integrationEvent.CorrelationId.ToString(),

                "causationeventid" => integrationEvent.CausationEventId?.ToString(),

                _ => null
            };

            if (value is not null)
                return true;

            if (parts[0].Equals(
                    "actor",
                    StringComparison.OrdinalIgnoreCase) &&
                parts.Length == 2)
            {
                value = parts[1].ToLowerInvariant() switch
                {
                    "id" => integrationEvent.Actor.Id.ToString(),

                    "type" => integrationEvent.Actor.Type,

                    _ => null
                };

                return value is not null;
            }

            return false;
        }

        /// <summary>
        /// Walks a property path through nested JSON objects, matching property names case-insensitively, and
        /// converts the terminal value to its string representation.
        /// </summary>
        private static bool TryReadJsonPath(
            JsonElement element,
            IEnumerable<string> path,
            out string? value)
        {
            value = null;

            var current = element;

            foreach (var propertyName in path)
            {
                if (current.ValueKind != JsonValueKind.Object)
                    return false;

                JsonElement? found = null;

                foreach (var property in current.EnumerateObject())
                {
                    if (property.Name.Equals(
                            propertyName,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        found = property.Value;
                        break;
                    }
                }

                if (found is null)
                    return false;

                current = found.Value;
            }

            value = current.ValueKind switch
            {
                JsonValueKind.String => current.GetString(),

                JsonValueKind.Number => current.GetRawText(),

                JsonValueKind.True => "true",

                JsonValueKind.False => "false",

                JsonValueKind.Null => null,

                _ => current.GetRawText()
            };

            return true;
        }
    }
}
