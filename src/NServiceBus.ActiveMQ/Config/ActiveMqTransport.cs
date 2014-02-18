namespace NServiceBus.Features
{
    using System.Text;

    using Apache.NMS;
    using Apache.NMS.ActiveMQ;
    using Apache.NMS.Policies;
    using Config;

    using NServiceBus.MessageInterfaces.MessageMapper.Reflection;
    using NServiceBus.Transports.ActiveMQ;
    using NServiceBus.Transports.ActiveMQ.Receivers;
    using NServiceBus.Transports.ActiveMQ.Senders;
    using NServiceBus.Utils;

    using Settings;
    using Transports;

    using ConnectionFactory = NServiceBus.Transports.ActiveMQ.ConnectionFactory;
    using IMessageProducer = Apache.NMS.IMessageProducer;
    using NMSConnectionFactory = Apache.NMS.ActiveMQ.ConnectionFactory;

    /// <summary>
    /// Default configuration for ActiveMQ
    /// </summary>
    public class ActiveMqTransport : ConfigureTransport<ActiveMQ>
    {
        public override void Initialize()
        {
            if (!SettingsHolder.GetOrDefault<bool>("ScaleOut.UseSingleBrokerQueue"))
            {
                Address.InitializeLocalAddress(Address.Local.Queue + "." + Address.Local.Machine);
            }

            var connectionString = SettingsHolder.Get<string>("NServiceBus.Transport.ConnectionString");
            var connectionConfiguration = new ConnectionStringBuilder().Parse(connectionString);

            NServiceBus.Configure.Instance.Configurer.RegisterSingleton<ConnectionConfiguration>(connectionConfiguration);

            NServiceBus.Configure.Component<ConnectionFactory>(DependencyLifecycle.SingleInstance);
            NServiceBus.Configure.Component<CurrentSessions>(DependencyLifecycle.SingleInstance);
            NServiceBus.Configure.Component<SubscriptionsManager>(DependencyLifecycle.SingleInstance);

            var transportConfig = NServiceBus.Configure.GetConfigSection<TransportConfig>();
            var maxRetries = transportConfig == null ? 6 : transportConfig.MaxRetries + 1;

            var transactionSettings = new Unicast.Transport.TransactionSettings();

            if (transactionSettings.IsTransactional)
            {
                if (!transactionSettings.DontUseDistributedTransactions)
                {
                    NServiceBus.Configure.Component<IConnectionFactory>(
                        () =>
                        new NetTxConnectionFactory(connectionConfiguration.ServerUrl)
                            {
                                AcknowledgementMode = AcknowledgementMode.Transactional,
                                RedeliveryPolicy = new RedeliveryPolicy
                                                       {
                                                           MaximumRedeliveries = maxRetries,
                                                           BackOffMultiplier = 0,
                                                           UseExponentialBackOff = false
                                                       },
                                ConfiguredResourceManagerId = connectionConfiguration.RessourceManagerId.ToString()
                            },
                            DependencyLifecycle.SingleInstance);
                    NServiceBus.Configure.Component<DistributedTransactionMessageReceiver>(DependencyLifecycle.InstancePerCall);
                    NServiceBus.Configure.Component<DistributedTransactionMessageSender>(DependencyLifecycle.InstancePerCall);

                }
                else
                {
                    NServiceBus.Configure.Component<IConnectionFactory>(
                        () =>
                        new NMSConnectionFactory(connectionConfiguration.ServerUrl)
                            {
                                AcknowledgementMode = AcknowledgementMode.Transactional,
                                RedeliveryPolicy = new RedeliveryPolicy { MaximumRedeliveries = maxRetries, BackOffMultiplier = 0, UseExponentialBackOff = false }
                            },
                            DependencyLifecycle.SingleInstance);
                    NServiceBus.Configure.Component<LocalTransactionMessageSender>(DependencyLifecycle.InstancePerCall);
                    NServiceBus.Configure.Component<LocalTransactionMessageReceiver>(DependencyLifecycle.InstancePerCall);
                }
            }
            else
            {
                NServiceBus.Configure.Component<IConnectionFactory>(
                    () =>
                    new NMSConnectionFactory(connectionConfiguration.ServerUrl)
                        {
                            AcknowledgementMode = AcknowledgementMode.AutoAcknowledge,
                            AsyncSend = true,
                        },
                        DependencyLifecycle.SingleInstance);
                NServiceBus.Configure.Component<NoTransactionMessageSender>(DependencyLifecycle.InstancePerCall);
                NServiceBus.Configure.Component<NoTransactionMessageReceiver>(DependencyLifecycle.InstancePerCall);

                NServiceBus.Configure.Component<ActiveMQMessageDefer>(DependencyLifecycle.InstancePerCall);
                NServiceBus.Configure.Component<ActiveMqSchedulerManagement>(DependencyLifecycle.SingleInstance)
                      .ConfigureProperty(p => p.Disabled, false);
                NServiceBus.Configure.Component<ActiveMqSchedulerManagementJobProcessor>(DependencyLifecycle.SingleInstance);
                NServiceBus.Configure.Component<ActiveMqSchedulerManagementCommands>(DependencyLifecycle.SingleInstance);
            }

            NServiceBus.Configure.Component<ActiveMqMessagePublisher>(DependencyLifecycle.InstancePerCall);
            NServiceBus.Configure.Component<ActiveMqQueueCreator>(DependencyLifecycle.InstancePerCall);
            NServiceBus.Configure.Component<DequeueStrategy>(DependencyLifecycle.InstancePerCall)
                       .ConfigureProperty(p => p.PurgeOnStartup, ConfigurePurging.PurgeRequested)
                       .ConfigureProperty(p => p.Settings, connectionConfiguration);
            NServiceBus.Configure.Component<TopicEvaluator>(DependencyLifecycle.InstancePerCall);
        }

        protected override void InternalConfigure(Configure config)
        {
            Enable<ActiveMqTransport>();
        }

        protected override string ExampleConnectionStringForErrorMessage
        {
            get { return "ServerUrl=activemq:tcp://localhost:61616; ResourceManagerId=2f2c3321-f251-4975-802d-11fc9d9e5e37"; }
        }
    }
}