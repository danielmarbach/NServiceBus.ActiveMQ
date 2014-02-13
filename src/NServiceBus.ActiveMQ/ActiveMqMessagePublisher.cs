namespace NServiceBus.Transports.ActiveMQ
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ActiveMqMessagePublisher : IPublishMessages
    {
        private readonly ITopicEvaluator topicEvaluator;
        private readonly ISendMessages messageSender;

        public ActiveMqMessagePublisher(ITopicEvaluator topicEvaluator, ISendMessages messageSender)
        {
            this.topicEvaluator = topicEvaluator;
            this.messageSender = messageSender;
        }

        public bool Publish(TransportMessage message, IEnumerable<Type> eventTypes)
        {
            var eventType = eventTypes.First(); //we route on the first event for now

            var topic = topicEvaluator.GetTopicFromMessageType(eventType);
            this.messageSender.Send(message, topic);

            return true;
        }
    }
}