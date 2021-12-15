using Newtonsoft.Json;

namespace Service.EmailSender.Domain.Models.DataModels
{
    public class DepositSuccessfulDataModel
    {
        [JsonProperty("asset")]
        public string AssetSymbol { get; set; }
        
        [JsonProperty("amount")]
        public string Amount { get; set; }
    }
}
