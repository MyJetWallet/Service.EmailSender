using System.Collections.Generic;
using MyJetWallet.Sdk.Service;
using MyYamlParser;
using Service.EmailSender.Domain.SettingModels;

namespace Service.EmailSender.Settings
{
    public class SettingsModel
    {
        [YamlProperty("EmailSender.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("EmailSender.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("EmailSender.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }
        
        [YamlProperty("EmailSender.SendGridSettingsApiKey")]
        public string SendGridSettingsApiKey { get; set; }

        [YamlProperty("EmailSender.IgnoreEmailsDomains")]
        public string IgnoreEmailsDomains { get; set; }
        
        [YamlProperty("EmailSender.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }

        [YamlProperty("EmailSender.DefaultTemplateLanguage")]
        public string DefaultTemplateLanguage { get; set; }
        
        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotConfirmEmailSettings")]
        public Dictionary<string, BaseEmailSettings> SpotConfirmEmailSettings { get; set; }

        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotVerifyByEmailSettings")]
        public Dictionary<string, BaseEmailSettings> SpotVerifyByEmailSettings { get; set; }
        
        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotRecoveryEmailSettings")]
        public Dictionary<string, SpotRecoveryEmailSettings> SpotRecoveryEmailSettings { get; set; }
        
        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotAlreadyRegisteredEmailSettings")]
        public Dictionary<string, BaseEmailSettings> SpotAlreadyRegisteredEmailSettings { get; set; }
        
        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotWithdrawalVerificationEmailSettings")]
        public Dictionary<string, BaseEmailSettings> SpotWithdrawalVerificationEmailSettings { get; set; }
        
        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotInternalWithdrawalVerificationEmailSettings")]
        public Dictionary<string, BaseEmailSettings> SpotInternalWithdrawalVerificationEmailSettings { get; set; }
        
        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotTransferByPhoneVerificationEmailSettings")]
        public Dictionary<string, BaseEmailSettings> SpotTransferByPhoneVerificationEmailSettings { get; set; }
        
        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotLoginEmailSettings")]
        public Dictionary<string, BaseEmailSettings> SpotLoginEmailSettings { get; set; }
        
        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotDepositSuccessfulSettings")]
        public Dictionary<string, BaseEmailSettings> SpotDepositSuccessfulSettings { get; set; } 
        
        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotWithdrawalSuccessfulSettings")] 
        public Dictionary<string, BaseEmailSettings> SpotWithdrawalSuccessfulSettings { get; set; }
        
        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotWithdrawalCancelledSettings")] 
        public Dictionary<string, BaseEmailSettings> SpotWithdrawalCancelledSettings { get; set; }
        
        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotKycDocumentsApprovedSettings")] 
        public Dictionary<string, BaseEmailSettings> SpotKycDocumentsApprovedSettings { get; set; }
        
        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotKycDocumentsDeclinedSettings")] 
        public Dictionary<string, BaseEmailSettings> SpotKycDocumentsDeclinedSettings { get; set; }
        
        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotKycBannedSettings")] 
        public Dictionary<string, BaseEmailSettings> SpotKycBannedSettings { get; set; }
        
        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotAutoInvestFailSettings")] 
        public Dictionary<string, BaseEmailSettings> SpotAutoInvestFailSettings { get; set; }
        
        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotJobCvPositionSubmitEmailSettings")]
        public Dictionary<string, JobCvPositionSubmitEmailSettings> SpotJobCvPositionSubmitEmailSettings { get; set; }
        
        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotTransferReceivedEmailSettings")]
        public Dictionary<string, BaseEmailSettings> SpotTransferReceivedEmailSettings { get; set; }
        
        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotTransferCancelledEmailSettings")]
        public Dictionary<string, BaseEmailSettings> SpotTransferCancelledEmailSettings { get; set; }
        
        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotTransferSuccessfulEmailSettings")]
        public Dictionary<string, BaseEmailSettings> SpotTransferSuccessfulEmailSettings { get; set; }
        
        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotSignInFailed1hEmailSettings")]
        public Dictionary<string, BaseEmailSettings> SpotSignInFailed1hEmailSettings { get; set; }
        
        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotSignInFailed24hEmailSettings")]
        public Dictionary<string, BaseEmailSettings> SpotSignInFailed24hEmailSettings { get; set; }
        
        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotSignInFailed2fa1hEmailSettings")]
        public Dictionary<string, BaseEmailSettings> SpotSignInFailed2fa1hEmailSettings { get; set; }
        
        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotSignInFailed2fa24hEmailSettings")]
        public Dictionary<string, BaseEmailSettings> SpotSignInFailed2fa24hEmailSettings { get; set; }
        
        [YamlProperty("EmailSender.EmailTemplatesSettings.Spot2faSettingsChangedEmailSettings")]
        public Dictionary<string, BaseEmailSettings> Spot2faSettingsChangedEmailSettings { get; set; }
 
        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotSuspiciousActivityEmailSettings")]
        public Dictionary<string, BaseEmailSettings> SpotSuspiciousActivityEmailSettings { get; set; }
 
        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotClientOfferTerminateEmailSettings")]
        public Dictionary<string, BaseEmailSettings> SpotClientOfferTerminateEmailSettings { get; set; }
        
        [YamlProperty("EmailSender.EmailTemplatesSettings.SpotProfileDeleteConfirmEmailSettings")]
        public Dictionary<string, BaseEmailSettings> SpotProfileDeleteConfirmEmailSettings { get; set; }

    }
}
