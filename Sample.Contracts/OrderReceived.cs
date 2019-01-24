using System;

namespace Sample.Contracts
{
    public interface OrderReceived
    {
        Guid OrderId { get; }
        DateTimeOffset OrderDateTime { get; }
    }
}