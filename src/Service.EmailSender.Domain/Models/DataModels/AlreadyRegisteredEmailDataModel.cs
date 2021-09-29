using Newtonsoft.Json;

namespace Service.EmailSender.Domain.Models.DataModels
{
    public class AlreadyRegisteredEmailDataModel
    {
        [JsonProperty("link")]
        public string Link { get; set; }
    }
}
