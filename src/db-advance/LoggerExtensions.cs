using System.Linq;
using Castle.Core.Internal;
using Castle.Core.Logging;

namespace DbAdvance.Host
{
    public static class LoggerExtensions
    {
        public static readonly string Banner = new string('*', 80);

        public static void WriteBlankLine(this ILogger logger)
        {
            logger.Info(string.Empty);
        }

        public static void WriteBanner(this ILogger logger)
        {
            logger.Info(Banner);
        }

        public static void WriteBannerMessage(this ILogger logger, params string[] messages)
        {
            if (!messages.Any()) return;

            WriteBanner(logger);
            messages.ForEach(message => logger.Info(message));
            WriteBanner(logger);
        }
    }
}