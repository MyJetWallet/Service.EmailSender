﻿using System.Runtime.Serialization;
using Destructurama.Attributed;
using Service.EmailSender.Domain.Models;

namespace Service.EmailSender.Grpc.Models
{
    [DataContract]
    public class AlreadyRegisteredEmailGrpcRequestContract: IEmailGrpcRequestContract
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
        public string TraderId { get; set; }

        [DataMember(Order = 6)]
        public string Ip { get; set; }
        
        [DataMember(Order = 7)]
        public string UserAgent { get; set; }
    }
}
