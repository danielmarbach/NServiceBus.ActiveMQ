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

        ~CurrentSessions()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.sessionsPerThread.Dispose();
            }

            this.disposed = true;
        }
    }
}