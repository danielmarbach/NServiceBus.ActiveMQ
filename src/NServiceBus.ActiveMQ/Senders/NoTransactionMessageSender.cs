namespace NServiceBus.Transports.ActiveMQ.Senders
{
    using System.Transactions;

    using NServiceBus.Serialization;

    public class NoTransactionMessageSender : MessageSender
    {
        public NoTransactionMessageSender(IMessageSerializer serializer)
            : base(serializer)
        {
        }

        protected override void InternalSend(TransportMessage message, ActiveMqAddress address)
        {
            using (var session = this.CreateSession())
            {
                var destination = address.GetDestination(session);
                using (var producer = session.CreateProducer(destination))
                {
                    var nativeMessage = this.CreateNativeMessage(message, session, producer);

                    using (new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        producer.Send(nativeMessage);
                    }

                    producer.Close();
                }

                session.Close();
            }
        }
    }
}