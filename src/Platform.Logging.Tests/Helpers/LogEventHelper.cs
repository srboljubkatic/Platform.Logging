namespace Platform.Logging.Tests.Helpers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using Serilog.Events;
    using Serilog.Formatting;
    using Serilog.Parsing;

    public static class LogEventHelper
    {
        private const string DefaultOutputTemplate =
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}";

        public static LogEvent SampleLogEvent()
        {
            MessageTemplateToken[] message =
            [
                new TextToken("Hello, world!")
            ];
            LogEventProperty[] properties =
            [
                new LogEventProperty("sourcecontext", new ScalarValue("LogSourceContext")),
                new LogEventProperty("@sanitize", new ScalarValue("sanitizeProp")),
                new LogEventProperty("Scope", new ScalarValue("skipScope"))
            ];

            return new LogEvent(
                DateTimeOffset.Now,
                LogEventLevel.Information,
                new ArgumentException(),
                new MessageTemplate(DefaultOutputTemplate, message),
                properties,
                ActivityTraceId.CreateRandom(),
                ActivitySpanId.CreateRandom());
        }

        public static string FormatToJson(this ITextFormatter formatter, LogEvent @event)
        {
            var output = new StringWriter();
            formatter.Format(@event, output);
            return output.ToString();
        }
    }
}