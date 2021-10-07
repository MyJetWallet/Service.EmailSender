using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Brands;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using Serilog;
using Service.EmailSender.Domain.Models;

namespace Service.EmailSender.Services
{
    
    public class SendGridEmailSender
    {
        private readonly ILogger<SendGridEmailSender> _logger;
        private readonly IBrandReader _brandReader;
        private readonly SendGridClient _client;
        
        public SendGridEmailSender(
            ILogger<SendGridEmailSender> logger, 
            IBrandReader brandReader)
        {
            _client = new SendGridClient(Program.Settings.SendGridSettingsApiKey);
            _logger = logger;
            _brandReader = brandReader;
        }

        public async Task<bool> SendMailAsync(string to, string subject, string templateId, object templateData, string from, string header)
        {
            try
            {
                var msg = new SendGridMessage
                {
                    From = new EmailAddress(from, header),
                    Subject = subject,
                    TemplateId = templateId
                };
                
                msg.AddTo(to);

                msg.SetTemplateData(templateData);

                var response = await _client.SendEmailAsync(msg);

                if (response.StatusCode != HttpStatusCode.Accepted)
                {
                    _logger.LogError("Can't send message, response: {resp}", JsonConvert.SerializeObject(response));
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return false;
            }

            return true;
        }

        public async Task<OperationResult<bool>> SendMailAsync(EmailModel emailModel)
        {
            try
            {
                var from = GenerateFrom(emailModel.Brand);

                if (from.Error)
                {
                    return new OperationResult<bool>
                    {
                        ErrorMessage = from.ErrorMessage
                    };
                }

                var msg = new SendGridMessage
                {
                    From = new EmailAddress(from.Value, emailModel.Subject),
                    Subject = emailModel.Subject,
                    TemplateId = emailModel.SendGridTemplateId
                };

                msg.AddTo(emailModel.To);

                msg.SetTemplateData(emailModel.Data);

                var response = await _client.SendEmailAsync(msg);

                if (response.StatusCode != HttpStatusCode.Accepted)
                {
                    return new OperationResult<bool>
                    {
                        ErrorMessage = "SendGrid returned: " + JsonConvert.SerializeObject(response)
                    };
                }
            }
            catch (Exception exception)
            {
                return new OperationResult<bool>
                {
                    ErrorMessage = exception.ToString()
                };
            }

            return new OperationResult<bool>(true);
        }

        public OperationResult<string> GenerateFrom(string brand)
        {
            var brandFromNoSQL = _brandReader.GetById(brand);

            if (brandFromNoSQL == null)
            {
                return new OperationResult<string>
                {
                    ErrorMessage = $"Brand not found {brand}"
                };
            }

            var from = $"noreply@{brandFromNoSQL.DomainsPool.FirstOrDefault()}";

            return new OperationResult<string>(from);
        }
    }
}