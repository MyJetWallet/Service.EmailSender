using Newtonsoft.Json;

namespace Service.EmailSender.Domain.Models.DataModels
{
	public class ClientOfferTerminateDataModel
	{
		[JsonProperty("asset")]
		public string AssetSymbol { get; set; }
        
		[JsonProperty("amountReturnned")]
		public string Amount { get; set; }
        
		[JsonProperty("SubscriptionName")]
		public string SubscriptionName { get; set; }

		[JsonProperty("interestEarn")]
		public string InterestEarn { get; set; }
	}
}