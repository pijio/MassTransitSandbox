using MassTransit;
using MassTransitSandbox.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PasswordGenerator;

namespace MassTransitSandbox.Producer;

public class NotificationProducer : BackgroundService
{
    private readonly ILogger<NotificationProducer> _logger;
    private readonly IBus _bus;
    private readonly Password _textGenerator = new (true, true, true, true, 32);

    public NotificationProducer(ILogger<NotificationProducer> logger, IBus bus)
    {
        _logger = logger;
        _bus = bus;
    }

    private async Task PublishNotification(CancellationToken token)
    {
        var notification = CreateNotification();
        await _bus.Publish(notification, token);
        _logger.LogInformation($"[NotificationProducer]: Опубликовано сообщение: {notification.Id}");
    }
    
    private SendNotification CreateNotification()
    {
        return new SendNotification(NewId.NextGuid().ToString(), _textGenerator.Next(), DateTime.UtcNow, NotificationsPriority.High);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await PublishNotification(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }

    }
}