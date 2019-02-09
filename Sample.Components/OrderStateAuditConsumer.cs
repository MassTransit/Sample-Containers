using System;
using System.Threading.Tasks;
using MassTransit;
using Sample.Contracts;

namespace Sample.Components
{
    public class OrderStateAuditConsumer :
        IConsumer<OrderStateCreated>
    {
        public async Task Consume(ConsumeContext<OrderStateCreated> context)
        {
            if (context.Message.OrderId != context.ConversationId)
                await Console.Error.WriteLineAsync("ConversationId was not correct!");

            await Console.Out.WriteLineAsync($"OrderState(created): {context.Message.OrderId} ({context.ConversationId})");
        }
    }
}