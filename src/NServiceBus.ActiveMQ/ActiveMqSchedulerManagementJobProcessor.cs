﻿//namespace NServiceBus.Transports.ActiveMQ
//{
//    using System;
//    using System.Collections.Concurrent;
//    using System.Collections.Generic;
//    using System.Linq;
//    using System.Threading;

//    public class ActiveMqSchedulerManagementJobProcessor
//    {
//        private readonly IActiveMqSchedulerManagementCommands activeMqSchedulerManagementCommands;
//        private readonly ConcurrentDictionary<ActiveMqSchedulerManagementJob, ActiveMqSchedulerManagementJob> jobs = 
//            new ConcurrentDictionary<ActiveMqSchedulerManagementJob, ActiveMqSchedulerManagementJob>();

//        public ActiveMqSchedulerManagementJobProcessor(IActiveMqSchedulerManagementCommands activeMqSchedulerManagementCommands)
//        {
//            this.activeMqSchedulerManagementCommands = activeMqSchedulerManagementCommands;
//        }

//        public void Start()
//        {
//            activeMqSchedulerManagementCommands.Start();
//        }

//        public void Stop()
//        {
//            DeleteJobs(jobs.Keys);
//            activeMqSchedulerManagementCommands.Stop();
//        }
        
//        public bool HandleTransportMessage(TransportMessage message)
//        {
//            var job = activeMqSchedulerManagementCommands.CreateActiveMqSchedulerManagementJob(
//                message.Headers[ActiveMqSchedulerManagement.ClearScheduledMessagesSelectorHeader]);

//            activeMqSchedulerManagementCommands.RequestDeferredMessages(job.Destination);
//            jobs[job] = job;

//            return true;
//        }

//        public void ProcessAllJobs(CancellationToken token)
//        {
//            foreach (var job in jobs.Keys.ToList())
//            {
//                if (token.IsCancellationRequested)
//                {
//                    return;
//                }
//                activeMqSchedulerManagementCommands.ProcessJob(job);
//            }

//            RemoveExpiredJobs();
//        }

//        private void RemoveExpiredJobs()
//        {
//            DeleteJobs(jobs.Keys.Where(j => DateTime.Now > j.ExprirationDate));
//        }

//        private void DeleteJobs(IEnumerable<ActiveMqSchedulerManagementJob> jobs)
//        {
//            foreach (var job in jobs.ToList())
//            {
//                ActiveMqSchedulerManagementJob jobValue;
//                this.jobs.TryRemove(job, out jobValue);
//                activeMqSchedulerManagementCommands.DisposeJob(job);
//            }
//        }
//    }
//}