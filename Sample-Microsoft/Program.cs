using System;
using System.Threading.Tasks;
using GreenPipes;
using MassTransit;
using MassTransit.Util;
using Microsoft.Extensions.DependencyInjection;
using Sample.Components;
using Sample.Contracts;

namespace Sample_Microsoft
{
    static class Program
    {
        static void Main()
        {
            var serviceProvider = ConfigureServiceProvider();

            var bus = serviceProvider.GetRequiredService<IBusControl>();

            try
            {
                bus.Start();
                try
                {
                    Console.WriteLine("Bus started, type 'exit' to exit.");

                    bool running = true;
                    while (running)
                    {
                        var input = Console.ReadLine();
                        switch (input)
                        {
                            case "exit":
                            case "quit":
                                running = false;
                                break;

                            case "submit":
                                TaskUtil.Await(() => Submit(serviceProvider));
                                break;
                        }
                    }
                }
                finally
                {
                    bus.Stop();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        static async Task Submit(IServiceProvider provider)
        {
            IBus bus = provider.GetRequiredService<IBus>();

            var orderId = NewId.NextGuid();

            await bus.Send<SubmitOrder>(new
            {
                OrderId = orderId,
                OrderDateTime = DateTimeOffset.Now
            }, Pipe.Execute<SendContext>(sendContext => sendContext.ConversationId = sendContext.CorrelationId = orderId));
        }

        static IServiceProvider ConfigureServiceProvider()
        {
            var collection = new ServiceCollection();
            collection.AddMassTransit(cfg =>
            {
                cfg.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
                cfg.AddSagaStateMachinesFromNamespaceContaining(typeof(OrderStateMachine));

                cfg.AddBus(BusFactory);
            });

            collection.AddScoped<PublishOrderEventActivity>();
            collection.RegisterInMemorySagaRepository();

            return collection.BuildServiceProvider();
        }

        static IBusControl BusFactory(IServiceProvider provider)
        {
            return Bus.Factory.CreateUsingInMemory(cfg => cfg.ConfigureEndpoints(provider));
        }
    }
}