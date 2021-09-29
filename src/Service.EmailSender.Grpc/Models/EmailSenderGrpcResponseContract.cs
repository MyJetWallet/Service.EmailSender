using System.Runtime.Serialization;

namespace Service.EmailSender.Grpc.Models
{
    [DataContract]
    public class EmailSenderGrpcResponseContract
    {
        [DataMember(Order = 1)]
        public bool Result { get; set; }

        [DataMember(Order = 2)]
        public string Message { get; set; }
    }
}
