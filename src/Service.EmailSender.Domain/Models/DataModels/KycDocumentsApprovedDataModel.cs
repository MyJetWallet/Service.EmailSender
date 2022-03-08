using Newtonsoft.Json;

namespace Service.EmailSender.Domain.Models.DataModels
{
    public class KycDocumentsApprovedDataModel
    {
        [JsonProperty("link")]
        public string Link { get; set; }
    }
}
