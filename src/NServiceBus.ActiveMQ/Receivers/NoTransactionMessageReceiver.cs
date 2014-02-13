namespace NServiceBus.Transports.ActiveMQ.Receivers
{
    using System;
    using System.Threading;

    using Apache.NMS;

    using NServiceBus.Serialization;

    public class NoTransactionMessageReceiver : MessageReceiver
    {
        IMessageConsumer consumer;

        public NoTransactionMessageReceiver(IMessageSerializer serializer)
            : base(serializer)
        {
        }

        protected override void Receive(CancellationToken token, IConnection connection)
        {
            token.Register(() =>
                {
                    if (this.consumer != null)
                    {
                        this.consumer.Close();
                    }
                });

            while (!token.IsCancellationRequested)
            {
                using (ISession session = connection.CreateSession())
                {
                    using (this.consumer = this.createConsumer(session))
                    {
                        IMessage message = this.consumer.Receive(TimeSpan.FromMilliseconds(MaximumDelay));

                        if (message != null)
                        {
                            Exception exception = null;
                            TransportMessage transportMessage = null;
                            try
                            {
                                transportMessage = this.ConvertMessage(message);

                                this.ProcessMessage(transportMessage);
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

                        this.consumer.Close();
                    }

                    session.Close();
                }
            }
        }
    }
}