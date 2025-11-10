
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Models;

namespace AuthApiBackend.BackgroundTask
{
    
    public class CleanExpiredCodes : BackgroundService
    {

        private readonly IServiceProvider serviceProvider;
        private readonly TimeSpan timespan = TimeSpan.FromHours(24);
        private readonly ILogger<CleanExpiredCodes> logger;

        public CleanExpiredCodes(IServiceProvider serviceProvider, ILogger<CleanExpiredCodes> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                using (var scope = serviceProvider.CreateAsyncScope())
                {
                    var codeService = scope.ServiceProvider.GetRequiredService<IVerificationCodeService>();

                    var tempPasswordService = scope.ServiceProvider.GetRequiredService<ITemporaryPasswordService>();


                    IEnumerable<VerificationCode>? expiredVerificationCodes = await codeService.ExpiredVerificationCodes(stoppingToken);

                    IEnumerable<TemporaryPassword>? expiredTemporaryCodes = await tempPasswordService.RetrieveExpiredCodes(stoppingToken);


                    if(expiredVerificationCodes is not null)
                    {
                        foreach(var code in expiredVerificationCodes)
                        {
                            await codeService.RemoveCodes(code, stoppingToken);
                        }
                    }

                    if(expiredTemporaryCodes is not null)
                    {
                        foreach (var code in expiredTemporaryCodes)
                        {
                            await tempPasswordService.RemoveCodes(code, stoppingToken);
                        }
                    }

                }

                await Task.Delay(timespan, stoppingToken);
            }
        }

    }

}