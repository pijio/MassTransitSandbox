using System.Security.Cryptography;
using MassTransit;
using MassTransitSandbox.Contracts;
using Microsoft.Extensions.Logging;

namespace MassTransitSandbox.Consumer;

public class SendNotificationConsumer : IConsumer<Batch<SendNotification>>
{
    private readonly ILogger<SendNotificationConsumer> _logger;

    public SendNotificationConsumer(ILogger<SendNotificationConsumer> logger)
    {
        _logger = logger;
    }
    
    public async Task Consume(ConsumeContext<Batch<SendNotification>> context)
    {
        _logger.LogInformation($"Получен пакет из {context.Message.Length} сообщений");
        foreach (var msg in context.Message)
        {
            _logger.LogInformation(
                $"Сообщение - Id:{msg.Message.Id}, Текст:{msg.Message.Message}");
            if (RandomNumberGenerator.GetInt32(0, 15) == 15)
                throw new OperationCanceledException("Что-то поломалось...");
            await Task.Delay(TimeSpan.FromSeconds(2));
            _logger.LogInformation("Отправка прошла успешно!");
        }
    }
}