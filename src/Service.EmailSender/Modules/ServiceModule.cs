using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using DotNetCoreDecorators;
using MyJetWallet.Sdk.NoSql;
using Service.DynamicLinkGenerator.Client;
using Service.EmailSender.Services;
using SimpleTrading.Common.Abstractions.Brand;
using SimpleTrading.Common.Abstractions.Platform;
using SimpleTrading.Common.MyNoSql;

namespace Service.EmailSender.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var noSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));

            builder
                .RegisterInstance(noSqlClient.CreatePlatformsMyNoSqlReader())
                .As<IPlatformReader>()
                .SingleInstance();
            
            builder
                .RegisterInstance(noSqlClient.CreateBrandMyNoSqlReader())
                .As<IBrandReader>()
                .SingleInstance();
            
            builder.RegisterDynamicLinkGeneratorClient(Program.Settings.DynamicLinkGrpcServiceUrl);

            builder.RegisterType<SendGridEmailSender>().AsSelf().SingleInstance();
            builder.RegisterType<SettingsManager>().AsSelf().SingleInstance();
        }
    }
}