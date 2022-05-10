using MyYamlParser;

namespace Service.EmailSender.Domain.SettingModels
{
	public class JobCvPositionSubmitEmailSettings: BaseEmailSettings
	{
		[YamlProperty("From")]
		public string From { get; set; }
	}
}