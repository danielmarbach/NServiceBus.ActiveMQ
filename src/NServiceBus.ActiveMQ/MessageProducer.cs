namespace NServiceBus.Transports.ActiveMQ
{
    using SessionFactories;

    public class MessageProducer : IMessageProducer
    {
        private readonly IActiveMqMessageMapper activeMqMessageMapper;
        private readonly ISessionFactory sessionFactory;
        private readonly IDestinationEvaluator destinationEvaluator;

        public MessageProducer(
            ISessionFactory sessionFactory, 
            IActiveMqMessageMapper activeMqMessageMapper,
            IDestinationEvaluator destinationEvaluator)
        {
            this.sessionFactory = sessionFactory;
            this.activeMqMessageMapper = activeMqMessageMapper;
            this.destinationEvaluator = destinationEvaluator;
        }

        public void SendMessage(TransportMessage message, string destination, string destinationPrefix)
        {
            var session = sessionFactory.GetSession();
            try
            {
                using (var producer = session.CreateProducer())
                {
                    var producerId = this.GetProducerId(producer);
                    var jmsMessage = activeMqMessageMapper.CreateJmsMessage(message, session, producerId);
                    producer.Send(destinationEvaluator.GetDestination(session, destination, destinationPrefix), jmsMessage);
                }
            }
            finally
            {
                sessionFactory.Release(session);
            }
        }

        protected virtual string GetProducerId(Apache.NMS.IMessageProducer producer)
        {
            return ((Apache.NMS.ActiveMQ.MessageProducer)producer).ProducerId.ToString();
        }
    }
}