using MyYamlParser;

namespace Service.EmailSender.Domain.SettingModels
{
    public class BaseEmailSettings
    {
        [YamlProperty("SendGridTemplateId")]
        public string SendGridTemplateId { get; set; }

        [YamlProperty("Subject")]
        public string Subject { get; set; }
    }
}
