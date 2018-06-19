using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AppInsightTest
{
    public class ApiCallsAppInsights
    {
        static TelemetryConfiguration Configuration = TelemetryConfiguration.Active;

        public async Task SendAsync(string eventName, string requestUri)
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
                    var isNotException = true;
                    var result = new HttpResponseMessage();
                    try
                    {                       
                        result = await httpClient.GetAsync(requestUri);
                    }
                    catch (Exception ex)
                    {
                        isNotException = false;
                        telemetryClient.TrackException(ex);
                        Console.WriteLine($"{ex.Message}");
                    }
                    finally
                    {
                        var requestTelemetry = new RequestTelemetry
                        {
                            Name = eventName,
                            Success = isNotException ? true : false,
                            Timestamp = isNotException ? (DateTimeOffset)result.Headers.Date : DateTimeOffset.UtcNow,
                            ResponseCode = isNotException ? result.StatusCode.ToString() : HttpStatusCode.BadRequest.ToString(),
                            Url = new Uri(requestUri),
                        };

                        var appEvent = new Dictionary<string, string>
                        {
                            { "ApiUrl", requestUri },
                            { "TimeStamp",  isNotException ? result.Headers.Date.ToString() : DateTimeOffset.UtcNow.ToString() },
                            { "ResponseCode", isNotException ? result.StatusCode.ToString() : HttpStatusCode.BadRequest.ToString() },
                            { "IsSuccess",  isNotException ? result.IsSuccessStatusCode.ToString() : "false" },
                            { "AppVersion", isNotException ? result.Version.ToString() : "" }
                        };

                        telemetryClient.TrackEvent(eventName, appEvent);
                        telemetryClient.TrackRequest(requestTelemetry);

                        if (!isNotException)
                        {
                            Console.WriteLine($"Api Result Code: {result.StatusCode}");
                        }
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
