namespace Platform.Logging.Tests.Helpers
{
    using System.IO;
    using Serilog.Core;
    using Serilog.Events;
    using Serilog.Formatting;

    public class StringHelperSink : ILogEventSink
    {
        private readonly ITextFormatter _formatter;
        private readonly StringWriter _sw = new();

        public StringHelperSink(ITextFormatter formatter)
        {
            _formatter = formatter;
        }

        public void Emit(LogEvent logEvent)
        {
            _formatter.Format(logEvent, _sw);
        }

        public override string ToString()
        {
            return _sw.ToString();
        }
    }
}