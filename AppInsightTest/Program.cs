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
                await sendApiCall.SendAsync();
            }

            //Add more to the string by looking what you might need in ConstQueryString e.g {ConstQueryString.AllDependencies}{ConstQueryString.Count}
            var queryString = $"{ConstQueryString.JoinWithDataTable}";
            var result = await new AppInsightsInformation().GetAsync(queryString);
        }
    }
}
