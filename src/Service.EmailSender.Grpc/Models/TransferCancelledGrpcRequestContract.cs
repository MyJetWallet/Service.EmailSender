using System.Runtime.Serialization;
using Destructurama.Attributed;
using Service.EmailSender.Domain.Models;

namespace Service.EmailSender.Grpc.Models
{
    [DataContract]
    public class TransferCancelledGrpcRequestContract: IEmailGrpcRequestContract
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
        public string OperationId { get; set; }
        
        [DataMember(Order = 8)]
        public string Timestamp { get; set; }
    }
}
