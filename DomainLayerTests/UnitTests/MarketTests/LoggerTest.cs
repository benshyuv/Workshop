using System;
using System.Threading;
using NUnit.Framework;
using Logger = CustomLogger.Logger;

namespace DomainLayerTests.UnitTests
{
    public class LoggerTest
    {
        [SetUp]
        public void SetUp()
        {
            
        }

        [Test]
        public void sanity_logger_test_multpleThreads_doesNotFail()
        {
            long initial_complete = ThreadPool.CompletedWorkItemCount;
            for (int i = 0; i<30; i++)
            {
                ThreadPool.QueueUserWorkItem(LogThread, i);
            }

            while(ThreadPool.CompletedWorkItemCount < initial_complete + 30)
            {
                Thread.Sleep(20);
            }
        }

        private void LogThread(object state)
        {
           for(int i = 0; i<200; i++)
            {
                Logger.writeEvent(String.Format("Logger Thread testing thread: {0}, iteration {1}", (int)state, i));
            }
        }
    }
}
