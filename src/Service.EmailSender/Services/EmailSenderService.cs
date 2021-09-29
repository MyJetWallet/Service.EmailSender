using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MyJetWallet.Sdk.Service;
using Service.DynamicLinkGenerator.Domain.Models.Enums;
using Service.DynamicLinkGenerator.Grpc;
using Service.DynamicLinkGenerator.Grpc.Models;
using Service.EmailSender.Domain.Models;
using Service.EmailSender.Domain.Models.DataModels;
using Service.EmailSender.Grpc;
using Service.EmailSender.Grpc.Models;
using SimpleTrading.Emails.Abstractions.Emails;

namespace Service.EmailSender.Services
{
    public class EmailSenderService: IEmailSenderService
    {
        private readonly SendGridEmailSender _emailSender;
        private readonly ILinkGenerator _linkGenerator;
        private readonly SettingsManager _settingsManager;
        
        public EmailSenderService(SendGridEmailSender emailSender, 
            ILinkGenerator linkGenerator, 
            SettingsManager settingsManager)
        {
            _emailSender = emailSender;
            _linkGenerator = linkGenerator;
            _settingsManager = settingsManager;
        }

        public async ValueTask<EmailSenderGrpcResponseContract> SendRegistrationConfirmAsync(RegistrationConfirmGrpcRequestContract requestContract)
        {
            using var activity = MyTelemetry.StartActivity("Send Registration Confirm Email");

            if (Regex.IsMatch(requestContract.Email, Program.Settings.IgnoreEmailsDomains))
            {
                return SettingsManager.EmailError($"Email in ignored list: {requestContract.Email}");
            }

            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotConfirmEmailSettings, requestContract);

            if (settingsResult.Error)
            {
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }

            var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = settingsResult.Value.SendGridTemplateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new { }
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }

            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);
        }

        public async ValueTask<EmailSenderGrpcResponseContract> SendVerificationCodeEmail(ConfirmEmailGrpcRequestContract requestContract)
        {
            if (Regex.IsMatch(requestContract.Email, Program.Settings.IgnoreEmailsDomains))
            {
                return SettingsManager.EmailError($"Email in ignored list: {requestContract.Email}");
            }

            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotVerifyByEmailSettings, requestContract);

            if (settingsResult.Error)
            {
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }

            var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = settingsResult.Value.SendGridTemplateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new
                {
                    code = requestContract.Code,
                    link = requestContract.Link
                }
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }

            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);
        }

        public async ValueTask<EmailSenderGrpcResponseContract> SendRecoveryEmailAsync(RecoveryEmailGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotRecoveryEmailSettings, requestContract);

            if (settingsResult.Error)
            {
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }

            var tokenHexResult = _settingsManager.GenerateEmailTokenHex(EmailTypes.Recovery, settingsResult.Value.TokenExpires, requestContract.TraderId,
                requestContract.Platform, requestContract.Brand);

            if (tokenHexResult.Error)
            {
                return SettingsManager.EmailError(tokenHexResult.ErrorMessage);
            }

            LinkResponse response;
            try
            {
                response = _linkGenerator.GenerateDeepLink(new GenerateLinkRequest()
                {
                    Brand = requestContract.Brand,
                    DeviceType = Enum.Parse<DeviceTypeEnum>(requestContract.DeviceType, true), 
                    Parameters = new (){{"jw_command","ForgotPassword"},{"jw_token",tokenHexResult.Value}}
                });
            }
            catch (Exception e)
            {
                return SettingsManager.EmailError(e.Message);
            }

            var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = settingsResult.Value.SendGridTemplateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new RecoveryEmailDataModel
                {
                    Link = response.Link,
                    TraderName = requestContract.FullName
                }
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }

            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);
        }

        public async ValueTask<EmailSenderGrpcResponseContract> SendAlreadyRegisteredEmailAsync(AlreadyRegisteredEmailGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotAlreadyRegisteredEmailSettings, requestContract);

            if (settingsResult.Error)
            {
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }

            LinkResponse response;
            try
            {
                response = _linkGenerator.GenerateDeepLink(new GenerateLinkRequest()
                {
                    Brand = requestContract.Brand,
                    DeviceType = DeviceTypeEnum.Unknown,
                    Parameters = new (){{"jw_command","Login"}}
                });
            }
            catch (Exception e)
            {
                return SettingsManager.EmailError(e.Message);
            }

            var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = settingsResult.Value.SendGridTemplateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new AlreadyRegisteredEmailDataModel()
                {
                    Link = response.Link,
                }
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }

            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);
        }


        public async ValueTask<EmailSenderGrpcResponseContract> SendWithdrawalVerificationEmailAsync(
            WithdrawalVerificationGrpcRequestContract requestContract)
        {
            var settingsResult = requestContract.IsInternal
                ? _settingsManager.GetSettings(Program.Settings.SpotInternalWithdrawalVerificationEmailSettings,
                    requestContract)
                : _settingsManager.GetSettings(Program.Settings.SpotWithdrawalVerificationEmailSettings,
                    requestContract);


            if (settingsResult.Error)
            {
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }
            
            var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = settingsResult.Value.SendGridTemplateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new WithdrawalVerificationDataModel
                {
                    Link = requestContract.Link,
                    AssetSymbol = requestContract.AssetSymbol,
                    Amount = requestContract.Amount,
                    DestinationAddress = requestContract.DestinationAddress,
                    IpAddress = requestContract.IpAddress,
                    Code = requestContract.Code,
                    FeeAmount = requestContract.FeeAmount,
                    FeeAssetSymbol = requestContract.FeeAssetSymbol ?? requestContract.AssetSymbol
                }
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }

            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);
        }

        public async ValueTask<EmailSenderGrpcResponseContract> SendTransferByPhoneVerificationEmailAsync(TransferByPhoneVerificationGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotTransferByPhoneVerificationEmailSettings, requestContract);

            if (settingsResult.Error)
            {
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }
            
            var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = settingsResult.Value.SendGridTemplateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new TransferVerificationDataModel()
                {
                    Link = requestContract.Link,
                    AssetSymbol = requestContract.AssetSymbol,
                    Amount = requestContract.Amount,
                    DestinationPhone = requestContract.DestinationPhone,
                    IpAddress = requestContract.IpAddress,
                    Code = requestContract.Code
                }
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }

            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);
        }

        public async ValueTask<EmailSenderGrpcResponseContract> SendLoginEmailAsync(LoginEmailGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotLoginEmailSettings, requestContract);

            if (settingsResult.Error)
            {
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }
            
            var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = settingsResult.Value.SendGridTemplateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new LoginEmailDataModel
                {
                    Email = requestContract.Email,
                    IpAddress = requestContract.Ip,
                    LoginTime = requestContract.LoginTime
                }
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }

            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);        
        }
    }
}