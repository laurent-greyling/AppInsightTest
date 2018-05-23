using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AppInsightTest
{
    class Program
    {
        static TelemetryConfiguration Configuration = TelemetryConfiguration.Active;

        static async Task Main(string[] args)
        {
            Configuration.InstrumentationKey = AppConst.InstrumentationKey;
            Configuration.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());
            Configuration.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());

            var telemetryClient = new TelemetryClient(Configuration)
            {
                InstrumentationKey = Configuration.InstrumentationKey
            };

            using (InitializeDependencyTracking(Configuration))
            {
                //this should be automatically tracked
                using (var httpClient = new HttpClient())
                {
                    try
                    {
                        var weatherKey = AppConst.WeatherApiKey;
                        var weatherResult = await httpClient.GetAsync($"http://api.openweathermap.org/data/2.5/weather?q=Amsterdam,NLD&APPID={weatherKey}");

                        var w = new RequestTelemetry
                        {
                            Name = "Weather",
                            Success = weatherResult.StatusCode == HttpStatusCode.OK,
                            Timestamp = (DateTimeOffset)weatherResult.Headers.Date,
                            ResponseCode = weatherResult.StatusCode.ToString(),
                            Url = new Uri("http://api.openweathermap.org/data/2.5"),
                        };

                        telemetryClient.TrackRequest(w);
                        Console.WriteLine($"Weather Api Result Code: {weatherResult.StatusCode}");

                        var currencyResult = await httpClient.GetAsync($"http://api.fixer.io/latest");
                        var c = new RequestTelemetry
                        {
                            Name = "Currency",
                            Success = currencyResult.StatusCode == HttpStatusCode.OK,
                            Timestamp = (DateTimeOffset)currencyResult.Headers.Date,
                            ResponseCode = currencyResult.StatusCode.ToString(),
                            Url = new Uri("http://api.fixer.io/latest")
                        };

                        telemetryClient.TrackRequest(c);

                        Console.WriteLine($"Currency Api Result Code: {currencyResult.StatusCode}");

                        var noneResult = await httpClient.GetAsync($"http://api.idonotexist.nl");
                    }
                    catch (Exception ex)
                    {
                        telemetryClient.TrackTrace("Hallo World");
                        telemetryClient.TrackException(ex);
                        Console.WriteLine($"{ex.Message}");
                    }

                    telemetryClient.Flush();

                    // flush is not blocking so wait a bit
                    await Task.Delay(5000);
                    Console.WriteLine("finished");

                    Console.ReadKey();
                }
            }
        }

        /// <summary>
        /// Copy Paste Job
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private static DependencyTrackingTelemetryModule InitializeDependencyTracking(TelemetryConfiguration configuration)
        {
            var module = new DependencyTrackingTelemetryModule();

            // prevent Correlation Id to be sent to certain endpoints. You may add other domains as needed.
            //module.ExcludeComponentCorrelationHttpHeadersOnDomains.Add("core.windows.net");
            module.ExcludeComponentCorrelationHttpHeadersOnDomains.Add("core.chinacloudapi.cn");
            module.ExcludeComponentCorrelationHttpHeadersOnDomains.Add("core.cloudapi.de");
            module.ExcludeComponentCorrelationHttpHeadersOnDomains.Add("core.usgovcloudapi.net");
            //module.ExcludeComponentCorrelationHttpHeadersOnDomains.Add("localhost");
            //module.ExcludeComponentCorrelationHttpHeadersOnDomains.Add("127.0.0.1");

            // enable known dependency tracking, note that in future versions, we will extend this list. 
            // please check default settings in https://github.com/Microsoft/ApplicationInsights-dotnet-server/blob/develop/Src/DependencyCollector/NuGet/ApplicationInsights.config.install.xdt#L20
            module.IncludeDiagnosticSourceActivities.Add("Microsoft.Azure.ServiceBus");
            module.IncludeDiagnosticSourceActivities.Add("Microsoft.Azure.EventHubs");

            // initialize the module
            module.Initialize(configuration);

            return module;
        }
    }   
}
