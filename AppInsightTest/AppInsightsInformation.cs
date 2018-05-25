using System.Net.Http;
using System.Threading.Tasks;

namespace AppInsightTest
{
    public class AppInsightsInformation
    {
        public async Task<string> GetAsync(string queryString)
        {
            using (var httpClient = new HttpClient())
            {
                var requestMessage = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"{AppConst.AppInsightApiUri}{AppConst.ApplicationId}/query?query={queryString}");
                requestMessage.Headers.Add("x-api-key", $"{AppConst.AppInsightsApiKey}");
                var result = await httpClient.SendAsync(requestMessage);
                return await result.Content.ReadAsStringAsync();
            }
        }
    }
}
