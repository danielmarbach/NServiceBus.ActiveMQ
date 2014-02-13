namespace NServiceBus.Transports.ActiveMQ
{
    using System;

    public class TopicEvaluator : ITopicEvaluator
    {
        public Address GetTopicFromMessageType(Type type)
        {
            return Address.Parse(string.Format("topic://VirtualTopic.{0}", type.FullName));
        }
    }
}