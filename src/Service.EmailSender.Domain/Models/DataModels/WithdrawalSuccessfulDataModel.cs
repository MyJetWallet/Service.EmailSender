using Newtonsoft.Json;

namespace Service.EmailSender.Domain.Models.DataModels
{
    public class WithdrawalSuccessfulDataModel
    {
        [JsonProperty("asset")]
        public string AssetSymbol { get; set; }
        
        [JsonProperty("amount")]
        public string Amount { get; set; }
        
        [JsonProperty("fullName")]
        public string FullName { get; set; }
    }
}
