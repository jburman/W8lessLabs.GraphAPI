using System;
using System.Diagnostics;

namespace W8lessLabs.GraphAPI.Logging
{
    public class ConsoleLogger : ILogger
    {
        public static readonly ConsoleLogger Instance = new ConsoleLogger();

        public void Error(string message, params object[] values) =>
            Console.WriteLine("ERROR: " + message, values);

        public void Error(Exception ex) =>
            Console.WriteLine("ERROR: " + ex.ToString());

        public void Info(string message, params object[] values) =>
            Console.WriteLine("INFO: " + message, values);

        public void Trace(string message, params object[] values) =>
            Console.WriteLine("TRACE: " + message, values);
    }
}
