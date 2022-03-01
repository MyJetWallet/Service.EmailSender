using Newtonsoft.Json;

namespace Service.EmailSender.Domain.Models.DataModels
{
    public class RecoveryEmailDataModel
    {
        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("full_name")]
        public string TraderName { get; set; }
        
        [JsonProperty("code")]
        public string Code { get; set; }
    }
}
