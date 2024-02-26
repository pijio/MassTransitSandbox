using System.Security.Cryptography;
using MassTransit;
using MassTransitSandbox.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PasswordGenerator;

namespace MassTransitSandbox.Producer;

public class RemindersProducer : BackgroundService
{
    private readonly ILogger<NotificationProducer> _logger;
    private readonly IMessageScheduler _scheduler;
    private readonly Password _textGenerator = new (true, true, true, true, 32);

    public RemindersProducer(ILogger<NotificationProducer> logger, IMessageScheduler scheduler)
    {
        _logger = logger;
        _scheduler = scheduler;
    }

    private async Task PublishNotification(CancellationToken token)
    {
        var notification = CreateNotification();
        var delay = RandomNumberGenerator.GetInt32(1, 5);
        var sendTime = DateTime.Now.AddMinutes(delay);
        await _scheduler.SchedulePublish(sendTime, notification, token);
        _logger.LogInformation($"[RemindersProducer]: Опубликовано сообщение: {notification.Id}. Будет отправлено в {sendTime:g}");
    }
    
    private SendNotification CreateNotification()
    {
        return new SendNotification(NewId.NextGuid().ToString(), _textGenerator.Next(), NotificationsPriority.High);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await PublishNotification(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }
}