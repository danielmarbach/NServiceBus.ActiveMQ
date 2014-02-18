namespace NServiceBus.Transports.ActiveMQ.Senders
{
    using System.Transactions;

    using Apache.NMS;
    using Apache.NMS.Util;

    using NServiceBus.Serialization;

    public class DistributedTransactionMessageSender : MessageSender
    {
        public DistributedTransactionMessageSender(IMessageSerializer serializer)
            : base(serializer)
        {
        }

        public CurrentSessions CurrentSessions { get; set; }

        protected override void InternalSend(TransportMessage message, ActiveMqAddress address)
        {
            var hasExistingSession = true;
            var session = this.CurrentSessions.GetSession();

            if (session == null)
            {
                hasExistingSession = false;
                session = this.CreateSession();
            }

            try
            {
                var destination = address.GetDestination(session);
                using (var producer = session.CreateProducer(destination))
                {
                    var nativeMessage = this.CreateNativeMessage(message, session, producer);

                    if (!hasExistingSession)
                    {
                        using (new TransactionScope(TransactionScopeOption.Suppress))
                        {
                            producer.Send(nativeMessage);
                        }
                    }
                    else
                    {
                        producer.Send(nativeMessage);
                    }

                    producer.Close();
                }
            }
            finally
            {
                if (!hasExistingSession)
                {
                    session.Close();
                    session.Dispose();
                }
            }
        }
    }
}