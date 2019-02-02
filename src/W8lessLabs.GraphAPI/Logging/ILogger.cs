using System;

namespace W8lessLabs.GraphAPI
{
    public interface ILogger
    {
        void Trace(string message, params object[] values);
        void Info(string message, params object[] values);
        void Error(string message, params object[] values);
        void Error(Exception ex);
    }
}
