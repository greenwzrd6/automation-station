using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationStation.Infrastructure.Integrations.Toj.Requests
{
    public sealed record UpdateTojTaskStatusRequest
    (
        int StatusId);
}
