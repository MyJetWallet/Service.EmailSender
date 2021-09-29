namespace Service.EmailSender.Domain.Models
{
    public class EmailModel
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string SendGridTemplateId { get; set; }
        public object Data { get; set; }
        public string Brand { get; set; }
    }
}
