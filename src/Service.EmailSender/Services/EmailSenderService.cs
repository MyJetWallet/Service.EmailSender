using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.DynamicLinkGenerator.Models;
using MyJetWallet.DynamicLinkGenerator.Services;
using MyJetWallet.Sdk.Service;
using Service.EmailSender.Domain.Models;
using Service.EmailSender.Domain.Models.DataModels;
using Service.EmailSender.Domain.SettingModels;
using Service.EmailSender.Grpc;
using Service.EmailSender.Grpc.Models;
using Service.EmailSender.Helpers;

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

	        string templateId = settingsResult.Value.SendGridTemplateId;

	        var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = templateId,
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
            
            _logger.LogInformation("Sent RegistrationConfirmEmail to {maskedEmail}, templateId: {templateId}", requestContract.Email.Mask(), templateId);
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

	        string templateId = settingsResult.Value.SendGridTemplateId;

	        var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = templateId,
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
            
            _logger.LogInformation("Sent VerificationCodeEmail to {maskedEmail}, templateId: {templateId}", requestContract.Email.Mask(), templateId);
            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);
        }

	    public async ValueTask<EmailSenderGrpcResponseContract> SendSignInVerificationCodeEmail(SignInVerificationCodeEmailRequest requestContract)
	    {
            if (Regex.IsMatch(requestContract.Email, Program.Settings.IgnoreEmailsDomains))
	            return SettingsManager.EmailError($"Email in ignored list: {requestContract.Email}");

		    var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotSignInVerifyByEmailSettings, requestContract);

            if (settingsResult.Error)
            {
                _logger.LogWarning("Unable to send SignInVerificationCodeEmail to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), settingsResult.ErrorMessage);
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }

            string templateId = settingsResult.Value.SendGridTemplateId;

            var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = templateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new
                {
                    code = requestContract.Code,
                    link = requestContract.Link,
                    country = CultureInfoHelper.GetCountryName(requestContract.CountryCode),
                    ip = requestContract.IpAddress
                }
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {
                _logger.LogError("Unable to send SignInVerificationCodeEmail to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email, sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }

            _logger.LogInformation("Sent SignInVerificationCodeEmail to {maskedEmail}, templateId: {templateId}", requestContract.Email.Mask(), templateId);
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
                var links = _linkGenerator.GenerateForgotPasswordLink(new GenerateForgotPasswordLinkRequest()
                {
                    Brand = requestContract.Brand,
                    DeviceType = Enum.Parse<DeviceTypeEnum>(requestContract.DeviceType, true), 
                    Token = requestContract.Token,
                    Code = requestContract.Code
                });
                link = links.longLink;
            }
            catch (Exception e)
            {
                _logger.LogError("Unable to send RecoveryEmail to userId {userId}, email {maskedEmail}. Error message: {errorMessage}", requestContract.TraderId, requestContract.Email.Mask(), e.Message);

                return SettingsManager.EmailError(e.Message);
            }

	        string templateId = settingsResult.Value.SendGridTemplateId;

	        var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = templateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new RecoveryEmailDataModel
                {
                    Link = link,
                    TraderName = requestContract.FullName,
                    Code = requestContract.Code
                }
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {
                _logger.LogError("Unable to send RecoveryEmail to userId {userId}, email {maskedEmail}. Error message: {errorMessage}", requestContract.TraderId, requestContract.Email.Mask(), sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent RecoveryEmail to {maskedEmail}, templateId: {templateId}", requestContract.Email.Mask(), templateId);
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
                var links = _linkGenerator.GenerateLoginLink(new GenerateLoginLinkRequest()
                {
                    Brand = requestContract.Brand,
                    DeviceType = DeviceTypeEnum.Unknown,
                    Email = requestContract.Email
                });
                link = links.longLink;
            }
            catch (Exception e)
            {
                _logger.LogWarning("Unable to send AlreadyRegisteredEmail to userId {userId}, email {maskedEmail}. Error message: {errorMessage}", requestContract.TraderId, requestContract.Email.Mask(), e.Message);
                return SettingsManager.EmailError(e.Message);
            }

	        string templateId = settingsResult.Value.SendGridTemplateId;

	        var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = templateId,
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
            
            _logger.LogInformation("Sent AlreadyRegisteredEmail to {maskedEmail}, templateId: {templateId}", requestContract.Email.Mask(), templateId);
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
                    ReceiveAmount = requestContract.ReceiveAmount,
                    TimeTrans = requestContract.Timestamp,
                    PhoneModel = requestContract.PhoneModel,
                    Location = requestContract.Location,
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
                Data = new TransferVerificationDataModel
                {
                    Link = requestContract.Link,
                    AssetSymbol = requestContract.AssetSymbol,
                    Amount = requestContract.Amount,
                    DestinationPhone = requestContract.DestinationPhone,
                    IpAddress = requestContract.IpAddress,
                    TimeTrans = requestContract.Timestamp,
                    PhoneModel = requestContract.PhoneModel,
                    Location = requestContract.Location,
                    Code = requestContract.Code,
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

        public async ValueTask<EmailSenderGrpcResponseContract> SendTransferReceivedEmailAsync(TransferReceivedGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotTransferReceivedEmailSettings, requestContract);

            if (settingsResult.Error)
            {                
                _logger.LogError("Unable to send TransferReceivedEmail to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), settingsResult.ErrorMessage);
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }
            
            var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = settingsResult.Value.SendGridTemplateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new TransferReceivedDataModel
                {
                    AssetSymbol = requestContract.AssetSymbol,
                    Amount = requestContract.Amount,
                    TransId = requestContract.OperationId,
                    TimeTrans = requestContract.Timestamp,
                }
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {                
                _logger.LogError("Unable to send TransferReceivedEmail to  email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent TransferReceivedEmail to {maskedEmail}", requestContract.Email.Mask());
            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);        
        }

        public async ValueTask<EmailSenderGrpcResponseContract> SendTransferSuccessfulEmailAsync(TransferSuccessfulGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotTransferSuccessfulEmailSettings, requestContract);

            if (settingsResult.Error)
            {                
                _logger.LogError("Unable to send TransferSuccessfulEmail to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), settingsResult.ErrorMessage);
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }
            
            var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = settingsResult.Value.SendGridTemplateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new WithdrawalSuccessfulDataModel
                {
                    AssetSymbol = requestContract.AssetSymbol,
                    Amount = requestContract.Amount,
                    FullName = requestContract.FullName,
                    TransId = requestContract.OperationId,
                    TimeTrans = requestContract.Timestamp
                }
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {                
                _logger.LogError("Unable to send TransferSuccessfulEmail to  email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent TransferSuccessfulEmail to {maskedEmail}", requestContract.Email.Mask());
            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);
        }

        public async ValueTask<EmailSenderGrpcResponseContract> SendTransferCancelledEmailAsync(TransferCancelledGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotTransferCancelledEmailSettings, requestContract);

            if (settingsResult.Error)
            {                
                _logger.LogError("Unable to send TransferCancelledEmail to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), settingsResult.ErrorMessage);
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }
            
            var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = settingsResult.Value.SendGridTemplateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new WithdrawalCancelledDataModel()
                {
                    AssetSymbol = requestContract.AssetSymbol,
                    Amount = requestContract.Amount,
                    TransId = requestContract.OperationId,
                    TimeTrans = requestContract.Timestamp,
                }
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {                
                _logger.LogError("Unable to send TransferCancelledEmail to  email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent TransferCancelledEmail to {maskedEmail}", requestContract.Email.Mask());
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

	        string templateId = settingsResult.Value.SendGridTemplateId;

	        var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = templateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new LoginEmailDataModel
                {
                    Email = requestContract.Email,
                    IpAddress = requestContract.Ip,
                    LoginTime = requestContract.LoginTime,
                    PhoneModel = requestContract.PhoneModel,
                    Location = requestContract.Location,
                }
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {                
                _logger.LogError("Unable to send LoginEmail to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent LoginEmail to {maskedEmail}, templateId: {templateId}", requestContract.Email.Mask(), templateId);
            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);        
        }

        public async ValueTask<EmailSenderGrpcResponseContract> Send2FaSettingsChangedEmailAsync(TwoFaSettingsChangedGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.Spot2faSettingsChangedEmailSettings, requestContract);

            if (settingsResult.Error)
            {                
                _logger.LogError("Unable to send 2faSettingsChangedEmail to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), settingsResult.ErrorMessage);
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }
            
            var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = settingsResult.Value.SendGridTemplateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new ()
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {                
                _logger.LogError("Unable to send 2faSettingsChangedEmail to  email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent 2faSettingsChangedEmail to {maskedEmail}", requestContract.Email.Mask());
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
                    FullName = requestContract.FullName,
                    TransId = requestContract.OperationId,
                    TimeTrans = requestContract.Timestamp
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

        public async ValueTask<EmailSenderGrpcResponseContract> SendWithdrawalCancelledEmailAsync(WithdrawalCancelledGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotWithdrawalCancelledSettings, requestContract);

            if (settingsResult.Error)
            {                
                _logger.LogError("Unable to send WithdrawalCancelledEmail to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), settingsResult.ErrorMessage);
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }
            
            var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = settingsResult.Value.SendGridTemplateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new WithdrawalCancelledDataModel()
                {
                    AssetSymbol = requestContract.AssetSymbol,
                    Amount = requestContract.Amount,
                    TransId = requestContract.OperationId,
                    TimeTrans = requestContract.Timestamp,
                }
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {                
                _logger.LogError("Unable to send WithdrawalCancelledEmail to  email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent WithdrawalCancelledEmail to {maskedEmail}", requestContract.Email.Mask());
            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);
        }

        public async ValueTask<EmailSenderGrpcResponseContract> SendKycDocumentsApprovedEmailAsync(KycApprovedEmailGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotKycDocumentsApprovedSettings, requestContract);

            if (settingsResult.Error)
            {
                _logger.LogError("Unable to send KycDocumentsApprovedEmail to email {maskedEmail}. Error message: {errorMessage}",  requestContract.Email.Mask(), settingsResult.ErrorMessage);
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }

            string link;
            try
            {
                var links = _linkGenerator.GenerateKycSuccessLink(new ()
                {
                    Brand = requestContract.Brand,
                    DeviceType = DeviceTypeEnum.Unknown,
                });
                link = links.longLink;
            }
            catch (Exception e)
            {
                _logger.LogWarning("Unable to send KycDocumentsApprovedEmail to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), e.Message);
                return SettingsManager.EmailError(e.Message);
            }

            var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = settingsResult.Value.SendGridTemplateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new KycDocumentsApprovedDataModel()
                {
                    Link = link,
                }
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {                
                _logger.LogError("Unable to send KycDocumentsApprovedEmail to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent KycDocumentsApprovedEmail to {maskedEmail}", requestContract.Email.Mask());
            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);
        }

        public async ValueTask<EmailSenderGrpcResponseContract> SendKycDocumentsDeclinedEmailAsync(KycDeclinedEmailGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotKycDocumentsDeclinedSettings, requestContract);

            if (settingsResult.Error)
            {
                _logger.LogError("Unable to send KycDocumentsDeclined to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), settingsResult.ErrorMessage);
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }

            string link;
            try
            {
                var links = _linkGenerator.GenerateKycFailLink(new ()
                {
                    Brand = requestContract.Brand,
                    DeviceType = DeviceTypeEnum.Unknown,
                });
                link = links.longLink;
            }
            catch (Exception e)
            {
                _logger.LogWarning("Unable to send KycDocumentsDeclined to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), e.Message);
                return SettingsManager.EmailError(e.Message);
            }

            var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = settingsResult.Value.SendGridTemplateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new KycDocumentsDeclinedDataModel()
                {
                    Link = link,
                }
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {                
                _logger.LogError("Unable to send KycDocumentsDeclined to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent KycDocumentsDeclined to {maskedEmail}", requestContract.Email.Mask());
            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);
            
        }

        public async ValueTask<EmailSenderGrpcResponseContract> SendKycBannedEmailAsync(KycBannedEmailGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotKycBannedSettings, requestContract);

            if (settingsResult.Error)
            {                
                _logger.LogError("Unable to send KycBannedEmail to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), settingsResult.ErrorMessage);
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }
            
            var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = settingsResult.Value.SendGridTemplateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new KycBannedDataModel()
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {                
                _logger.LogError("Unable to send KycBannedEmail to  email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent KycBannedEmail to {maskedEmail}", requestContract.Email.Mask());
            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);
        }

        public async ValueTask<EmailSenderGrpcResponseContract> SendSignInFailed1HEmailAsync(SignInFailedGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotSignInFailed1hEmailSettings, requestContract);

            if (settingsResult.Error)
            {                
                _logger.LogError("Unable to send SignInFailed1hEmail to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), settingsResult.ErrorMessage);
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }
            
            var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = settingsResult.Value.SendGridTemplateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new ()
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {                
                _logger.LogError("Unable to send SignInFailed1hEmail to  email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent SignInFailed1hEmail to {maskedEmail}", requestContract.Email.Mask());
            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);
            
        }

        public async ValueTask<EmailSenderGrpcResponseContract> SendSignInFailed24HEmailAsync(SignInFailedGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotSignInFailed24hEmailSettings, requestContract);

            if (settingsResult.Error)
            {                
                _logger.LogError("Unable to send SignInFailed24hEmail to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), settingsResult.ErrorMessage);
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }
            
            var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = settingsResult.Value.SendGridTemplateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new ()
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {                
                _logger.LogError("Unable to send SignInFailed24hEmail to  email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent SignInFailed24hEmail to {maskedEmail}", requestContract.Email.Mask());
            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);
            
        }

        public async ValueTask<EmailSenderGrpcResponseContract> SendSignInFailed2Fa1HEmailAsync(SignInFailedGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotSignInFailed2fa1hEmailSettings, requestContract);

            if (settingsResult.Error)
            {                
                _logger.LogError("Unable to send SignInFailed2Fa1h to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), settingsResult.ErrorMessage);
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }
            
            var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = settingsResult.Value.SendGridTemplateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new ()
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {                
                _logger.LogError("Unable to send SignInFailed2Fa1h to  email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent SignInFailed2Fa1h to {maskedEmail}", requestContract.Email.Mask());
            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);
            
        }

        public async ValueTask<EmailSenderGrpcResponseContract> SendSignInFailed2Fa24HEmailAsync(SignInFailedGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotSignInFailed2fa24hEmailSettings, requestContract);

            if (settingsResult.Error)
            {                
                _logger.LogError("Unable to send SignInFailed2fa24hEmail to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), settingsResult.ErrorMessage);
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }
            
            var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = settingsResult.Value.SendGridTemplateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new ()
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {                
                _logger.LogError("Unable to send SignInFailed2fa24hEmail to  email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent SignInFailed2fa24hEmail to {maskedEmail}", requestContract.Email.Mask());
            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);
        }

        public async ValueTask<EmailSenderGrpcResponseContract> SendRecurringBuyFailedEmailAsync(RecurrentBuyFailedGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotAutoInvestFailSettings, requestContract);

            if (settingsResult.Error)
            {                
                _logger.LogError("Unable to send RecurringBuyFailed to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), settingsResult.ErrorMessage);
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }
            
            var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = settingsResult.Value.SendGridTemplateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new RecurrentBuyFailedDataModel
                {
                    ToAsset = requestContract.ToAsset,
                    FromAmount = requestContract.FromAmount,
                    FromAsset = requestContract.FromAsset,
                    FailTime = requestContract.FailTime,
                    FailureReason = requestContract.FailureReason,
                }
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {                
                _logger.LogError("Unable to send RecurringBuyFailed to  email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent RecurringBuyFailed to {maskedEmail}", requestContract.Email.Mask());
            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);
        }
        
        public async ValueTask<EmailSenderGrpcResponseContract> SendJobCvPositionSubmitEmailAsync(JobCvPositionSubmitGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotJobCvPositionSubmitEmailSettings, requestContract);
            string emailMasked = requestContract.Email.Mask();

	        if (settingsResult.Error)
            {
                _logger.LogError("Unable to send JobCvPositionSubmit to email {maskedEmail}. Error message: {errorMessage}", emailMasked, settingsResult.ErrorMessage);
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }

	        var emailSettings = settingsResult.Value;
	        string subject = emailSettings.Subject;

	        var templateData = new JobCvPositionSubmitDataModel
	        {
		        ApplicantName = requestContract.ApplicantName,
		        PositionTitle = requestContract.PositionTitle
	        };

	        bool sended = await _emailSender.SendMailAsync(requestContract.Email, subject, emailSettings.SendGridTemplateId, templateData, emailSettings.From, subject);
            if (!sended)
            {                
                _logger.LogError("Unable to send JobCvPositionSubmit to  email {maskedEmail}.", emailMasked);
                return SettingsManager.EmailError($"Unable to send JobCvPositionSubmit to  email {emailMasked}.");
            }
            
            _logger.LogInformation("Sent JobCvPositionSubmit to {maskedEmail}", emailMasked);
            return SettingsManager.EmailSentSuccessResponse(emailSettings, requestContract);
        }
        
        public async ValueTask<EmailSenderGrpcResponseContract> SendSuspiciousActivityEmailAsync(SuspiciousActivityBannedEmailGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotSuspiciousActivityEmailSettings, requestContract);

            if (settingsResult.Error)
            {                
                _logger.LogError("Unable to send SuspiciousActivityEmail to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), settingsResult.ErrorMessage);
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }
            
            var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = settingsResult.Value.SendGridTemplateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new ()
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {                
                _logger.LogError("Unable to send SuspiciousActivityEmail to  email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent SuspiciousActivityEmail to {maskedEmail}", requestContract.Email.Mask());
            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);
        }

        public async ValueTask<EmailSenderGrpcResponseContract> SendClientOfferTerminateEmailAsync(ClientOfferTerminateGrpcRequestContract requestContract)
        {
            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotClientOfferTerminateEmailSettings, requestContract);

            if (settingsResult.Error)
            {
                _logger.LogError("Unable to send SendClientOfferTerminateEmail to email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), settingsResult.ErrorMessage);
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }

            var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = settingsResult.Value.SendGridTemplateId,
                Subject = settingsResult.Value.Subject,
                Brand = requestContract.Brand,
                Data = new ClientOfferTerminateDataModel
                {
                    AssetSymbol = requestContract.AssetSymbol,
                    Amount = requestContract.Amount,
                    SubscriptionName = requestContract.SubscriptionName,
                    InterestEarn = requestContract.InterestEarn.ToString()
                }
            };

            var sendingResult = await _emailSender.SendMailAsync(emailModel);

            if (sendingResult.Error)
            {
                _logger.LogError("Unable to send SendClientOfferTerminateEmail to  email {maskedEmail}. Error message: {errorMessage}", requestContract.Email.Mask(), sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }

            _logger.LogInformation("Sent SendClientOfferTerminateEmail to {maskedEmail}", requestContract.Email.Mask());
            return SettingsManager.EmailSentSuccessResponse(settingsResult.Value, requestContract);
        }

        public async ValueTask<EmailSenderGrpcResponseContract> SendProfileDeleteConfirmEmail(ConfirmProfileDeleteGrpcRequestContract requestContract)
        {
            if (Regex.IsMatch(requestContract.Email, Program.Settings.IgnoreEmailsDomains))
            {
                return SettingsManager.EmailError($"Email in ignored list: {requestContract.Email}");
            }

            var settingsResult = _settingsManager.GetSettings(Program.Settings.SpotProfileDeleteConfirmEmailSettings, requestContract);

            if (settingsResult.Error)
            {               
                _logger.LogWarning("Unable to send ProfileDeleteConfirmEmail to userId {userId}, email {maskedEmail}. Error message: {errorMessage}", requestContract.TraderId, requestContract.Email.Mask(), settingsResult.ErrorMessage);
                return SettingsManager.EmailError(settingsResult.ErrorMessage);
            }

	        string templateId = settingsResult.Value.SendGridTemplateId;

	        var emailModel = new EmailModel
            {
                To = requestContract.Email,
                SendGridTemplateId = templateId,
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
                _logger.LogError("Unable to send ProfileDeleteConfirmEmail to userId {userId}, email {maskedEmail}. Error message: {errorMessage}", requestContract.TraderId, requestContract.Email, sendingResult.ErrorMessage);
                return SettingsManager.EmailError(sendingResult.ErrorMessage);
            }
            
            _logger.LogInformation("Sent ProfileDeleteConfirmEmail to {maskedEmail}, templateId: {templateId}", requestContract.Email.Mask(), templateId);
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