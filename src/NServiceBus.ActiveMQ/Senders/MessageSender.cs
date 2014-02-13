namespace NServiceBus.Transports.ActiveMQ.Senders
{
    using Apache.NMS;

    using NServiceBus.Serialization;

    public abstract class MessageSender : ISendMessages
    {
        private ActiveMqMessageMapper messageMapper;

        protected MessageSender(IMessageSerializer serializer)
        {
            this.messageMapper = new ActiveMqMessageMapper(serializer, new MessageTypeInterpreter(), new ActiveMqMessageEncoderPipeline(), new ActiveMqMessageDecoderPipeline());
        }

        public ConnectionFactory ConnectionFactory { get; set; }

        public abstract void Send(TransportMessage message, Address address);

        protected ISession CreateSession()
        {
            return this.ConnectionFactory.GetPooledConnection().CreateSession();
        }

        protected IMessage CreateNativeMessage(TransportMessage message, ISession session, IMessageProducer producer)
        {
            return this.messageMapper.CreateJmsMessage(message, session, ((Apache.NMS.ActiveMQ.MessageProducer)producer).ProducerId.ToString());
        }
    }
}