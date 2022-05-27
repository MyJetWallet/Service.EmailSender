using Newtonsoft.Json;

namespace Service.EmailSender.Domain.Models.DataModels
{
    public class WithdrawalCancelledDataModel
    {
        [JsonProperty("asset")]
        public string AssetSymbol { get; set; }
        
        [JsonProperty("amount")]
        public string Amount { get; set; }
        
        [JsonProperty("transID")]
        public string TransId { get; set; }

        [JsonProperty("timeTrans")]
        public string TimeTrans { get; set; }
    }
}
