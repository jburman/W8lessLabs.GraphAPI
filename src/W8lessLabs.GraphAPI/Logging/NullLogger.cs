using System;

namespace W8lessLabs.GraphAPI.Logging
{
    public class NullLogger : ILogger
    {
        public static readonly NullLogger Instance = new NullLogger();

        public void Error(string message, params object[] values)
        {
        }

        public void Error(Exception ex)
        {
        }

        public void Info(string message, params object[] values)
        {
        }

        public void Trace(string message, params object[] values)
        {
        }
    }
}
