namespace NServiceBus.Transports.ActiveMQ
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;

    using Apache.NMS;

    using NServiceBus.Logging;
    using NServiceBus.ObjectBuilder;
    using NServiceBus.Transports.ActiveMQ.Receivers;
    using NServiceBus.Transports.ActiveMQ.Senders;
    using NServiceBus.Unicast.Transport;

    public class SubscriptionsManager : IManageSubscriptions
    {
        private readonly ConnectionFactory factory;

        private readonly BlockingCollection<Tuple<Type, Address>> events =
            new BlockingCollection<Tuple<Type, Address>>();

        private readonly List<EventConsumerSatellite> satellites = new List<EventConsumerSatellite>();
        private Action<TransportMessage, Exception> endProcessMessage;

        private Thread startSubscriptionThread;
        private TransactionSettings settings;
        private Func<TransportMessage, bool> tryProcessMessage;
        static readonly ILog Logger = LogManager.GetLogger(typeof(SubscriptionsManager));

        private readonly ITopicEvaluator topicEvaluator;

        public IBuilder Builder { get; set; }

        public SubscriptionsManager(ConnectionFactory factory, ITopicEvaluator topicEvaluator)
        {
            this.topicEvaluator = topicEvaluator;
            this.factory = factory;
        }

        public void Subscribe(Type eventType, Address publisherAddress)
        {
            this.events.TryAdd(Tuple.Create(eventType, publisherAddress));
        }

        public void Unsubscribe(Type eventType, Address publisherAddress)
        {
            EventConsumerSatellite consumerSatellite =
                this.satellites.Find(
                    consumer =>
                    consumer.InputAddress == new ActiveMqAddress(publisherAddress) && consumer.EventType == eventType);

            if (consumerSatellite == null)
            {
                return;
            }

            consumerSatellite.Stop();
            this.satellites.Remove(consumerSatellite);

            //var connection = factory.GetPooledConnection();
            //using (ISession session = connection.CreateSession())
            //{
            //    session.D
            //    session.Close();
            //}
        }

        public void Init(TransactionSettings settings, Func<TransportMessage, bool> tryProcessMessage,
                         Action<TransportMessage, Exception> endProcessMessage)
        {
            this.settings = settings;
            this.tryProcessMessage = tryProcessMessage;
            this.endProcessMessage = endProcessMessage;
        }

        public void Start(int maximumConcurrencyLevel)
        {
            this.startSubscriptionThread = new Thread(() =>
                {
                    foreach (var tuple in this.events.GetConsumingEnumerable())
                    {
                        var messageReceiver = this.Builder.Build<IMessageReceiver>();
                        Address topic = this.topicEvaluator.GetTopicFromMessageType(tuple.Item1);
                        var address = new ActiveMqAddress(Address.Parse(string.Format("queue://Consumer.{0}.{1}", Configure.EndpointName.Replace('.', '-'), topic.Queue.Replace("topic://", string.Empty))));
                        var consumerSatellite = new EventConsumerSatellite(messageReceiver, address, tuple.Item1);

                        messageReceiver.Init(address, this.settings, this.tryProcessMessage, this.endProcessMessage, consumerSatellite.CreateConsumer);

                        this.satellites.Add(consumerSatellite);
                        Logger.InfoFormat("Starting receiver for [{0}] subscription.", address);

                        consumerSatellite.Start(maximumConcurrencyLevel);
                    }
                }) { IsBackground = true };

            this.startSubscriptionThread.SetApartmentState(ApartmentState.MTA);
            this.startSubscriptionThread.Name = "Start ActiveMq Subscription Listeners";
            this.startSubscriptionThread.Start();
        }

        public void Stop()
        {
            this.events.CompleteAdding();

            foreach (EventConsumerSatellite consumerSatellite in this.satellites)
            {
                consumerSatellite.Stop();
            }
        }

        private class EventConsumerSatellite
        {
            private readonly ActiveMqAddress address;
            private readonly Type eventType;
            private readonly IMessageReceiver receiver;

            public EventConsumerSatellite(IMessageReceiver receiver, ActiveMqAddress address, Type eventType)
            {
                this.receiver = receiver;
                this.eventType = eventType;

                this.address = address;
            }

            public IMessageConsumer CreateConsumer(ISession session)
            {
                return session.CreateConsumer(this.address.GetDestination(session));
            }

            public ActiveMqAddress InputAddress
            {
                get { return this.address; }
            }

            public Type EventType
            {
                get { return this.eventType; }
            }

            public void Start(int maximumConcurrencyLevel)
            {
                this.receiver.Start(maximumConcurrencyLevel);
            }

            public void Stop()
            {
                this.receiver.Stop();
            }
        }
    }
}