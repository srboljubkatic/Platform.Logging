namespace Platform.Logging.Formatters
{
    using System;
    using System.IO;
    using Serilog.Events;
    using Serilog.Formatting;
    using Serilog.Formatting.Json;

    /// <summary>
    ///     An <see cref="ITextFormatter" /> that outputs log entries in a JSON and Compact JSON format.
    /// </summary>
    public class PlatformJsonFormatter : ITextFormatter
    {
        private const string TraceId = "trace_id";
        private const string SpanId = "span_id";
        private readonly bool _compact;
        private readonly string _logEntryTimestampFormat = "yyyy-MM-dd HH:mm:ss.ffffff";
        private readonly bool _logSourceContext;

        /// <summary>
        ///     Construct a <see cref="PlatformJsonFormatter" /> instance.
        /// </summary>
        /// <param name="compact">Specify whether the JSON is in compact format or not.</param>
        public PlatformJsonFormatter(bool compact) : this(false, compact)
        {
        }

        /// <summary>
        ///     Construct a <see cref="PlatformJsonFormatter" /> instance.
        /// </summary>
        /// <param name="compact">Specify whether the JSON is in compact format or not.</param>
        /// <param name="logSourceContext">log the source context of the log entry or not.</param>
        public PlatformJsonFormatter(bool logSourceContext, bool compact)
        {
            _compact = compact;
            _logSourceContext = logSourceContext;
        }

        /// <summary>
        ///     Implements <see cref="ITextFormatter" />
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="output">The <see cref="TextWriter"/> to write the log entry text.</param>
        /// <returns>
        ///     Returns the fieldname initial letter prefixed with a @ symbol if the formatter
        ///     is configured to output compact JSON, otherwise returns the field name as it is.
        /// </returns>
        public void Format(LogEvent logEvent, TextWriter output)
        {
            var valueFormatter = new JsonValueFormatter();

            output.Write('{');

            output.Write($"\"{TransformPropName("timestamp")}\":\"");

            output.Write(logEvent.Timestamp.UtcDateTime.ToString(_logEntryTimestampFormat));
            output.Write('\"');
            output.Write(',');

            output.Write($"\"{TransformPropName("level")}\":\"");
            output.Write(logEvent.Level);
            output.Write('\"');
            output.Write(',');

            output.Write($"\"{TransformPropName("msg")}\":");
            JsonValueFormatter.WriteQuotedJsonString(logEvent.MessageTemplate.Render(logEvent.Properties), output);

            if (logEvent.Exception != null)
            {
                output.Write(',');
                output.Write($"\"{TransformPropName("exception", "x")}\":");
                JsonValueFormatter.WriteQuotedJsonString(logEvent.Exception.ToString(), output);
            }

            foreach (var property in logEvent.Properties)
            {
                var name = SanitizePropName(property.Key);
                if (name.Equals("Scope", StringComparison.CurrentCultureIgnoreCase))
                    continue;

                if (!_logSourceContext && name.Equals("sourcecontext", StringComparison.CurrentCultureIgnoreCase))
                    continue;

                output.Write(',');
                JsonValueFormatter.WriteQuotedJsonString(name, output);
                output.Write(':');
                valueFormatter.Format(property.Value, output);
            }

            if (logEvent.TraceId != null && logEvent.Properties != null && !logEvent.Properties.ContainsKey(TraceId))
            {
                output.Write(',');
                output.Write($"\"{TraceId}\":\"");
                output.Write(logEvent.TraceId.ToString());
                output.Write('\"');
            }

            if (logEvent.SpanId != null && logEvent.Properties != null && !logEvent.Properties.ContainsKey(SpanId))
            {
                output.Write(',');
                output.Write($"\"{SpanId}\":\"");
                output.Write(logEvent.SpanId.ToString());
                output.Write('\"');
            }

            output.Write('}');
            output.WriteLine();
        }

        /// <summary>
        ///     Private method to get the property name.
        /// </summary>
        /// <param name="propName">The field name</param>
        /// <param name="compactPropName">Optional: the compact field name</param>
        /// <returns>
        ///     Returns the property's initial letter prefixed with a @ symbol if the formatter
        ///     is configured to output compact JSON, otherwise returns the field name as it is.
        /// </returns>
        private string TransformPropName(string propName, string compactPropName = null)
        {
            if (_compact) return "@" + (string.IsNullOrWhiteSpace(compactPropName) ? propName.Substring(0, 1) : compactPropName);
            return propName;
        }

        /// <summary>
        ///     Sanitize the property name
        /// </summary>
        /// <param name="propName">The property name to be sanitized.</param>
        /// <returns>
        ///     Returns the sanitized property name.
        /// </returns>
        private string SanitizePropName(string propName)
        {
            if (propName.StartsWith("@")) propName = propName.Substring(1);
            return propName.ToLower();
        }
    }
}