using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Threading;

namespace DomainLayerTests
{
    [SetUpFixture]
    public class SetUpFixtureClass
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            //Process process = new Process();
            //process.StartInfo.FileName = "docker";
            //process.StartInfo.Arguments = "run -e ACCEPT_EULA=Y -e SA_PASSWORD=Aa123456@ -p 1433:1433 --name sql1 -d mcr.microsoft.com/mssql/server:2019-CU3-ubuntu-18.04";
            //process.Start();
            //process.WaitForExit();
            //Console.WriteLine("here");
            ////sleep for 10 sec to allow sqlserver to be up and ready
            //Thread.Sleep(1000);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            //Process process = new Process();
            //process.StartInfo.FileName = "docker";
            //process.StartInfo.Arguments = "kill sql1";
            //process.Start();
            //process.WaitForExit();
            //process = new Process();
            //process.StartInfo.FileName = "docker";
            //process.StartInfo.Arguments = "rm sql1";
            //process.Start();
            //process.WaitForExit();
        }
    }
}
