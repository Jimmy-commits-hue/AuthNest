using AuthApiBackend.Interfaces.IOperations;
using AuthApiBackend.Interfaces.IServices;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Serilog.Context;

namespace AuthApiBackend.Services.Operations
{
    public class CodeResend : ICodeResend
    {

        private readonly IUserService userService;
        private readonly IVerificationCodeService codeService;
        private readonly ILogger<CodeResend> logger;

        public CodeResend(IUserService userService, IVerificationCodeService codeService, ILogger<CodeResend> logger)
        {
            this.userService = userService;
            this.codeService = codeService;
            this.logger = logger;
        }
        public async Task ResendCode(string idNumber, CancellationToken cancellationToken)
        {
            using (LogContext.PushProperty("Operation", "CodeRequest"))
            {

                var userAttemptsAndUserId = await userService.GetUserIdAsync(idNumber, cancellationToken);

                await codeService.RequestForCode(userAttemptsAndUserId, cancellationToken);

                logger.LogInformation("New verification code was sent for {UserId}", userAttemptsAndUserId.UserId);

            }

        }

    }

}