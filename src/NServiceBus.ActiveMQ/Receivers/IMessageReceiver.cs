namespace NServiceBus.Transports.ActiveMQ.Receivers
{
    using System;

    using Apache.NMS;

    public interface IMessageReceiver
    {
        void Init(Address address, Unicast.Transport.TransactionSettings settings, Func<TransportMessage, bool> tryProcessMessage,
                  Action<TransportMessage, Exception> endProcessMessage, Func<ISession, IMessageConsumer> createConsumer);

        void Start(int maximumConcurrencyLevel);
        void Stop();
    }
}