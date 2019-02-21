namespace W8lessLabs.GraphAPI.Logging
{
    public class ConsoleLoggerProvider : ILoggerProvider
    {
        public ILogger GetLogger()
        {
            return ConsoleLogger.Instance;
        }
    }
}
