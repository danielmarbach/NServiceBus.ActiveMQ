namespace NServiceBus.Transports.ActiveMQ.SessionFactories
{
    using System;
    using Apache.NMS;

    public interface ISessionFactory : IDisposable
    {
        string GetClientId(ISession session);

        ISession GetSession();

        void Release(ISession session);

        void SetSessionForCurrentThread(ISession session);

        void RemoveSessionForCurrentThread();
    }
}