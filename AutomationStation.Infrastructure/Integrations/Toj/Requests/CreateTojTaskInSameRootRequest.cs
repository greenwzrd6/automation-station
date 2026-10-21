using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationStation.Infrastructure.Integrations.Toj.Requests
{
    /// <summary>
    /// Record <c>CreateTojTaskInSameRootRequest</c> is the JSON body posted to the TOJ automation API to create a new
    /// task under the same root as an existing task.
    /// </summary>
    /// <remarks>Only the title travels in the body; the source task that determines the shared root is addressed by
    /// the request URL and the calling executor by the Automation-Executor-Id header, both set by the TOJ client.
    /// Value-based record equality applies.</remarks>
    /// <param name="Title">The title of the task to create.</param>
    public sealed record CreateTojTaskInSameRootRequest
    (
        string Title);
}
