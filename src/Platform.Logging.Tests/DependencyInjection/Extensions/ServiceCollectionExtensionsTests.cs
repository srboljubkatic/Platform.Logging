namespace Platform.Logging.Tests.DependencyInjection.Extensions
{
    using System;
    using System.Collections.Generic;
    using Logging.DependencyInjection.Builders;
    using Logging.DependencyInjection.Extensions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Xunit;
    using LogLevel = Primitives.Enums.LogLevel;

    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void ConfigureServiceHostLogging_ShouldApplyLogBuilderConfiguration()
        {
            // Arrange
            var hostBuilder = new HostBuilder();
            Action<LogBuilder> configureLogging = logBuilder =>
            {
                logBuilder.LogLevel = LogLevel.Error;
                logBuilder.EnableSourceContext = true;
            };

            // Act
            hostBuilder.ConfigureServiceHostLogging(configureLogging);
            var host = hostBuilder.Build();

            // Assert
            var loggerFactory = host.Services.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            Assert.NotNull(loggerFactory);

            var logger = loggerFactory.CreateLogger("TestLogger");
            Assert.NotNull(logger);

            Assert.True(logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Error));
            Assert.False(logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Warning));
        }

        [Fact]
        public void ConfigureServiceHostLogging_ShouldApplyLogBuilderConfiguration_HostBuilderContext()
        {
            // Arrange
            var hostBuilder = new HostBuilder();
            Action<HostBuilderContext, LogBuilder> configureLogging = (context, logBuilder) =>
            {
                logBuilder.LogLevel = LogLevel.Error;
                logBuilder.EnableSourceContext = true;
            };

            // Act
            hostBuilder.ConfigureServiceHostLogging(configureLogging);
            var host = hostBuilder.Build();

            // Assert
            var loggerFactory = host.Services.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            Assert.NotNull(loggerFactory);

            var logger = loggerFactory.CreateLogger("TestLogger");
            Assert.NotNull(logger);

            Assert.True(logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Error));
            Assert.False(logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Warning));
        }

        [Fact]
        public void ConfigureServiceHostLogging_ShouldConfigureLogging()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string>
            {
                { "Logging:LogLevel", "Error" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            var hostBuilder = new HostBuilder();
            var configurationSection = "Logging";

            hostBuilder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                configBuilder.AddConfiguration(configuration);
            });

            // Act
            hostBuilder.ConfigureServiceHostLogging(configurationSection);
            var host = hostBuilder.Build();

            // Assert
            var loggerFactory = host.Services.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            Assert.NotNull(loggerFactory);

            var logger = loggerFactory.CreateLogger("TestLogger");
            Assert.NotNull(logger);

            // Assert
            Assert.True(logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Error));
            Assert.False(logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Warning));
        }

        [Fact]
        public void ConfigureServiceHostLogging_ShouldConfigureLogging_HostBuilderContext()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string>
            {
                { "Logging:LogLevel", "Error" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            var hostBuilder = new HostBuilder();
            var configurationSection = "Logging";

            hostBuilder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                configBuilder.AddConfiguration(configuration);
            });

            Func<HostBuilderContext, IConfigurationSection> configureLogging = context =>
                context.Configuration.GetSection(configurationSection);

            // Act
            hostBuilder.ConfigureServiceHostLogging(configureLogging);
            var host = hostBuilder.Build();

            // Assert
            var loggerFactory = host.Services.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            Assert.NotNull(loggerFactory);

            var logger = loggerFactory.CreateLogger("TestLogger");
            Assert.NotNull(logger);

            // Assert
            Assert.True(logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Error));
            Assert.False(logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Warning));
        }
    }
}