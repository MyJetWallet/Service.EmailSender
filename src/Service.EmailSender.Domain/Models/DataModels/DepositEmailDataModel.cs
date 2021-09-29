using Newtonsoft.Json;

namespace Service.EmailSender.Domain.Models.DataModels
{
    public class DepositEmailDataModel
    {
        [JsonProperty("text_Amount")]
        public decimal Amount { get; set; }

        [JsonProperty("firstname")]
        public string TraderName { get; set; }
    }
}
