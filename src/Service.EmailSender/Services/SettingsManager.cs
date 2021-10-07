using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Service.EmailSender.Domain.Models;
using Service.EmailSender.Domain.SettingModels;
using Service.EmailSender.Grpc.Models;

namespace Service.EmailSender.Services
{
    public class SettingsManager
    {
        private readonly ILogger<SettingsManager> _logger;

        public SettingsManager(ILogger<SettingsManager> logger)
        {
            _logger = logger;
        }

        public static EmailSenderGrpcResponseContract EmailError(string message) =>
          new EmailSenderGrpcResponseContract
          {
              Message = message,
              Result = false
          };

        public static EmailSenderGrpcResponseContract EmailSentSuccessResponse(BaseEmailSettings baseSettings, IEmailGrpcRequestContract requestContract) =>
            new EmailSenderGrpcResponseContract { Result = true, Message = $"{GetEmailSettingsKey(requestContract)}|{baseSettings.SendGridTemplateId}" };

        public OperationResult<T> GetSettings<T>(Dictionary<string, T> dictionary, IEmailGrpcRequestContract requestContract)
        {
            var key = GetEmailSettingsKey(requestContract);

            if (dictionary.TryGetValue(key, out T emailSettings))
            {
                return new OperationResult<T>(emailSettings);
            }
            else
            {
                var settingsWithDefaultLang = new EmailTemplateSettings(requestContract) { 
                    Lang = Program.Settings.DefaultTemplateLanguage
                };

                key = GetEmailSettingsKey(settingsWithDefaultLang);

                _logger.LogWarning("Unable to find Email template for language: {lang}. Using {defaultLang}", requestContract.Lang, settingsWithDefaultLang.Lang);

                if (dictionary.TryGetValue(key, out emailSettings))
                {
                    return new OperationResult<T>(emailSettings);
                }
                else
                {
                    return new OperationResult<T>
                    {
                        ErrorMessage = $"Unable to get settings by key: {key}"
                    };
                }
            }  
        }

        public static string GetEmailSettingsKey(IEmailGrpcRequestContract r)
        {
            const char delimeter = '|';

            return $"{r.Brand}{delimeter}{r.Platform}{delimeter}{r.Lang}".ToLower();
        }
    }
}
