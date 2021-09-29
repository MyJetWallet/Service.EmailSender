﻿using System.Collections.Generic;
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
        
        [YamlProperty("EmailSender.DynamicLinkGrpcServiceUrl")]
        public string DynamicLinkGrpcServiceUrl { get; set; }
    }
}
