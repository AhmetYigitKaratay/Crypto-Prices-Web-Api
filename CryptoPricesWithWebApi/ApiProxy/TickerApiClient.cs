using System.Text.Json;

namespace CryptoPricesWithWebApi.ApiProxy
{
    public class TickerApiClient
    {
        public static async Task<TickerResponse> GetTicker()
        {
            using HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync("https://api.btcturk.com/api/v2/ticker");

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                TickerResponse ticker =
                JsonSerializer.Deserialize<TickerResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive= true});

                return ticker;
            }
            else
            {
                return null;
            }         
        }  
    }
}
