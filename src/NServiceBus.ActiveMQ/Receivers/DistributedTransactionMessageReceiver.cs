namespace NServiceBus.Transports.ActiveMQ.Receivers
{
    using System;
    using System.Threading;
    using System.Transactions;

    using Apache.NMS;

    using NServiceBus.Serialization;

    public class DistributedTransactionMessageReceiver : MessageReceiver
    {
        public DistributedTransactionMessageReceiver(IMessageSerializer serializer)
            : base(serializer)
        {
        }

        protected override void Receive(CancellationToken token, IConnection connection)
        {
            using (ISession session = connection.CreateSession())
            {
                this.CurrentSessions.SetSession(session);

                using (IMessageConsumer consumer = this.createConsumer(session))
                {
                    while (!token.IsCancellationRequested)
                    {
                        using (var scope = new TransactionScope(TransactionScopeOption.Required, this.transactionOptions))
                        {
                            IMessage message = consumer.Receive(TimeSpan.FromMilliseconds(MaximumDelay));

                            if (message != null)
                            {
                                Exception exception = null;
                                TransportMessage transportMessage = null;

                                try
                                {
                                    transportMessage = this.ConvertMessage(message);

                                    if (this.ProcessMessage(transportMessage))
                                    {
                                        scope.Complete();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error("Error processing message.", ex);

                                    exception = ex;
                                }
                                finally
                                {
                                    this.endProcessMessage(transportMessage, exception);
                                }
                            }
                        }
                    }

                    consumer.Close();
                }

                session.Close();
            }
        }
    }
}