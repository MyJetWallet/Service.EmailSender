namespace Service.EmailSender.Domain.Models
{
    public class OperationResult<T>
    {
        public OperationResult()
        {
                
        }

        public OperationResult(T data)
        {
            Value = data;
        }

        public bool Error => ErrorMessage != null;
        public string ErrorMessage { get; set; }
        public T Value { get; set; }
    }
}
