using Newtonsoft.Json;

namespace Service.EmailSender.Domain.Models.DataModels
{
    public class TransferVerificationDataModel
    {
        [JsonProperty("code")]
        public string Code { get; set; }
        
        [JsonProperty("link")]
        public string Link { get; set; }
        
        [JsonProperty("assetSymbol")]
        public string AssetSymbol { get; set; }
        
        [JsonProperty("amount")]
        public string Amount { get; set; }
        
        [JsonProperty("destinationPhone")]
        public string DestinationPhone { get; set; }
        
        [JsonProperty("ipAddress")]
        public string IpAddress { get; set; }
        
        [JsonProperty("timeTrans")]
        public string TimeTrans { get; set; }
        
        [JsonProperty("phoneModel")]
        public string PhoneModel { get; set; }
        
        [JsonProperty("location")]
        public string Location { get; set; }
    }
}
