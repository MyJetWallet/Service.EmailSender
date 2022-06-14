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
        ValueTask<EmailSenderGrpcResponseContract> SendLoginEmailAsync(LoginEmailGrpcRequestContract requestContract);
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendJobCvPositionSubmitEmailAsync(JobCvPositionSubmitGrpcRequestContract requestContract);
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendProfileDeleteConfirmEmail(ConfirmProfileDeleteGrpcRequestContract requestContract);
        //2fa
        ValueTask<EmailSenderGrpcResponseContract> Send2FaSettingsChangedEmailAsync(TwoFaSettingsChangedGrpcRequestContract requestContract);

        //Deposit
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendDepositSuccessfulEmailAsync(DepositSuccessfulGrpcRequestContract requestContract);
        
        //Withdrawal
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendWithdrawalVerificationEmailAsync(WithdrawalVerificationGrpcRequestContract requestContract);
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendWithdrawalSuccessfulEmailAsync(WithdrawalSuccessfulGrpcRequestContract requestContract);
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendWithdrawalCancelledEmailAsync(WithdrawalCancelledGrpcRequestContract requestContract);
        
        //Transfer
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendTransferByPhoneVerificationEmailAsync(TransferByPhoneVerificationGrpcRequestContract requestContract);
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendTransferReceivedEmailAsync(TransferReceivedGrpcRequestContract requestContract);
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendTransferSuccessfulEmailAsync(TransferSuccessfulGrpcRequestContract requestContract);
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendTransferCancelledEmailAsync(TransferCancelledGrpcRequestContract requestContract);

        //RecurringBuy
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendRecurringBuyFailedEmailAsync(RecurrentBuyFailedGrpcRequestContract requestContract);
        
        //Kyc
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendKycDocumentsApprovedEmailAsync(KycApprovedEmailGrpcRequestContract requestContract);
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendKycDocumentsDeclinedEmailAsync(KycDeclinedEmailGrpcRequestContract requestContract);
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendKycBannedEmailAsync(KycBannedEmailGrpcRequestContract requestContract);
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendSuspiciousActivityEmailAsync(SuspiciousActivityBannedEmailGrpcRequestContract requestContract);

        //SignInFailed
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendSignInFailed1HEmailAsync(SignInFailedGrpcRequestContract requestContract);
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendSignInFailed24HEmailAsync(SignInFailedGrpcRequestContract requestContract);
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendSignInFailed2Fa1HEmailAsync(SignInFailedGrpcRequestContract requestContract);
        [OperationContract]
        ValueTask<EmailSenderGrpcResponseContract> SendSignInFailed2Fa24HEmailAsync(SignInFailedGrpcRequestContract requestContract);

	    //HighYield
	    ValueTask<EmailSenderGrpcResponseContract> SendClientOfferTerminateEmailAsync(ClientOfferTerminateGrpcRequestContract requestContract);
    }
}