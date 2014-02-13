namespace NServiceBus.Transports.ActiveMQ
{
    using System;

    public interface ITopicEvaluator
    {
        Address GetTopicFromMessageType(Type type);
    }
}