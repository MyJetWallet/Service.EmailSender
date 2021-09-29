using MyYamlParser;

namespace Service.EmailSender.Domain.SettingModels
{
    public class SpotRecoveryEmailSettings : BaseEmailSettings
    {
        [YamlProperty("TokenExpires")]
        public string TokenExpires { get; set; }
    }
}
