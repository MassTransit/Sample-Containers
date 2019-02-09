using System;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using GreenPipes;
using MassTransit;
using MassTransit.Util;
using Sample.Components;
using Sample.Contracts;

namespace Sample_Autofac
{
    static class Program
    {
        static void Main()
        {
            var container = ConfigureContainer();

            var bus = container.Resolve<IBusControl>();

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

        static async Task Submit(IWindsorContainer container)
        {
            IBus bus = container.Resolve<IBus>();

            var orderId = NewId.NextGuid();

            await bus.Send<SubmitOrder>(new
            {
                OrderId = orderId,
                OrderDateTime = DateTimeOffset.Now
            }, Pipe.Execute<SendContext>(sendContext => sendContext.ConversationId = sendContext.CorrelationId = orderId));
        }

        static IWindsorContainer ConfigureContainer()
        {
            var container = new WindsorContainer();
            container.AddMassTransit(cfg =>
            {
                cfg.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
                cfg.AddSagaStateMachinesFromNamespaceContaining(typeof(OrderStateMachine));

                cfg.AddBus(kernel => BusFactory(container));
            });

            container.Register(Component.For<PublishOrderEventActivity>());

            container.RegisterInMemorySagaRepository();

            return container;
        }

        static IBusControl BusFactory(IWindsorContainer container)
        {
            return Bus.Factory.CreateUsingInMemory(cfg => cfg.ConfigureEndpoints(container));
        }
    }
}