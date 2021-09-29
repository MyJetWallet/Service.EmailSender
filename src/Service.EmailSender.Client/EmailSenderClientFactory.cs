using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;
using Service.EmailSender.Grpc;

namespace Service.EmailSender.Client
{
    [UsedImplicitly]
    public class EmailSenderClientFactory: MyGrpcClientFactory
    {
        public EmailSenderClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        public IEmailSenderService GetEmailSender() => CreateGrpcService<IEmailSenderService>();
    }
}
