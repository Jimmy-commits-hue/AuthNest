
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Models;

namespace AuthApiBackend.BackgroundTask
{
    /*
    public class CleanExpiredCodes : BackgroundService
    {

        private readonly IServiceProvider serviceProvider;
        private readonly TimeSpan timespan = TimeSpan.FromHours(24);

        public CleanExpiredCodes(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                using(var scope = serviceProvider.CreateAsyncScope())
                {
                    var codeService = scope.ServiceProvider.GetRequiredService<IVerificationCodeService>();

                    var tempPasswordService = scope.ServiceProvider.GetRequiredService<ITemporaryPasswordService>();


                    IEnumerable<VerificationCode>? expiredVerificationCodes = await codeService.ExpiredVerifcationCodes(stoppingToken);

                    IEnumerable<TemporaryPassword>? expiredTemporaryCodes = await tempPasswordService.RetrieveExpiredCodes(stoppingToken);


                    if(expiredVerificationCodes is not null)
                    {
                       await codeService.RemoveExpiredCodes(expiredVerificationCodes, stoppingToken);
                    }

                    if(expiredTemporaryCodes is not null)
                    {
                        await tempPasswordService.RemoveCodes(expiredTemporaryCodes, stoppingToken);
                    }
                   
                }

                await Task.Delay(timespan, stoppingToken);
            }
        }

    }*/

}