using System.Runtime.Serialization;
using Destructurama.Attributed;
using Service.EmailSender.Domain.Models;

namespace Service.EmailSender.Grpc.Models
{
    [DataContract]
    public class WithdrawalVerificationGrpcRequestContract : IEmailGrpcRequestContract
    {
        [DataMember(Order = 1)]
        public string Brand { get; set; }

        [DataMember(Order = 2)]
        public string Platform { get; set; }

        [DataMember(Order = 3)]
        public string Lang { get; set; }

        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        [DataMember(Order = 4)]
        public string Email { get; set; }

        [DataMember(Order = 5)] 
        public string AssetSymbol { get; set; }
        
        [DataMember(Order = 6)] 
        public string Amount { get; set; }
        
        [DataMember(Order = 7)] 
        public string DestinationAddress { get; set; }
        
        [DataMember(Order = 8)] 
        public string IpAddress { get; set; }
        
        [DataMember(Order = 9)] 
        public string Link { get; set; }
        
        [DataMember(Order = 10)]
        public string Code { get; set; }
        
        [DataMember(Order = 11)]
        public string FeeAssetSymbol { get; set; }
        
        [DataMember(Order = 12)]
        public string FeeAmount { get; set; }
        
        [DataMember(Order = 13)]
        public bool IsInternal { get; set; }

    }
}