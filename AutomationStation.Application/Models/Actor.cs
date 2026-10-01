using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationStation.Application.Models
{
    public record Actor(
        Guid Id,
        string Type);
}