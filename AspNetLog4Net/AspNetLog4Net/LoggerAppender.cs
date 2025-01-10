using log4net;
using log4net.Appender;
using log4net.Core;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace AspNetLog4Net
{
    public class LoggerAppender : TraceAppender
    {
        protected override void Append(LoggingEvent loggingEvent)
        {
            var loggerFactory = LogManager.GetRepository().Properties["ILoggerFactory"] as ILoggerFactory;
            var log = loggerFactory.CreateLogger(loggingEvent.LoggerName);

            var level = loggingEvent.Level;
            var message = loggingEvent.MessageObject.ToString();

            var activity = System.Diagnostics.Activity.Current;

            // Log with TraceId and SpanId
            using (log.BeginScope(new Dictionary<string, object>
            {
                { "TraceId", activity?.TraceId.ToString() ?? "N/A" },
                { "SpanId", activity?.SpanId.ToString() ?? "N/A" }
            }))
            {
                if (level >= Level.Error)
                {
                    log.LogError(message);
                }
                else if (level >= Level.Warn)
                {
                    log.LogWarning(message);
                }
                else if (level >= Level.Info)
                {
                    log.LogInformation(message);
                }
                else
                {
                    log.LogTrace(message);
                }
            }
            
        }
    }
}