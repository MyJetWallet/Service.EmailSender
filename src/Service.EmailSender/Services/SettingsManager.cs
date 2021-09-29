using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Service.EmailSender.Domain.Models;
using Service.EmailSender.Domain.SettingModels;
using Service.EmailSender.Grpc.Models;
using SimpleTrading.Common.Abstractions.Brand;
using SimpleTrading.Common.Abstractions.Platform;
using SimpleTrading.Common.Abstractions.Platforms;
using SimpleTrading.Emails.Abstractions.Emails;
using SimpleTrading.TokensManager;
using SimpleTrading.TokensManager.Tokens;

namespace Service.EmailSender.Services
{
    public class SettingsManager
    {
        private readonly IBrandReader _brandReader;
        private readonly IPlatformReader _platformReader;
        private readonly ILogger<SettingsManager> _logger;

        public SettingsManager(
            IBrandReader brandReader, 
            IPlatformReader platformReader,
            ILogger<SettingsManager> logger
            )
        {
            _brandReader = brandReader;
            _platformReader = platformReader;
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

        public OperationResult<string> GenerateEmailTokenHex(EmailTypes type, string tokenExpires, string traderId, string platform, string brand)
        {
            var platformNoSQL = _platformReader.Get(brand, PlatformTypes.St);

            if (platformNoSQL == null)
            {
                return new OperationResult<string>
                {
                    ErrorMessage = $"Platform not found for BrandId:{brand}, Type:{PlatformTypes.St}"
                };
            }

            var expires = TimeSpan.Parse(tokenExpires);

            if(!Enum.TryParse(typeof(PlatformTypes), platform, out object platformType))
            {
                return new OperationResult<string>
                {
                    ErrorMessage = $"Unable to parse Platform {platform}"
                };
            }

            var emailSenderToken = new EmailToken
            {
                Url = platformNoSQL.BasePlatformUrl,
                Expires = DateTime.UtcNow.Add(expires),
                Id = traderId,
                Type = type,
                Platform = (PlatformTypes)platformType
            };

            var tokenHex = emailSenderToken.IssueTokenAsHexString(Program.EmailKey);

            return new OperationResult<string>(tokenHex);
        }
    }
}
