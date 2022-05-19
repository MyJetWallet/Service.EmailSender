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
        
        [JsonProperty("phoneModel")]
        public string PhoneModel { get; set; }
        
        [JsonProperty("location")]
        public string Location { get; set; }
    }
}
