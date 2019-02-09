using System;
using System.Threading.Tasks;
using GreenPipes;
using Lamar;
using MassTransit;
using MassTransit.Util;
using Sample.Components;
using Sample.Contracts;

namespace Sample_Lamar
{
    static class Program
    {
        static void Main()
        {
            var container = ConfigureContainer();

            var bus = container.GetInstance<IBusControl>();

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
                                TaskUtil.Await(() => Submit(container));
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

        static async Task Submit(IContainer container)
        {
            IBus bus = container.GetInstance<IBus>();

            var orderId = NewId.NextGuid();

            await bus.Send<SubmitOrder>(new
            {
                OrderId = orderId,
                OrderDateTime = DateTimeOffset.Now
            }, Pipe.Execute<SendContext>(sendContext => sendContext.ConversationId = sendContext.CorrelationId = orderId));
        }

        static IContainer ConfigureContainer()
        {
            return new Container(builder =>
            {
                builder.AddMassTransit(cfg =>
                {
                    cfg.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
                    cfg.AddSagaStateMachinesFromNamespaceContaining(typeof(OrderStateMachine));

                    cfg.AddBus(BusFactory);
                });

                builder.ForConcreteType<PublishOrderEventActivity>();

                builder.RegisterInMemorySagaRepository();
            });
        }

        static IBusControl BusFactory(IServiceContext context)
        {
            return Bus.Factory.CreateUsingInMemory(cfg => cfg.ConfigureEndpoints(context));
        }
    }
}