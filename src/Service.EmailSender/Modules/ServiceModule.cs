using Autofac;
using MyJetWallet.Brands;
using MyJetWallet.DynamicLinkGenerator.Ioc;
using MyJetWallet.Sdk.NoSql;
using Service.EmailSender.Services;

namespace Service.EmailSender.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var noSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));

            builder.RegisterBrandReader(noSqlClient);
            builder.RegisterDynamicLinkClient(noSqlClient);
            builder.RegisterType<SendGridEmailSender>().AsSelf().SingleInstance();
            builder.RegisterType<SettingsManager>().AsSelf().SingleInstance();
        }
    }
}