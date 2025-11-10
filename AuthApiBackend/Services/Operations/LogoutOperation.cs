using AuthApiBackend.Interfaces.IOperations;
using AuthApiBackend.Interfaces.IServices;
using Serilog.Context;
using System.IdentityModel.Tokens.Jwt;

namespace AuthApiBackend.Services.Operations
{
    public class LogoutOperation : ILogoutOperation
    {
        private readonly IHttpContextAccessor httpContext;
        private readonly IBlackListedTokenService blackListToken;
        private readonly ILogger<LogoutOperation> logger;

        public LogoutOperation(IHttpContextAccessor httpContext, IBlackListedTokenService blackListToken, ILogger<LogoutOperation> logger)
        {
            this.httpContext = httpContext;
            this.blackListToken = blackListToken;
            this.logger = logger;
        }

        public async Task Logout(CancellationToken cancellationToken)
        {
            using (LogContext.PushProperty("Operation", "Logout"))
            {

                var token = httpContext.HttpContext!.Request.Cookies["token"]!;
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var expiresAt = jwtToken.ValidTo;
                var expiresAtInUnix = new DateTimeOffset(expiresAt).ToUnixTimeSeconds();

                await blackListToken.AddBlackListedToken(token, expiresAtInUnix, cancellationToken);

                logger.LogInformation("User: {UserId} logged out successfully", 
                                      httpContext.HttpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value);
            }
        }
    }
}
