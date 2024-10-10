using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;
using PZConfigurations;
using Serilog;
using Serilog.Logfmt;

namespace TopshelfSerilogConsole472
{
    class Program
    {
        static readonly string applicationName = "PlayerzeroApplicationService";
        static readonly string applicationTitle = "Playerzero Application Service";
        private static ActivitySource activitySource;

        static void Main(string[] args)
        {
            PZConfigurations.PZConfigurations.GetInstance.InitializePZOtel();

            var loggerConfig = new LoggerConfiguration()
                                    .ReadFrom.AppSettings()
                                    .Enrich.WithMachineName()
                                    .WriteTo.File(new LogfmtFormatter(), "logs\\log.txt");

            loggerConfig = PZConfigurations.PZConfigurations.GetInstance.ConfigureSerilogLoggerFactory(loggerConfig);

            Log.Logger = loggerConfig.CreateLogger();

            activitySource = new ActivitySource("My Dataset", "1.0.0");

            using (var activity = Activity.Current ?? activitySource.StartActivity("Main"))
            {
                HostFactory.Run(x =>
                {
                    x.Service<ApplicationService>(s =>
                    {
                        s.ConstructUsing(name => new ApplicationService());
                        s.WhenStarted(op => op.Start());
                        s.WhenStopped(op => op.Stop());
                    });
                    x.RunAsLocalSystem();

                    x.SetServiceName(applicationName);
                    x.SetDisplayName(applicationTitle);
                    x.SetDescription("This service manages background operations for the ApplicationService.");
                });
            }
        }
    }
}
