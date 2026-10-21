using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationStation.Infrastructure.Integrations.Toj.Requests
{
    /// <summary>
    /// Request payload sent to the Toj system to change the status of a task.
    /// </summary>
    /// <param name="StatusId">The identifier of the status to set on the Toj task, as understood by the Toj
    /// status catalogue.</param>
    public sealed record UpdateTojTaskStatusRequest
    (
        int StatusId);
}
