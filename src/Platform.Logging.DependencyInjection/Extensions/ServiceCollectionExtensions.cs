namespace Platform.Logging.DependencyInjection.Extensions
{
    using System;
    using Builders;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Serilog;

    /// <summary>
    ///     Provides extension methods for the <see cref="IHostBuilder" /> from the hosting package.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Configures the default <see cref="Microsoft.Extensions.Logging.ILogger" /> for platform logging
        /// </summary>
        /// <param name="builder">The <see cref="IHostBuilder" /></param>
        /// <param name="configureLogging">An action for configuring the Logger.</param>
        /// <returns>A <see cref="IHostBuilder" /></returns>
        public static IHostBuilder ConfigureServiceHostLogging(this IHostBuilder builder,
            Action<LogBuilder> configureLogging)
        {
            var logBuilder = new LogBuilder();
            configureLogging(logBuilder);

            builder.ConfigureLogging((context, loggingBuilder) => { loggingBuilder.ClearProviders(); });

            builder.UseSerilog((context, configuration) => { logBuilder.Configure(configuration); });

            return builder;
        }

        /// <summary>
        ///     Configures the default <see cref="Microsoft.Extensions.Logging.ILogger" /> for platform logging
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureLogging"></param>
        /// <returns></returns>
        public static IHostBuilder ConfigureServiceHostLogging(this IHostBuilder builder,
            Action<HostBuilderContext, LogBuilder> configureLogging)
        {
            var logBuilder = new LogBuilder();

            builder.ConfigureLogging((context, loggingBuilder) =>
            {
                configureLogging(context, logBuilder);
                loggingBuilder.ClearProviders();
            });

            builder.UseSerilog((context, configuration) => { logBuilder.Configure(configuration); });

            return builder;
        }

        /// <summary>
        ///     Configures the default <see cref="Microsoft.Extensions.Logging.ILogger" /> for platform logging
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configurationSection"></param>
        /// <returns></returns>
        public static IHostBuilder ConfigureServiceHostLogging(this IHostBuilder builder, string configurationSection)
        {
            var logBuilder = new LogBuilder();

            builder.ConfigureLogging((context, loggingBuilder) => { loggingBuilder.ClearProviders(); });

            builder.UseSerilog((context, configuration) =>
            {
                var config = context.Configuration.GetSection(configurationSection);
                config.Bind(logBuilder);
                logBuilder.Configure(configuration);
            });

            return builder;
        }

        /// <summary>
        ///     Configures the default <see cref="Microsoft.Extensions.Logging.ILogger" /> for platform logging
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configurationSection"></param>
        /// <returns></returns>
        public static IHostBuilder ConfigureServiceHostLogging(this IHostBuilder builder,
            Func<HostBuilderContext, IConfigurationSection> configurationSection)
        {
            var logBuilder = new LogBuilder();

            builder.ConfigureLogging((context, loggingBuilder) => { loggingBuilder.ClearProviders(); });

            builder.UseSerilog((context, configuration) =>
            {
                var config = configurationSection(context);
                config.Bind(logBuilder);
                logBuilder.Configure(configuration);
            });

            return builder;
        }
    }
}