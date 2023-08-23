namespace CryptoPricesWithWebApi.ApiProxy
{
    public class TickerResponse
    {
        public int Code { get; set; }
        public string Message { get; set; } 
        public bool Success { get; set; }   
        public List<PairTickerDetail> Data { get; set; }   

    }
}
