namespace NServiceBus.Transports.ActiveMQ.Receivers
{
    using System.Transactions;
    using Apache.NMS;
    using Apache.NMS.ActiveMQ;
    using Apache.NMS.Util;

    //public class MessageProcessor
    //{

    //    public bool PurgeOnStartup { get; set; }


    //    public MessageProcessor(
    //        IActiveMqPurger purger
    //    {
    //        this.activeMqMessageMapper = activeMqMessageMapper;
    //        this.sessionFactory = sessionFactory;
    //        this.purger = purger;
    //        this.transactionScopeFactory = transactionScopeFactory;
    //    }

    //    public IMessageConsumer CreateMessageConsumer(string destination)
    //    {
    //        var d = SessionUtil.GetDestination(session, destination);
    //        PurgeIfNecessary(session, d);
    //        var consumer = session.CreateConsumer(d);
    //        var netTxConsumer = consumer as NetTxMessageConsumer;
    //        if (netTxConsumer != null)
    //        {
    //            netTxConsumer.CreateTransactionScopeForAsyncMessage = CreateTransactionScopeForAsyncMessage;                
    //        }
    //        return consumer;
    //    }

    //    private TransactionScope CreateTransactionScopeForAsyncMessage()
    //    {
    //        return transactionScopeFactory.CreateTransactionScopeForAsyncMessage(transactionSettings);
    //    }

    //    private void PurgeIfNecessary(ISession session, IDestination destination)
    //    {
    //        if (PurgeOnStartup)
    //        {
    //            purger.Purge(session, destination);
    //        }
    //    }
    //}
}