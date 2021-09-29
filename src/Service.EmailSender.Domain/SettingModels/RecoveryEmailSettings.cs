using MyYamlParser;

namespace Service.EmailSender.Domain.SettingModels
{
    public class RecoveryEmailSettings : BaseEmailSettings
    {
        [YamlProperty("Url")]
        public string Url { get; set; }

        [YamlProperty("TokenExpires")]
        public string TokenExpires { get; set; }
    }
}
