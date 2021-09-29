using Newtonsoft.Json;

namespace Service.EmailSender.Domain.Models.DataModels
{
    public class ConfirmEmailDataModel
    {
        [JsonProperty("link")]
        public string Link { get; set; }
    }
}
