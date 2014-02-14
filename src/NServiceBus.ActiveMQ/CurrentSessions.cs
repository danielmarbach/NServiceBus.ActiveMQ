namespace NServiceBus.Transports.ActiveMQ
{
    using System;
    using System.Threading;

    using Apache.NMS;

    public class CurrentSessions : IDisposable
    {
        private readonly ThreadLocal<ISession> sessionsPerThread = new ThreadLocal<ISession>();
        private bool disposed;

        /// <summary>
        ///     Sets the native session.
        /// </summary>
        /// <param name="session">
        ///     Native <see cref="ISession" />.
        /// </param>
        public void SetSession(ISession session)
        {
            this.sessionsPerThread.Value = session;
        }

        public ISession GetSession()
        {
            if (this.sessionsPerThread.IsValueCreated)
            {
                return this.sessionsPerThread.Value;
            }

            return null;
        }

        public void Dispose()
        {
            // Injected during compile time
        }

        private void DisposeManager()
        {
            this.sessionsPerThread.Dispose();
        }
    }
}