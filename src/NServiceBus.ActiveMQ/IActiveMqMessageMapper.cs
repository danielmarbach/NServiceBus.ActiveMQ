namespace NServiceBus.Transports.ActiveMQ
{
    using Apache.NMS;

    public interface IActiveMqMessageMapper
    {
        IMessage CreateJmsMessage(TransportMessage message, ISession session, string producerId);

        TransportMessage CreateTransportMessage(IMessage message);
    }
}