namespace NServiceBus.Transports.ActiveMQ.Receivers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Schedulers;
    using System.Transactions;

    using Apache.NMS;

    using NServiceBus.CircuitBreakers;
    using NServiceBus.Features;
    using NServiceBus.Logging;
    using NServiceBus.Serialization;

    public abstract class MessageReceiver : IMessageReceiver
    {
        protected const int MaximumDelay = 1000;
        protected static readonly ILog Logger = LogManager.GetLogger(typeof(MessageReceiver));
        private readonly CircuitBreaker circuitBreaker = new CircuitBreaker(100, TimeSpan.FromSeconds(30));
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
        protected Func<ISession, IMessageConsumer> createConsumer;
        protected Action<TransportMessage, Exception> endProcessMessage;
        private Address endpointAddress;
        private MTATaskScheduler scheduler;
        protected TransactionOptions transactionOptions;
        private Func<TransportMessage, bool> tryProcessMessage;

        private ActiveMqMessageMapper messageMapper;

        public CurrentSessions CurrentSessions { get; set; }

        public ConnectionFactory ConnectionFactory { get; set; }

        public MessageReceiver(IMessageSerializer serializer)
        {
            this.messageMapper = new ActiveMqMessageMapper(serializer, new MessageTypeInterpreter(), new ActiveMqMessageEncoderPipeline(), new ActiveMqMessageDecoderPipeline());
        }

        public void Init(Address address, Unicast.Transport.TransactionSettings settings, Func<TransportMessage, bool> tryProcessMessage,
                         Action<TransportMessage, Exception> endProcessMessage, Func<ISession, IMessageConsumer> createConsumer)
        {
            this.endpointAddress = address;
            this.tryProcessMessage = tryProcessMessage;
            this.endProcessMessage = endProcessMessage;
            this.createConsumer = createConsumer;

            this.transactionOptions = new TransactionOptions
                                     {
                                         IsolationLevel = settings.IsolationLevel,
                                         Timeout = settings.TransactionTimeout
                                     };
        }

        public void Start(int maximumConcurrencyLevel)
        {
            Logger.InfoFormat("Starting MessageReceiver for [{0}].", this.endpointAddress);

            this.scheduler = new MTATaskScheduler(maximumConcurrencyLevel,
                String.Format("MessageReceiver Worker Thread for [{0}]",
                    this.endpointAddress));

            for (int i = 0; i < maximumConcurrencyLevel; i++)
            {
                this.StartConsumer();
            }

            Logger.InfoFormat(" MessageReceiver for [{0}] started with {1} worker threads.", this.endpointAddress,
                maximumConcurrencyLevel);
        }

        public void Stop()
        {
            Logger.InfoFormat("Stopping MessageReceiver for [{0}].", this.endpointAddress);

            this.tokenSource.Cancel();
            this.scheduler.Dispose();

            Logger.InfoFormat("MessageReceiver for [{0}] stopped.", this.endpointAddress);
        }

        private void StartConsumer()
        {
            CancellationToken token = this.tokenSource.Token;

            Task.Factory
                .StartNew(this.Action, token, token, TaskCreationOptions.None, this.scheduler)
                .ContinueWith(t =>
                    {
                        t.Exception.Handle(ex =>
                            {
                                Logger.Error("Error retrieving message.", ex);

                                this.circuitBreaker.Execute(
                                    () =>
                                    Configure.Instance.RaiseCriticalError(
                                        string.Format("One of the MessageReceiver consumer threads for [{0}] crashed.",
                                            this.endpointAddress), ex));
                                return true;
                            });

                        this.StartConsumer();
                    }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private void Action(object obj)
        {
            var cancellationToken = (CancellationToken)obj;

            using (IConnection connection = this.ConnectionFactory.CreateNewConnection())
            {
                this.Receive(cancellationToken, connection);

                connection.Stop();
                connection.Close();
            }
        }

        protected abstract void Receive(CancellationToken token, IConnection connection);


        protected TransportMessage ConvertMessage(IMessage message)
        {
            try
            {
                return this.ConvertToTransportMessage(message);
            }
            catch (Exception ex)
            {
                Logger.Error("Error in converting ActiveMQ message to TransportMessage.", ex);

                return new TransportMessage(message.NMSMessageId, null);
            }
        }

        protected bool ProcessMessage(TransportMessage message)
        {
            return this.tryProcessMessage(message);
        }

        private TransportMessage ConvertToTransportMessage(IMessage message)
        {
            return this.messageMapper.CreateTransportMessage(message);
        }
    }
}