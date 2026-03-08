using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Platform.Logging.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using Formatters;
    using Helpers;
    using Serilog.Events;
    using Serilog.Parsing;
    using Xunit;

    public class PlatformJsonFormatterTests
    {
        public static IEnumerable<object[]> GetLogFormatTestData =>
            new List<object[]>
            {
                new object[] { null },
                new object[] { new Exception() },
                new object[] { new ArgumentException("message") },
                new object[] { new Exception("message", new NullReferenceException()) }
            };

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void Constructor_ShouldInitializeCompact(bool logSourceContext, bool compact)
        {
            var formatter = new PlatformJsonFormatter(logSourceContext, compact);
            Assert.NotNull(formatter);
        }

        [Fact]
        public void Formatter_ShouldReturn_TransformedName_WhenCompact()
        {
            var formatter = new PlatformJsonFormatter(true);
            var logStr = formatter.FormatToJson(LogEventHelper.SampleLogEvent());
            Assert.NotNull(logStr);
            Assert.DoesNotContain("\"timestamp\"", logStr);
            Assert.Contains("\"@t\"", logStr);
            Assert.DoesNotContain("\"level\"", logStr);
            Assert.Contains("\"@l\"", logStr);
            Assert.DoesNotContain("\"msg\"", logStr);
            Assert.Contains("\"@m\"", logStr);
            Assert.DoesNotContain("\"exception\"", logStr);
            Assert.Contains("\"@x\"", logStr);
            Assert.DoesNotContain("scope", logStr);
            Assert.Contains("\"trace_id\"", logStr);
            Assert.Contains("\"span_id\"", logStr);
        }

        [Fact]
        public void Formatter_ShouldReturn_Name_WhenNotCompact()
        {
            var formatter = new PlatformJsonFormatter(false);
            var logStr = formatter.FormatToJson(LogEventHelper.SampleLogEvent());
            Assert.NotNull(logStr);
            Assert.Contains("\"timestamp\"", logStr);
            Assert.DoesNotContain("\"@t\"", logStr);
            Assert.Contains("\"level\"", logStr);
            Assert.DoesNotContain("\"@l\"", logStr);
            Assert.Contains("\"msg\"", logStr);
            Assert.DoesNotContain("\"@m\"", logStr);
            Assert.Contains("\"exception\"", logStr);
            Assert.DoesNotContain("\"@x\"", logStr);
            Assert.DoesNotContain("scope", logStr);
            Assert.Contains("\"trace_id\"", logStr);
            Assert.Contains("\"span_id\"", logStr);
        }

        [Fact]
        public void Formatter_ShouldReturn_LogSourceContext_WhenIncludeSourceContextTrue()
        {
            var formatter = new PlatformJsonFormatter(true, false);
            var logStr = formatter.FormatToJson(LogEventHelper.SampleLogEvent());
            Assert.NotNull(logStr);
            Assert.Contains("\"sourcecontext\"", logStr);
            Assert.DoesNotContain("scope", logStr);
        }

        [Fact]
        public void Formatter_ShouldReturn_LogSourceContext_WhenIncludeSourceContextFalse()
        {
            var formatter = new PlatformJsonFormatter(false, false);
            var logStr = formatter.FormatToJson(LogEventHelper.SampleLogEvent());
            Assert.NotNull(logStr);
            Assert.DoesNotContain("\"sourcecontext\"", logStr);
            Assert.DoesNotContain("scope", logStr);
        }

        [Theory]
        [MemberData(nameof(GetLogFormatTestData))]
        public void FormatMessage_IsValidJson(Exception exception)
        {
            var formatter = new PlatformJsonFormatter(false, false);
            var @event = new LogEvent(DateTimeOffset.Now, LogEventLevel.Debug, exception,
                new MessageTemplate(new List<MessageTemplateToken>()), new List<LogEventProperty>(),
                ActivityTraceId.CreateRandom(), ActivitySpanId.CreateRandom());

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);

            formatter.Format(@event, writer);
            writer.Flush();
            JsonSerializer.Deserialize<dynamic>(Encoding.UTF8.GetString(stream.ToArray()));
        }
    }
}