using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Exceptions.ExceptionTypes;
using Microsoft.AspNetCore.Diagnostics;
using Org.BouncyCastle.Pqc.Crypto.Lms;

namespace AuthApiBackend.Exceptions
{

    public class ExceptionsGlobalHandler : IExceptionHandler
    {

        private readonly ILogger<ExceptionsGlobalHandler> log;

        public ExceptionsGlobalHandler(ILogger<ExceptionsGlobalHandler> log)
        {

            this.log = log;

        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {

            var response = new ExceptionResponse
            {

               ErrorMessage = exception.Message,

            };


            switch (exception)
            {

                case AccountAlreadyExistException: response.ErrorCode = StatusCodes.Status409Conflict;
                    break;

                case CodeExpiredException: response.ErrorCode = StatusCodes.Status406NotAcceptable;
                    log.LogError("Code for user has expired");
                    break;

                case DailyMaximumAttemptsReachedException: response.ErrorCode = StatusCodes.Status429TooManyRequests; 
                    log.LogError("{UserId} Requested to many verification codes");
                    break;

                case EmailAlreadyVerifiedException: response.ErrorCode = StatusCodes.Status409Conflict;
                    log.LogWarning("Email already verified");
                    break;

                case NoAccountMatchException: response.ErrorCode = StatusCodes.Status404NotFound;
                    log.LogError("No Account Match");
                    break;

                case NoRoleMatchException: response.ErrorCode = StatusCodes.Status404NotFound;
                    log.LogError("No such role");
                    break;

                case RoleAlreadyExistException: response.ErrorCode = StatusCodes.Status409Conflict;
                    log.LogWarning("Role already exist");
                    break;

                case UserAlreadyExistException: response.ErrorCode = StatusCodes.Status409Conflict;
                    log.LogWarning("User already exist");
                    break;

                case UserNotFoundException: response.ErrorCode = StatusCodes.Status404NotFound;
                    log.LogError("User not found exception");
                    break;

                default: return false;

            }

            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true;

        }

    }

}
