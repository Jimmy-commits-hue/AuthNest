using AuthApiBackend.DTOs.ResponseDtos;
using Microsoft.AspNetCore.Diagnostics;

namespace AuthApiBackend.Exceptions
{

    public class UnknownExceptionHandler : IExceptionHandler
    {

        private readonly ILogger<UnknownExceptionHandler> log;

        public UnknownExceptionHandler(ILogger<UnknownExceptionHandler> log)
        {

            this.log = log;

        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {

            var response = new ExceptionResponse
            {

                ErrorMessage = exception.Message,
                ErrorCode = StatusCodes.Status500InternalServerError

            };

            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            log.LogError("Unknown Exception was thrown {Exception}", exception);

            return true;

        }

    }

}
