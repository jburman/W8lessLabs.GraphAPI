namespace W8lessLabs.GraphAPI.Logging
{
    public class DebugLoggerProvider : ILoggerProvider
    {
        public ILogger GetLogger()
        {
            return DebugLogger.Instance;
        }
    }
}
