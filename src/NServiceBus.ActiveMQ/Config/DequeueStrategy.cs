namespace NServiceBus.Features
{
    using System;

    using NServiceBus.Logging;
    using NServiceBus.Transports;
    using NServiceBus.Transports.ActiveMQ;
    using NServiceBus.Transports.ActiveMQ.Receivers;
    using NServiceBus.Transports.ActiveMQ.Senders;

    public class DequeueStrategy : IDequeueMessages
    {
        private readonly SubscriptionsManager subscriptionsManager;
        private readonly IMessageReceiver messageReceiver;

        /// <summary>
        ///     Purges the queue on startup.
        /// </summary>
        public bool PurgeOnStartup { get; set; }

        /// <summary>
        /// Settings
        /// </summary>
        public ConnectionConfiguration Settings { get; set; }

        public DequeueStrategy(SubscriptionsManager subscriptionsManager, IMessageReceiver messageReceiver)
        {
            this.subscriptionsManager = subscriptionsManager;
            this.messageReceiver = messageReceiver;
        }

        public void Init(Address address, Unicast.Transport.TransactionSettings settings, Func<TransportMessage, bool> tryProcessMessage,
                         Action<TransportMessage, Exception> endProcessMessage)
        {
            this.isTheMainTransport = address == Address.Local;

            this.endpointAddress = new ActiveMqAddress(address);

            if (address == Address.Local)
            {
                this.subscriptionsManager.Init(settings, tryProcessMessage, endProcessMessage);
            }

            this.messageReceiver.Init(this.endpointAddress, settings, tryProcessMessage, endProcessMessage, session => session.CreateConsumer(this.endpointAddress.GetDestination(session)));
        }

        public void Start(int maximumConcurrencyLevel)
        {
            if (this.PurgeOnStartup)
            {
                this.Purge();
            }

            this.messageReceiver.Start(maximumConcurrencyLevel);

            if (this.isTheMainTransport)
            {
                this.subscriptionsManager.Start(1);
            }
        }

        public void Stop()
        {
            if (this.isTheMainTransport)
            {
                this.subscriptionsManager.Stop();
            }

            this.messageReceiver.Stop();
        }

        private void Purge()
        {
            // TOODELDIDOOO
        }


        static readonly ILog Logger = LogManager.GetLogger(typeof(DequeueStrategy));
        ActiveMqAddress endpointAddress;
        bool isTheMainTransport;
    }
}