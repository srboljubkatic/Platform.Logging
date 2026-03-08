namespace Platform.Logging.DependencyInjection.Builders
{
    using System;
    using System.Collections.Generic;
    using Logging.Extensions;
    using Primitives.Enums;
    using Serilog;
    using Serilog.Core;
    using Serilog.Events;

    /// <summary>
    ///     Builder for configuring the Logger
    /// </summary>
    public class LogBuilder
    {
        /// <summary>
        ///     Get or set the sourcecontext value in the logs
        /// </summary>
        public bool EnableSourceContext { get; set; } = false;

        /// <summary>
        ///     Get or set the builder default log entry formatter.
        /// </summary>
        public LogFormat Format { get; set; } = LogFormat.CompactJson;

        /// <summary>
        ///     Get or set the minimum log level.
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Information;

        /// <summary>
        ///     Overrides the defined <see cref="LogLevel" /> for the source contexts defined
        /// </summary>
        public IDictionary<string, LogLevel> SourceContextOverrides { get; set; } = new Dictionary<string, LogLevel>();

        private static LogEventLevel MapLogLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Debug:
                    return LogEventLevel.Debug;
                case LogLevel.Error:
                    return LogEventLevel.Error;
                case LogLevel.Fatal:
                    return LogEventLevel.Fatal;
                case LogLevel.Information:
                    return LogEventLevel.Information;
                case LogLevel.Warning:
                    return LogEventLevel.Warning;
                case LogLevel.Verbose:
                    return LogEventLevel.Verbose;
                default:
                    throw new InvalidOperationException(nameof(logLevel));
            }
        }

        /// <summary>
        ///     Configure the log.
        /// </summary>
        /// <param name="configuration">The logger configuration</param>
        internal void Configure(LoggerConfiguration configuration)
        {
            configuration.Enrich.FromLogContext();

            var levelSwitch = new LoggingLevelSwitch
            {
                MinimumLevel = MapLogLevel(LogLevel)
            };

            configuration.MinimumLevel.ControlledBy(levelSwitch);

            foreach (var contextOverride in SourceContextOverrides)
                configuration.MinimumLevel.Override(contextOverride.Key, MapLogLevel(contextOverride.Value));

            var formatter = Format.GetFormatter(EnableSourceContext);
            if (formatter == null)
                configuration.WriteTo.Console();
            else
                configuration.WriteTo.Console(formatter);
        }
    }
}