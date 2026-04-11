namespace RawPowerLabs.DynamicAI
{
    public readonly struct LogEvent
    {
        public LogEvent(LogLevel level, string message)
        {
            Level = level;
            Message = message;
        }

        public LogLevel Level { get; }

        public string Message { get; }

        public void Deconstruct(out LogLevel level, out string message)
        {
            level = Level;
            message = Message;
        }
    }
}