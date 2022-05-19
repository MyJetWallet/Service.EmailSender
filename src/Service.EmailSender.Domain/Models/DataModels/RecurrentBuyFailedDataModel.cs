using Newtonsoft.Json;

namespace Service.EmailSender.Domain.Models.DataModels
{
    public class RecurrentBuyFailedDataModel
    {
        [JsonProperty("assetSymbol")]
        public string ToAsset { get; set; }

        [JsonProperty("fromAmount")]
        public decimal FromAmount { get; set; }
        
        [JsonProperty("fromAssetSymbol")]
        public string FromAsset { get; set; }
        
        [JsonProperty("failTime")]
        public string FailTime { get; set; }

        [JsonProperty("failReason")]
        public string FailureReason { get; set; }
    }
}
