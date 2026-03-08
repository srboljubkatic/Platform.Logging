namespace Platform.Logging.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Formatters;
    using Helpers;
    using Microsoft.Extensions.Logging;
    using Primitives.Constants;
    using Serilog;
    using Serilog.Extensions.Logging;
    using Xunit;

    public class LoggerTests
    {
        [Fact]
        public void Logger_Should_Be_Able_To_Be_Logged()
        {
            var sink = new StringHelperSink(new PlatformJsonFormatter(true));
            var serilogLogger = new LoggerConfiguration()
                .WriteTo.Sink(sink)
                .CreateLogger();
            var microsoftLogger = new SerilogLoggerFactory(serilogLogger)
                .CreateLogger<LoggerTests>();


            microsoftLogger.LogInformation("Hello World!");

            var logStr = sink.ToString();

            Assert.Contains("Hello World!", logStr);
        }

        [Fact]
        public void Logger_Should_Be_Able_To_Log_Scope()
        {
            var tenantId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var sink = new StringHelperSink(new PlatformJsonFormatter(true));
            var serilogLogger = new LoggerConfiguration()
                .WriteTo.Sink(sink)
                .CreateLogger();
            var microsoftLogger = new SerilogLoggerFactory(serilogLogger)
                .CreateLogger<LoggerTests>();

            using var loggerScope = microsoftLogger.BeginScope(new Dictionary<string, object>
            {
                { Constants.Fields.TenantId, tenantId },
                { Constants.Fields.UserId, userId }
            });

            microsoftLogger.LogInformation("Hello World!");

            var logStr = sink.ToString();

            Assert.Contains("Hello World!", logStr);
            Assert.Contains($"\"tid\":\"{tenantId.ToString()}\"", logStr);
            Assert.Contains($"\"uid\":\"{userId.ToString()}\"", logStr);
        }

        [Fact]
        public void Logger_Should_Be_Able_To_Log_Telemetry()
        {
            using var listener = new ActivityListener();
            listener.ShouldListenTo = _ => true;
            listener.Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData;
            ActivitySource.AddActivityListener(listener);

            using var source = new ActivitySource(nameof(LoggerTests));
            using var activity = source.StartActivity();
            Assert.NotNull(activity);
            Assert.NotEqual("00000000000000000000000000000000", activity.TraceId.ToHexString());
            Assert.NotEqual("0000000000000000", activity.SpanId.ToHexString());

            var sink = new StringHelperSink(new PlatformJsonFormatter(true));
            var serilogLogger = new LoggerConfiguration()
                .WriteTo.Sink(sink)
                .CreateLogger();
            var microsoftLogger = new SerilogLoggerFactory(serilogLogger)
                .CreateLogger<LoggerTests>();


            microsoftLogger.LogInformation("Hello World!");

            var logStr = sink.ToString();

            Assert.Contains("Hello World!", logStr);
            Assert.Contains($"\"trace_id\":\"{activity.TraceId.ToString()}\"", logStr);
            Assert.Contains($"\"span_id\":\"{activity.SpanId.ToString()}\"", logStr);
        }
    }
}