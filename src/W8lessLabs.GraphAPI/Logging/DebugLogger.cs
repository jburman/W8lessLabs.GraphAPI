using System;
using System.Diagnostics;

namespace W8lessLabs.GraphAPI.Logging
{
    public class DebugLogger : ILogger
    {
        public static readonly DebugLogger Instance = new DebugLogger();

        public void Error(string message, params object[] values) =>
            Debug.WriteLine("ERROR: " + message, values);

        public void Error(Exception ex) =>
            Debug.WriteLine("ERROR: " + ex.ToString());

        public void Info(string message, params object[] values) =>
            Debug.WriteLine("INFO: " + message, values);

        public void Trace(string message, params object[] values) =>
            Debug.WriteLine("TRACE: " + message, values);
    }
}
