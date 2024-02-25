using MassTransit;
using MassTransit.RabbitMqTransport.Topology;
using MassTransitSandbox.Contracts;
using MassTransitSandbox.Producer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = CreateHostBuilder(args).Build();
host.Run();


static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseSystemd()
        .ConfigureServices((hostContext, services) =>
        {
            var rabbitConfig = hostContext.Configuration
                .GetSection(nameof(RabbitMqOptions))
                .Get<RabbitMqOptions>();
            
            var recEndp = hostContext.Configuration
                .GetSection(nameof(ReceiveEndpointOptions))
                .Get<ReceiveEndpointOptions>();
            
            services.Configure<ReceiveEndpointOptions>(hostContext.Configuration
                .GetSection(nameof(ReceiveEndpointOptions)));
            
            if (rabbitConfig == null)
                throw new ArgumentException("Missing RabbitMq configuration section");
            
            if (recEndp == null)
                throw new ArgumentException("Missing Receive endpoint configuration section");
            
            services.AddMassTransit(x =>
            {
                x.SetSnakeCaseEndpointNameFormatter();
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitConfig.Host, "/", h => {
                        h.Username(rabbitConfig.Username);
                        h.Password(rabbitConfig.Password);
                    });
                    cfg.UseDelayedMessageScheduler();
                    cfg.Send<SendNotification>(cf =>
                    {
                        cf.UseRoutingKeyFormatter(f => f.Message.Priority);
                    });
                    cfg.Publish<SendNotification>(cf =>
                    {
                        cf.ExchangeType = "topic";
                    });
                    cfg.Message<SendNotification>(mt =>
                    {
                        mt.SetEntityName(recEndp.ExchangeName);
                    });
                });
            });
            services.AddHostedService<NotificationProducer>();
        });