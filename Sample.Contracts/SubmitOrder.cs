using System;

namespace Sample.Contracts
{
    public interface SubmitOrder
    {
        Guid OrderId { get; }
        DateTimeOffset OrderDateTime { get; }
    }
}