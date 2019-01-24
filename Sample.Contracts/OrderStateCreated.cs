using System;

namespace Sample.Contracts
{
    public interface OrderStateCreated
    {
        Guid OrderId { get; }
        DateTime Timestamp { get; }
    }
}