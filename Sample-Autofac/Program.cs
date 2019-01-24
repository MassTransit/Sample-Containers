using System;
using System.Threading.Tasks;
using Autofac;
using GreenPipes;
using MassTransit;
using MassTransit.Saga;
using MassTransit.Util;
using Sample.Components;
using Sample.Contracts;

namespace Sample_Autofac
{
    class Program
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

        static async Task Submit(IContainer container)
        {
            ISendEndpointProvider provider = container.Resolve<ISendEndpointProvider>();
            var endpoint = await provider.GetSendEndpoint(new Uri("loopback://localhost/submit-order"));

            await endpoint.Send<SubmitOrder>(new
            {
                OrderId = NewId.NextGuid(),
                OrderDateTime = DateTimeOffset.Now
            }, sendContext => sendContext.CorrelationId = NewId.NextGuid());
        }

        static IContainer ConfigureContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterConsumers(typeof(SubmitOrderConsumer).Assembly)
                .InstancePerLifetimeScope();

            builder.RegisterStateMachineSagas(typeof(OrderStateMachine).Assembly);
            builder.RegisterType<PublishOrderEventActivity>();

            builder.RegisterGeneric(typeof(InMemorySagaRepository<>))
                .As(typeof(ISagaRepository<>));

            builder.Register(BusFactory)
                .As<IBusControl>()
                .As<IBus>()
                .As<ISendEndpointProvider>()
                .As<IPublishEndpoint>()
                .SingleInstance();

            return builder.Build();
        }

        static IBusControl BusFactory(IComponentContext context)
        {
            return Bus.Factory.CreateUsingInMemory(cfg =>
            {
                cfg.ReceiveEndpoint("submit-order", e =>
                {
                    e.UseMessageRetry(r => r.Interval(5, 1000));
                    e.UseInMemoryOutbox();

                    e.Consumer<SubmitOrderConsumer>(context);
                });

                cfg.ReceiveEndpoint("order-state", e =>
                {
                    e.UseMessageRetry(r => r.Interval(5, 1000));
                    e.UseInMemoryOutbox();

                    e.StateMachineSaga<OrderState>(context);
                });

                cfg.ReceiveEndpoint("order-state-audit", e =>
                {
                    e.UseMessageRetry(r => r.Interval(5, 1000));
                    e.UseInMemoryOutbox();

                    e.Consumer<OrderStateAuditConsumer>(context);
                });
            });
        }
    }
}