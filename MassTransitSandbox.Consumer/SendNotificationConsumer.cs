using System.Security.Cryptography;
using MassTransit;
using MassTransitSandbox.Contracts;
using Microsoft.Extensions.Logging;

namespace MassTransitSandbox.Consumer;

public class SendNotificationConsumer : IConsumer<Batch<SendNotification>>
{
    public async Task Consume(ConsumeContext<Batch<SendNotification>> context)
    {
        var logger = context.GetServiceOrCreateInstance<ILogger>();
        logger.LogInformation($"Получен пакет из {context.Message.Length} сообщений");
        foreach (var msg in context.Message)
        {
            logger.LogInformation(
                $"Сообщение - Id:{msg.Message.Id}, Текст:{msg.Message.Message}, Дата публикации {msg.Message.Date:g}\n");
            if (RandomNumberGenerator.GetInt32(0, 15) == 15)
                throw new OperationCanceledException("Что-то поломалось...");
            await Task.Delay(TimeSpan.FromSeconds(2));
            logger.LogInformation("Отправка прошла успешно!");
        }
    }
}