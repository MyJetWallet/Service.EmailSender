using Newtonsoft.Json;

namespace Service.EmailSender.Domain.Models.DataModels
{
	public class JobCvPositionSubmitDataModel
	{
		[JsonProperty("applicantName")]
		public string ApplicantName { get; set; }

		[JsonProperty("positionTitle")]
		public string PositionTitle { get; set; }
	}
}