namespace Service.EmailSender.Domain.Models
{
    public interface IEmailGrpcRequestContract
    {
        public string Brand { get; set; }

        public string Platform { get; set; }

        public string Lang { get; set; }

        public string Email { get; set; }
    }
}
