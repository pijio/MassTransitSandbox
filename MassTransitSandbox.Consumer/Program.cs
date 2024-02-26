using MassTransit;
using MassTransitSandbox.Consumer;
using MassTransitSandbox.Contracts;
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
            
            services.AddMassTransit(x =>
            {
                x.SetSnakeCaseEndpointNameFormatter();
                x.AddConsumer<SendNotificationConsumer>();
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitConfig.Host, "/", h => {
                        h.Username(rabbitConfig.Username);
                        h.Password(rabbitConfig.Password);
                    });

                    cfg.Message<SendNotification>(m => m.SetEntityName(recEndp.ExchangeName));
                    
                    cfg.ReceiveEndpoint(recEndp.QueueName, e =>
                    {
                        e.ConfigureConsumeTopology = false;
                        e.ConfigureConsumer<SendNotificationConsumer>(context);
                        e.Bind<SendNotification>(p =>
                        {
                            p.ExchangeType = "topic";
                            p.RoutingKey = NotificationsPriority.High;
                        });
                    });
                });
            });
        });