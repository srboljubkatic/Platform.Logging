namespace Platform.Logging.Tests.DependencyInjection.Builders
{
    using System;
    using System.Collections.Generic;
    using Logging.DependencyInjection.Builders;
    using Primitives.Enums;
    using Serilog;
    using Serilog.Events;
    using Xunit;

    public class LogBuilderTests
    {
        [Fact]
        public void DefaultValues_ShouldBeCorrect()
        {
            var logBuilder = new LogBuilder();

            Assert.False(logBuilder.EnableSourceContext);
            Assert.Equal(LogFormat.CompactJson, logBuilder.Format);
            Assert.Equal(LogLevel.Information, logBuilder.LogLevel);
            Assert.Empty(logBuilder.SourceContextOverrides);
        }

        [Theory]
        [InlineData(LogLevel.Debug, LogEventLevel.Debug)]
        [InlineData(LogLevel.Error, LogEventLevel.Error)]
        [InlineData(LogLevel.Fatal, LogEventLevel.Fatal)]
        [InlineData(LogLevel.Information, LogEventLevel.Information)]
        [InlineData(LogLevel.Warning, LogEventLevel.Warning)]
        [InlineData(LogLevel.Verbose, LogEventLevel.Verbose)]
        public void MapLogLevel_ShouldReturnCorrectLogEventLevel(LogLevel logLevel, LogEventLevel logEventLevel)
        {
            var logBuilder = new LogBuilder
            {
                LogLevel = logLevel,
                EnableSourceContext = false,
                Format = LogFormat.Text
            };

            var configuration = new LoggerConfiguration();
            logBuilder.Configure(configuration);

            var logger = configuration.CreateLogger();

            Assert.True(logger.IsEnabled(logEventLevel));
        }

        [Fact]
        public void MapLogLevel_Should_ThrowInvalidOperationException_WhenLogLevelIsInvalid()
        {
            var logBuilder = new LogBuilder
            {
                LogLevel = (LogLevel)999
            };

            var configuration = new LoggerConfiguration();
            Assert.Throws<InvalidOperationException>(() => logBuilder.Configure(configuration));
        }

        [Fact]
        public void Configure_ShouldSetMinimumLevel()
        {
            var logBuilder = new LogBuilder
            {
                LogLevel = LogLevel.Debug
            };

            var configuration = new LoggerConfiguration();
            logBuilder.Configure(configuration);

            var logger = configuration.CreateLogger();


            Assert.True(logger.IsEnabled(LogEventLevel.Debug));
            Assert.False(logger.IsEnabled(LogEventLevel.Verbose));
        }

        [Fact]
        public void Configure_ShouldOverrideSourceContextLevels()
        {
            var logBuilder = new LogBuilder
            {
                LogLevel = LogLevel.Fatal,
                SourceContextOverrides = new Dictionary<string, LogLevel>
                {
                    { "Platform.Logging.Tests.DependencyInjection.Builders.MyClass1", LogLevel.Error },
                    { "Platform.Logging.Tests.DependencyInjection.Builders.MyClass2", LogLevel.Warning }
                }
            };

            var configuration = new LoggerConfiguration();
            logBuilder.Configure(configuration);

            var logger = configuration.CreateLogger();

            Assert.True(logger.IsEnabled(LogEventLevel.Fatal));
            Assert.False(logger.IsEnabled(LogEventLevel.Error));

            Assert.True(logger.ForContext<MyClass1>().IsEnabled(LogEventLevel.Error));
            Assert.False(logger.ForContext<MyClass1>().IsEnabled(LogEventLevel.Warning));

            Assert.True(logger.ForContext<MyClass2>().IsEnabled(LogEventLevel.Warning));
            Assert.False(logger.ForContext<MyClass2>().IsEnabled(LogEventLevel.Information));
        }
    }

    internal class MyClass1
    {
    }

    internal class MyClass2
    {
    }
}