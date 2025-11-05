using AuthApiBackend.DTOs;
using AuthApiBackend.Interfaces.IOperations;
using AuthApiBackend.Interfaces.IServices;
using Org.BouncyCastle.Crypto.Prng;
using Serilog.Context;

namespace AuthApiBackend.Services.Operations
{
    public class CancelDeletion : ICancelDeletion
    {

        private readonly IAccountService accountService;
        private readonly ILogger<CancelDeletion> logger;
        
        public CancelDeletion(IAccountService accountService,ILogger<CancelDeletion> logger)
        {
           this.accountService = accountService;
           this.logger = logger;  
        }

        public async Task CancelAccountDeletion(LoginDto login, CancellationToken cancellationToken)
        {
            using(LogContext.PushProperty("Operation", nameof(accountService)))
            {
                var accountId = await accountService.GetAccountId(login.LoginNumber, cancellationToken);
                var hashedPassword = await accountService.VerifyLoginNumber(login.LoginNumber, cancellationToken);

                await accountService.CancelAccountDeletion(accountId, hashedPassword, login.Password, cancellationToken);

                logger.LogInformation("{AccountId} was retrieved at {DateTime}", accountId, DateTime.UtcNow.ToLocalTime());
            }
        }
    }
}
