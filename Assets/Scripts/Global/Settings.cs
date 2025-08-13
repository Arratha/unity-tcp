namespace Global
{
    public static class Settings
    {
        public static class Network
        {
            public const string LocalIp = "127.0.0.1";

            public const int MaxMessageSize = 10 * 1024 * 1024;

            public const int SendTimeout = 5000;
            public const int ReceiveTimeout = 5000;

            public const int SendBatch = 10;
            public const int ReceiveBatch = 10;

            public const int ServerProcessIntervalMs = 100;
            public const int ClientProcessIntervalMs = 100;

            public static class Heartbeat
            {
                public const int SentIntervalMs = 2500;
                public const int ReceiveIntervalMs = 10000;
                public const int InitialDelayMs = 30000;
            }
        }
    }
}