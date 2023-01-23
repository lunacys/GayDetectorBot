namespace GayDetectorBot.WebApi.Logging
{
    public struct LogMessage
    {
        public readonly string LogName;
        public readonly string Message;
        public readonly LogLevel LogLevel;
        public readonly EventId EventId;
        public readonly Exception Exception;

        internal LogMessage(string logName, LogLevel level, EventId eventId, string message, Exception ex)
        {
            LogName = logName;
            Message = message;
            LogLevel = level;
            EventId = eventId;
            Exception = ex;
        }

    }
}