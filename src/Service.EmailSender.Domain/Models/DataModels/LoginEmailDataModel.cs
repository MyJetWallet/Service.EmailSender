using Newtonsoft.Json;

namespace Service.EmailSender.Domain.Models.DataModels
{
    public class LoginEmailDataModel
    {
        [JsonProperty("email")]
        public string Email { get; set; }
        
        [JsonProperty("ipAddress")]
        public string IpAddress { get; set; }
        
        [JsonProperty("loginTime")]
        public string LoginTime { get; set; }
    }
}
