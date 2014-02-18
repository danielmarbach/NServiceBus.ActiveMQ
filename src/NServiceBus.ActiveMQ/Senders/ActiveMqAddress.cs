namespace NServiceBus.Transports.ActiveMQ.Senders
{
    using Apache.NMS;
    using Apache.NMS.Util;

    public class ActiveMqAddress
    {
        readonly Address address;

        public ActiveMqAddress(Address address)
        {
            this.address = address;
        }

        public IDestination GetDestination(ISession session)
        {
            return SessionUtil.GetDestination(session, this.address.Queue);
        }

        public override string ToString()
        {
            return this.address.ToString();
        }

        public override int GetHashCode()
        {
            return this.address.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.address.Equals(obj);
        }
    }
}