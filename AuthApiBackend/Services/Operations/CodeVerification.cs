using AuthApiBackend.DTOs;
using AuthApiBackend.Interfaces.IOperations;
using AuthApiBackend.Interfaces.IServices;
using Serilog.Context;

namespace AuthApiBackend.Services.Operations
{
    public class CodeVerification : ICodeVerification
    {

        private readonly IVerificationCodeService codeService;
        private readonly IContactDetailsService contactService;
        private readonly IAccountService accountService;
        private readonly ILogger<CodeVerification> logger;

        public CodeVerification(IVerificationCodeService codeService, IContactDetailsService contactService, 
            IAccountService accountService, ILogger<CodeVerification> logger)
        {
            this.codeService = codeService;
            this.contactService = contactService;
            this.accountService = accountService;
            this.logger = logger;
        }

        public async Task VerifyCode(CodeVerificationDto code, CancellationToken cancellationToken)
        {
            using (LogContext.PushProperty("Operation", "CodeVerification"))
            {

                string userId = await codeService.VerifyCodeAsync(code.CodeId, code.Code, cancellationToken);

                await contactService.UpdateIsEmailVerified(userId, cancellationToken);

                await codeService.UpdateCodeAsync(code.CodeId, cancellationToken);

                await accountService.UpdateAccountNumber(userId, cancellationToken);

                logger.LogInformation("Code for {UserId} was verifyed successfully", userId);

            }
        }

    }

}
