using Service.EmailSender.Domain.Models;

namespace Service.EmailSender.Domain.SettingModels
{
    public class EmailTemplateSettings : IEmailGrpcRequestContract
    {
        public EmailTemplateSettings(IEmailGrpcRequestContract emailRequest)
        {
            Brand = emailRequest.Brand;
            Platform = emailRequest.Platform;
            Lang = emailRequest.Lang;
            Email = emailRequest.Email;
        }

        public string Brand { get; set; }
        public string Platform { get; set; }
        public string Lang { get; set; }
        public string Email { get; set; }
    }
}
