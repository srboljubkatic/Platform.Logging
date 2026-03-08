namespace Platform.Logging.Extensions
{
    using System;
    using Formatters;
    using Primitives.Enums;
    using Serilog.Formatting;

    /// <summary>
    ///     Helpers for <see cref="ITextFormatter" />
    /// </summary>
    public static class LogFormatExtensions
    {
        /// <summary>
        ///     Gets the <see cref="ITextFormatter" /> for a given <see cref="LogFormat" /> and the option to log the source
        ///     context
        /// </summary>
        /// <param name="logFormat">An <see cref="LogFormat" /></param>
        /// <param name="logSourceContext">log the source context of the log entry or not.</param>
        /// <returns>
        ///     An instance of <see cref="ITextFormatter" />
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Returns an exception if an invalid format if <see cref="LogFormat" /> is invalid
        /// </exception>
        public static ITextFormatter GetFormatter(this LogFormat logFormat, bool logSourceContext)
        {
            switch (logFormat)
            {
                case LogFormat.Text:
                    return null;
                case LogFormat.Json:
                    return new PlatformJsonFormatter(logSourceContext, false);
                case LogFormat.CompactJson:
                    return new PlatformJsonFormatter(logSourceContext, true);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}