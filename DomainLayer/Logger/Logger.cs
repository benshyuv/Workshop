using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;

namespace CustomLogger
{

    public class Logger
    {
        private static string event_log = @"event_log.txt";
        private static string error_log = @"error_log.txt";
        private static Mutex mutex = new Mutex();

        public static void writeEvent(string details)
        {
            string time = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();
            string output = time + ":   " + details + " done Successfully";
            mutex.WaitOne();
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(event_log, true))
            {
                file.WriteLine(output);
            }
            mutex.ReleaseMutex();
        }
        public static void writeError(Exception info)
        {
            StackTrace st = new StackTrace();
            string time = DateTime.Now.ToLongDateString() + " ";
            string output = time + ":ERROR:  " + info.Message + " at: " + info.StackTrace;
            mutex.WaitOne();
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(error_log, true))
            {
                file.WriteLine(output);
            }
            mutex.ReleaseMutex();
        }

        public static void WriteWarning(Exception info)
        {
            StackTrace st = new StackTrace();
            string time = DateTime.Now.ToLongDateString() + " ";
            string output = time + ": WARNING: Operation Failed: " + info.Message + " at: " + info.StackTrace;
            mutex.WaitOne();
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(event_log, true))
            {
                file.WriteLine(output);
            }
            mutex.ReleaseMutex();
        }

    }
}