using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppInsightTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (AppConst.SendApiCall)
            {
                var sendApiCall =  new SendApiCallsAppInsights();
                await sendApiCall.SendAsync("AppEvent", $"http://api.openweathermap.org/data/2.5/weather?q=Amsterdam,NLD&APPID={AppConst.WeatherApiKey}");
                await sendApiCall.SendAsync("AppEvent", $"http://data.fixer.io/api/latest?access_key={AppConst.CurrencyApiKey}");
                await sendApiCall.SendAsync("AppEvent", $"http://api.idonotexist.nl");
            }

            //TODO: this does not work completely yet, will get the json but not deserialise
            //Add more to the string by looking what you might need in ConstQueryString e.g {ConstQueryString.AllDependencies}{ConstQueryString.Count}
            var queryString = $"{ConstQueryString.JoinWithDataTable}";

            var result = await new AppInsightsInformation().GetAsync(queryString);
            var f = JsonConvert.DeserializeObject(result);

            var s = JsonConvert.DeserializeObject<List<TableModel>>(result);
            var t = JsonConvert.DeserializeObject<TableModel[]>(result);
        }
    }
}
