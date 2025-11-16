
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

                    var numberOfExpiredVerificationCodes = await codeService.NumberOfExpiredCodes(stoppingToken);

                    int tolalRoundsForVerificationCodes = numberOfExpiredVerificationCodes / 10;

                    var tempPasswordService = scope.ServiceProvider.GetRequiredService<ITemporaryPasswordService>();

                    int numberOfExpiredTempCodes = await tempPasswordService.NumberOfExpiredTempCodes(stoppingToken);

                    int totalRoundsForTempCodes = numberOfExpiredTempCodes / 10;

                    int roundsToMake = 0;

                    if(numberOfExpiredVerificationCodes > numberOfExpiredTempCodes) { 
                        roundsToMake = tolalRoundsForVerificationCodes;
                    }
                    else
                    {
                        roundsToMake = totalRoundsForTempCodes;
                    }

                    int round = 0;

                    logger.LogInformation("Starting cleanup of expired codes. Estimated rounds: {TotalRounds}", roundsToMake);

                    while (roundsToMake >= round)
                    { 
                        IEnumerable<VerificationCode>? expiredVerificationCodes = await codeService.ExpiredVerificationCodes(round, stoppingToken);

                        IEnumerable<TemporaryPassword>? expiredTemporaryCodes = await tempPasswordService.RetrieveExpiredCodes(round, stoppingToken);


                        if (expiredVerificationCodes is not null)
                        {
                            foreach (var code in expiredVerificationCodes)
                            {
                                await codeService.RemoveCodes(code, stoppingToken);
                            }
                        }

                        if (expiredTemporaryCodes is not null)
                        {
                            foreach (var code in expiredTemporaryCodes)
                            {
                                await tempPasswordService.RemoveCodes(code, stoppingToken);
                            }
                        }

                        round++;
                    }
                }

                await Task.Delay(timespan, stoppingToken);
            }
        }

    }

}