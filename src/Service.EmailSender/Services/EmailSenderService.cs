using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.DynamicLinkGenerator.Models;
using MyJetWallet.DynamicLinkGenerator.Services;
using MyJetWallet.Sdk.Service;
using Service.EmailSender.Domain.Models;
using Service.EmailSender.Domain.Models.DataModels;
using Service.EmailSender.Grpc;
using Service.EmailSender.Grpc.Models;

namespace Service.EmailSender.Services
{
    public class EmailSenderService: IEmailSenderService
    {
        private readonly SendGridEmailSender _emailSender;
        private readonly IDynamicLinkClient _linkGenerator;
        private readonly SettingsManager _settingsManager;
        private readonly ILogger<EmailSenderService> _logger;

        public EmailSenderService(SendGridEmailSender emailSender, 
            IDynamicLinkClient linkGenerator, 
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

            string link;
            try
            {
                link = _linkGenerator.GenerateForgotPasswordLink(new GenerateForgotPasswordLinkRequest()
                {
                    Brand = requestContract.Brand,
                    DeviceType = Enum.Parse<DeviceTypeEnum>(requestContract.DeviceType, true), 
                    Token = requestContract.Token
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
                    Link = link,
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

            string link;
            try
            {
                link = _linkGenerator.GenerateLoginLink(new GenerateLoginLinkRequest()
                {
                    Brand = requestContract.Brand,
                    DeviceType = DeviceTypeEnum.Unknown,
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
                    Link = link,
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

        public async ValueTask<EmailSenderGrpcResponseContract> SendDepositSuccessfulEmailAsync(DepositSuccessfulGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotDepositSuccessfulSettings, requestContract);

            if (settingsResult.Error)
            {                
                _logger.LogError("Unable to send SendDepositSuccessfulEmailAsync to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), settingsResult.ErrorMessage);
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }
            
            var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = settingsResult.Value.SendGridTemplateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new DepositSuccessfulDataModel()
                {
                    AssetSymbol = requestContract.AssetSymbol,
                    Amount = requestContract.Amount,
                }
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {                
                _logger.LogError("Unable to send SendDepositSuccessfulEmailAsync to  email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent SendDepositSuccessfulEmailAsync to {maskedEmail}", requestContract.Email.Mask());
            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);
        }

        public async ValueTask<EmailSenderGrpcResponseContract> SendWithdrawalSuccessfulEmailAsync(WithdrawalSuccessfulGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotWithdrawalSuccessfulSettings, requestContract);

            if (settingsResult.Error)
            {                
                _logger.LogError("Unable to send SendWithdrawalSuccessfulEmailAsync to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), settingsResult.ErrorMessage);
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }
            
            var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = settingsResult.Value.SendGridTemplateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new WithdrawalSuccessfulDataModel()
                {
                    AssetSymbol = requestContract.AssetSymbol,
                    Amount = requestContract.Amount,
                    FullName = requestContract.FullName
                }
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {                
                _logger.LogError("Unable to send SendWithdrawalSuccessfulEmailAsync to  email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent SendWithdrawalSuccessfulEmailAsync to {maskedEmail}", requestContract.Email.Mask());
            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);
        }
    }
    
    public static class EmailMaskedHelper
    {
        public static string Mask(this string email)
        {
            if (email.Length > 8)
            {
                var mask = $"{email.Substring(0, 3)}**{email.Substring(email.Length - 4, 3)}";
                return mask;
            }
            
            if (email.Length > 5)
            {
                var mask = $"{email[0]}**{email[^1]}";
                return mask;
            }
            
            return "*****";
        }
    }
}