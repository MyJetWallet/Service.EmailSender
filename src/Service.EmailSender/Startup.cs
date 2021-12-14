using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Autofac;
using MyJetWallet.Sdk.GrpcSchema;
using MyJetWallet.Sdk.Service;
using Prometheus;
using Service.EmailSender.Grpc;
using Service.EmailSender.Modules;
using Service.EmailSender.Services;
using SimpleTrading.ServiceStatusReporterConnector;

namespace Service.EmailSender
{
    public class Startup
    {
        private const string EmailTokenKeyEnv = "EMAIL_TOKEN_KEY";

        public void ConfigureServices(IServiceCollection services)
        {
            services.BindCodeFirstGrpc();

            services.AddHostedService<ApplicationLifetimeManager>();

            services.AddMyTelemetry("SP-", Program.Settings.ZipkinUrl);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseMetricServer();

            app.BindServicesTree(Assembly.GetExecutingAssembly());

            app.BindIsAlive();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcSchema<EmailSenderService, IEmailSenderService>();

                endpoints.MapGrpcSchemaRegistry();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });

            GetEmailTokenKey();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule<SettingsModule>();
            builder.RegisterModule<ServiceModule>();
        }
        
        private static void GetEmailTokenKey()
        {
            var key = Environment.GetEnvironmentVariable(EmailTokenKeyEnv);
            if (string.IsNullOrEmpty(key))
                throw new Exception($"Env Variable {EmailTokenKeyEnv} is not found");

            Program.EmailKey = key.EncodeToSha1();
        }
        
    }
}
