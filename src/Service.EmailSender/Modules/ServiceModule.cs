using Autofac;
using MyJetWallet.Brands;
using MyJetWallet.Sdk.NoSql;
using Service.DynamicLinkGenerator.Client;
using Service.EmailSender.Services;

namespace Service.EmailSender.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var noSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));


            builder.RegisterBrandReader(noSqlClient);

            builder.RegisterDynamicLinkGeneratorClient(Program.Settings.DynamicLinkGrpcServiceUrl);

            builder.RegisterType<SendGridEmailSender>().AsSelf().SingleInstance();
            builder.RegisterType<SettingsManager>().AsSelf().SingleInstance();
        }
    }
}