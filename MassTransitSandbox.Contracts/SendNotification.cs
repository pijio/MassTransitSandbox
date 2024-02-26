using MassTransit;
using MassTransit.Topology;

namespace MassTransitSandbox.Contracts;

[EntityName("notifications")]
public record SendNotification(string Id, string Message, string Priority)
{
    
}