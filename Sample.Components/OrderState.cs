using System;
using Automatonymous;

namespace Sample.Components
{
    public class OrderState :
        SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }

        public DateTimeOffset OrderSubmissionDateTime { get; set; }
        public DateTime OrderSubmissionDateTimeUtc { get; set; }
    }
}