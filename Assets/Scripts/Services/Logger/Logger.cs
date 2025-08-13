using System;
using UnityEngine;

namespace Services.Logger
{
    public class Logger : ILogger
    {
        public void Log(LogType type, string message)
        {
            Log(type, message, null);
        }

        public void Log(LogType type, string message, GameObject sender)
        {
            switch (type)
            {
                case LogType.Message:
                    LogMessage(message, sender);
                    break;
                case LogType.Warning:
                    LogWarning(message, sender);
                    break;
                case LogType.Exception:
                    LogException(message, sender);
                    break;
            }
        }

        public void Log(Exception exception)
        {
            Log(LogType.Exception, exception.ToString(), null);
        }

        private void LogMessage(string message, GameObject sender)
        {
            if (sender == null)
            {
                Debug.Log(message);
            }
            else
            {
                Debug.Log(message, sender);
            }
        }

        private void LogWarning(string message, GameObject sender)
        {
            if (sender == null)
            {
                Debug.LogWarning(message);
            }
            else
            {
                Debug.LogWarning(message, sender);
            }
        }

        private void LogException(string message, GameObject sender)
        {
            if (sender == null)
            {
                Debug.LogError(message);
            }
            else
            {
                Debug.LogError(message, sender);
            }
        }
    }
}