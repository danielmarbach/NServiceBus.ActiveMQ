namespace NServiceBus.Transports.ActiveMQ.Senders
{
    using System.Transactions;

    using Apache.NMS;
    using Apache.NMS.Util;

    using NServiceBus.Serialization;

    public class NoTransactionMessageSender : MessageSender
    {
        public NoTransactionMessageSender(IMessageSerializer serializer)
            : base(serializer)
        {
        }

        public override void Send(TransportMessage message, Address address)
        {
            using (var session = this.CreateSession())
            {
                var destination = SessionUtil.GetDestination(session, Address.Parse(string.Format("queue://{0}", address.Queue)).ToString());
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