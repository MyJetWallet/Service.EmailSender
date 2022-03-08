using System.ServiceModel;
using System.Threading.Tasks;
using Service.EmailSender.Grpc.Models;

namespace Service.EmailSender.Grpc
{
    [ServiceContract]
    public interface IEmailSenderService
    {
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendRegistrationConfirmAsync(RegistrationConfirmGrpcRequestContract requestContract);

        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendVerificationCodeEmail(ConfirmEmailGrpcRequestContract requestContract);
        
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendRecoveryEmailAsync(RecoveryEmailGrpcRequestContract requestContract);
        
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendAlreadyRegisteredEmailAsync(AlreadyRegisteredEmailGrpcRequestContract requestContract);
        
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendWithdrawalVerificationEmailAsync(WithdrawalVerificationGrpcRequestContract requestContract);
        
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendTransferByPhoneVerificationEmailAsync(TransferByPhoneVerificationGrpcRequestContract requestContract);
        
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendLoginEmailAsync(LoginEmailGrpcRequestContract requestContract);
        
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendDepositSuccessfulEmailAsync(DepositSuccessfulGrpcRequestContract requestContract);
        
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendWithdrawalSuccessfulEmailAsync(WithdrawalSuccessfulGrpcRequestContract requestContract);
        
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendWithdrawalCancelledEmailAsync(WithdrawalCancelledGrpcRequestContract requestContract);
        
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendKycDocumentsApprovedEmailAsync(KycApprovedEmailGrpcRequestContract requestContract);
        
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendKycDocumentsDeclinedEmailAsync(KycDeclinedEmailGrpcRequestContract requestContract);
        
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendKycBannedEmailAsync(KycBannedEmailGrpcRequestContract requestContract);
    }
}