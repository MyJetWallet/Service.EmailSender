using Newtonsoft.Json;

namespace Service.EmailSender.Domain.Models.DataModels
{
    public class KycDocumentsDeclinedDataModel
    {
        [JsonProperty("link")]
        public string Link { get; set; }
    }
}
