using Autofac;
using Service.EmailSender.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.EmailSender.Client
{
    public static class AutofacHelper
    {
        public static void RegisterEmailSenderClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new EmailSenderClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetEmailSender()).As<IEmailSenderService>().SingleInstance();
        }
    }
}
