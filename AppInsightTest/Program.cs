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
        }
    }
}
