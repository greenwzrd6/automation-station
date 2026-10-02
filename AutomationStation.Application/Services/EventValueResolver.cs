using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;
using System.Text.Json;

namespace AutomationStation.Application.Services
{
    public sealed class EventValueResolver : IEventValueResolver
    {
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
                "source" => integrationEvent.Source,
                "companyid" => integrationEvent.CompanyId.ToString(),
                "eventid" => integrationEvent.EventId.ToString(),
                "correlationid" => integrationEvent.CorrelationId.ToString(),
                "causationeventid" =>
                    integrationEvent.CausationEventId?.ToString(),
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
