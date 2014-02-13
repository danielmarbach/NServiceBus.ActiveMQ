namespace NServiceBus.Transports.ActiveMQ.Receivers
{
    using System;
    using System.Threading;

    using Apache.NMS;

    using NServiceBus.Serialization;

    public class LocalTransactionMessageReceiver : MessageReceiver
    {
        private IMessageConsumer consumer;

        public LocalTransactionMessageReceiver(IMessageSerializer serializer)
            : base(serializer)
        {
        }

        protected override void Receive(CancellationToken token, IConnection connection)
        {
            using (ISession session = connection.CreateSession())
            {
                this.CurrentSessions.SetSession(session);

                token.Register(() =>
                    {
                        if (this.consumer != null)
                        {
                            this.consumer.Close();
                        }
                    });

                using (this.consumer = this.createConsumer(session))
                {
                    while (!token.IsCancellationRequested)
                    {
                        IMessage message = this.consumer.Receive(TimeSpan.FromMilliseconds(MaximumDelay));

                        if (message != null)
                        {
                            Exception exception = null;
                            TransportMessage transportMessage = null;
                            try
                            {
                                transportMessage = this.ConvertMessage(message);

                                if (this.ProcessMessage(transportMessage))
                                {
                                    session.Commit();
                                }
                                else
                                {
                                    session.Rollback();
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Error("Error processing message.", ex);

                                session.Rollback();

                                exception = ex;
                            }
                            finally
                            {
                                this.endProcessMessage(transportMessage, exception);
                            }
                        }
                    }

                    this.consumer.Close();
                }

                session.Close();
            }
        }
    }
}