using Newtonsoft.Json;

namespace Service.EmailSender.Domain.Models
{
    public class BaseTemplate
    {
        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("full_name")]
        public string Data { get; set; }
    }
}