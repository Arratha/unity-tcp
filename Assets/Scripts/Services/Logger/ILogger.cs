using System;

namespace Services.Logger
{
    public enum LogType
    {
        Message,
        Warning,
        Exception
    }

    public interface ILogger
    {
        public void Log(LogType type, string message);

        public void Log(Exception exception);
    }
}