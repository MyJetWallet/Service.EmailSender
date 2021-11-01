using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using Service.DynamicLinkGenerator.Domain.Models.Enums;
using Service.DynamicLinkGenerator.Grpc;
using Service.DynamicLinkGenerator.Grpc.Models;
using Service.EmailSender.Domain.Models;
using Service.EmailSender.Domain.Models.DataModels;
using Service.EmailSender.Grpc;
using Service.EmailSender.Grpc.Models;

namespace Service.EmailSender.Services
{
    public class EmailSenderService: IEmailSenderService
    {
        private readonly SendGridEmailSender _emailSender;
        private readonly ILinkGenerator _linkGenerator;
        private readonly SettingsManager _settingsManager;
        private readonly ILogger<EmailSenderService> _logger;

        public EmailSenderService(SendGridEmailSender emailSender, 
            ILinkGenerator linkGenerator, 
            SettingsManager settingsManager, 
            ILogger<EmailSenderService> logger)
        {
            _emailSender = emailSender;
            _linkGenerator = linkGenerator;
            _settingsManager = settingsManager;
            _logger = logger;
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
                _logger.LogError("Unable to send RegistrationConfirmEmail to userId {userId}, email {maskedEmail}. Error message: {errorMessage}", requestContract.TraderId, requestContract.Email.Mask(), settingsResult.ErrorMessage);
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
                _logger.LogError("Unable to send RegistrationConfirmEmail to userId {userId}, email {maskedEmail}. Error message: {errorMessage}", requestContract.TraderId, requestContract.Email.Mask(), sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent RegistrationConfirmEmail to {maskedEmail}", requestContract.Email.Mask());
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
                _logger.LogWarning("Unable to send VerificationCodeEmail to userId {userId}, email {maskedEmail}. Error message: {errorMessage}", requestContract.TraderId, requestContract.Email.Mask(), settingsResult.ErrorMessage);
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
                _logger.LogError("Unable to send VerificationCodeEmail to userId {userId}, email {maskedEmail}. Error message: {errorMessage}", requestContract.TraderId, requestContract.Email, sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent VerificationCodeEmail to {maskedEmail}", requestContract.Email.Mask());
            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);
        }

        public async ValueTask<EmailSenderGrpcResponseContract> SendRecoveryEmailAsync(RecoveryEmailGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotRecoveryEmailSettings, requestContract);

            if (settingsResult.Error)
            {
                _logger.LogError("Unable to send RecoveryEmail to userId {userId}, email {maskedEmail}. Error message: {errorMessage}", requestContract.TraderId, requestContract.Email.Mask(), settingsResult.ErrorMessage);
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }

            LinkResponse response;
            try
            {
                response = _linkGenerator.GenerateDeepLink(new GenerateLinkRequest()
                {
                    Brand = requestContract.Brand,
                    DeviceType = Enum.Parse<DeviceTypeEnum>(requestContract.DeviceType, true), 
                    Parameters = new (){{"jw_command","ForgotPassword"},{"jw_token", requestContract.Token}}
                });
            }
            catch (Exception e)
            {
                _logger.LogError("Unable to send RecoveryEmail to userId {userId}, email {maskedEmail}. Error message: {errorMessage}", requestContract.TraderId, requestContract.Email.Mask(), e.Message);

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
                _logger.LogError("Unable to send RecoveryEmail to userId {userId}, email {maskedEmail}. Error message: {errorMessage}", requestContract.TraderId, requestContract.Email.Mask(), sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent RecoveryEmail to {maskedEmail}", requestContract.Email.Mask());
            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);
        }

        public async ValueTask<EmailSenderGrpcResponseContract> SendAlreadyRegisteredEmailAsync(AlreadyRegisteredEmailGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotAlreadyRegisteredEmailSettings, requestContract);

            if (settingsResult.Error)
            {
                _logger.LogError("Unable to send AlreadyRegisteredEmail to userId {userId}, email {maskedEmail}. Error message: {errorMessage}", requestContract.TraderId, requestContract.Email.Mask(), settingsResult.ErrorMessage);
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
                _logger.LogWarning("Unable to send AlreadyRegisteredEmail to userId {userId}, email {maskedEmail}. Error message: {errorMessage}", requestContract.TraderId, requestContract.Email.Mask(), e.Message);
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
                _logger.LogError("Unable to send AlreadyRegisteredEmail to userId {userId}, email {maskedEmail}. Error message: {errorMessage}", requestContract.TraderId, requestContract.Email.Mask(), sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent AlreadyRegisteredEmail to {maskedEmail}", requestContract.Email.Mask());
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
                _logger.LogError("Unable to send WithdrawalVerificationEmail to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), settingsResult.ErrorMessage);
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
                _logger.LogError("Unable to send WithdrawalVerificationEmail to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent WithdrawalVerificationEmail to {maskedEmail}", requestContract.Email.Mask());
            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);
        }

        public async ValueTask<EmailSenderGrpcResponseContract> SendTransferByPhoneVerificationEmailAsync(TransferByPhoneVerificationGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotTransferByPhoneVerificationEmailSettings, requestContract);

            if (settingsResult.Error)
            {                
                _logger.LogError("Unable to send TransferByPhoneVerificationEmail to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), settingsResult.ErrorMessage);
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
                _logger.LogError("Unable to send TransferByPhoneVerificationEmail to  email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent TransferByPhoneVerificationEmail to {maskedEmail}", requestContract.Email.Mask());
            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);
        }

        public async ValueTask<EmailSenderGrpcResponseContract> SendLoginEmailAsync(LoginEmailGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotLoginEmailSettings, requestContract);

            if (settingsResult.Error)
            {               
                _logger.LogError("Unable to send LoginEmail to  email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), settingsResult.ErrorMessage);
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
                _logger.LogError("Unable to send LoginEmail to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent LoginEmail to {maskedEmail}", requestContract.Email.Mask());
            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);        
        }

        
    }
    
    public static class EmailMaskedHelper
    {
        public static string Mask(this string email)
        {
            var sha = email.EncodeToSha1();
            var mask = Convert.ToBase64String(sha);
            return mask;
        }
    }
}