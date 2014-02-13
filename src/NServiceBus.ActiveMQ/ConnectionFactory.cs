namespace NServiceBus.Transports.ActiveMQ
{
    using System;
    using System.Threading;

    using Apache.NMS;

    using NServiceBus.Settings;

    public class ConnectionFactory : IDisposable
    {
        private readonly IConnectionFactory connectionFactory;
        private IConnection connection;
        private readonly object lockObj = new object();
        private bool disposed;

        public ConnectionFactory(IConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        ~ConnectionFactory()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                if (connection != null)
                {
                    connection.Stop();
                    connection.Close();
                    connection.Dispose();
                }
            }

            disposed = true;
        }

        public IConnection CreateNewConnection()
        {
            var newConnection = connectionFactory.CreateConnection();
            newConnection.ClientId = ClientId();

            newConnection.Start();

            return newConnection;
        }

        public IConnection GetPooledConnection()
        {
            if (connection != null)
            {
                return connection;
            }

            lock (lockObj)
            {
                if (connection != null)
                {
                    return connection;
                }

                connection = connectionFactory.CreateConnection();
                connection.ClientId = ClientId();

                if (!SettingsHolder.Get<bool>("Endpoint.SendOnly"))
                {
                    connection.Start();
                }
            }

            return connection;
        }

        private static string ClientId()
        {
            var clientId = String.Format("NServiceBus-{0}-{1}-{2}", Address.Local, Configure.DefineEndpointVersionRetriever(),
                                         Thread.CurrentThread.ManagedThreadId);
            return clientId;
        }
    }
}