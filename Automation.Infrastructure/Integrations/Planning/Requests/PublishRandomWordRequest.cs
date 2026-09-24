using System;
using System.Collections.Generic;
using System.Text;

namespace Automation.Infrastructure.Integrations.Planning.Requests
{
    public sealed record PublishRandomWordRequest(
        string Word,
        string Definition);
}
