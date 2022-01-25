using Newtonsoft.Json;

namespace Service.EmailSender.Domain.Models.DataModels
{
    public class WithdrawalVerificationDataModel
    {
        [JsonProperty("code")]
        public string Code { get; set; }
        
        [JsonProperty("link")]
        public string Link { get; set; }
        
        [JsonProperty("assetSymbol")]
        public string AssetSymbol { get; set; }
        
        [JsonProperty("amount")]
        public string Amount { get; set; }
        
        [JsonProperty("destinationAddress")]
        public string DestinationAddress { get; set; }
        
        [JsonProperty("ipAddress")]
        public string IpAddress { get; set; }
        
        [JsonProperty("feeAssetSymbol")]
        public string FeeAssetSymbol { get; set; }
        
        [JsonProperty("feeAmount")]
        public string FeeAmount { get; set; }
        
        [JsonProperty("receiveAmount")]
        public string ReceiveAmount { get; set; }
    }
}
