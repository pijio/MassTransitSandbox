﻿namespace MassTransitSandbox.Contracts;

public class ReceiveEndpointOptions
{
    public string QueueName { get; set; }
    public int PrefetchCount { get; set; }
    public string ExchangeName { get; set; }
}