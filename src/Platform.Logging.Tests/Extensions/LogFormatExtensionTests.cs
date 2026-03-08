namespace Platform.Logging.Tests.Extensions
{
    using System;
    using Formatters;
    using Helpers;
    using Logging.Extensions;
    using Primitives.Enums;
    using Xunit;

    public class LogFormatExtensionTests
    {
        [Fact]
        public void GetFormatter_TextFormat_ReturnsNull()
        {
            // Arrange
            var logFormat = LogFormat.Text;

            // Act
            var result = logFormat.GetFormatter(false);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetFormatter_JsonFormat_ReturnsPlatformJsonFormatter()
        {
            // Arrange
            var logFormat = LogFormat.Json;

            // Act
            var result = logFormat.GetFormatter(false);

            // Assert
            Assert.IsType<PlatformJsonFormatter>(result);
            var formatter = result as PlatformJsonFormatter;
            Assert.NotNull(formatter);

            var logStr = formatter.FormatToJson(LogEventHelper.SampleLogEvent());
            Assert.NotNull(logStr);
            Assert.Contains("\"timestamp\"", logStr);
            Assert.DoesNotContain("\"@t\"", logStr);
        }

        [Fact]
        public void GetFormatter_CompactJsonFormat_ReturnsPlatformJsonFormatter()
        {
            // Arrange
            var logFormat = LogFormat.CompactJson;

            // Act
            var result = logFormat.GetFormatter(false);

            // Assert
            Assert.IsType<PlatformJsonFormatter>(result);
            var formatter = result as PlatformJsonFormatter;
            Assert.NotNull(formatter);

            var logStr = formatter.FormatToJson(LogEventHelper.SampleLogEvent());
            Assert.NotNull(logStr);
            Assert.DoesNotContain("\"timestamp\"", logStr);
            Assert.Contains("\"@t\"", logStr);
        }

        [Fact]
        public void GetFormatter_InvalidFormat_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var logFormat = (LogFormat)999; // Invalid enum value

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => logFormat.GetFormatter(false));
        }
    }
}