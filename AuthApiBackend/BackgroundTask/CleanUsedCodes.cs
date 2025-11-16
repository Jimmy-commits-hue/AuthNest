using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Models;

namespace AuthApiBackend.BackgroundTask
{
    
    public class CleanUsedCodes : BackgroundService
    {

        private readonly IServiceProvider serviceProvider;
        private readonly TimeSpan timer = TimeSpan.FromMinutes(1);
        private ILogger<CleanUsedCodes> logger;
        public CleanUsedCodes(IServiceProvider serviceProvider, ILogger<CleanUsedCodes> logger)
        {
           this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var codeService = scope.ServiceProvider.GetRequiredService<IVerificationCodeService>();

                    int NumberOfUsedVerificationCodes = await codeService.NumberOfUsedCodes(stoppingToken);

                    int totalRoundsForUsedVerificationCodes = NumberOfUsedVerificationCodes / 10;

                    var tempPasswordService = scope.ServiceProvider.GetRequiredService<ITemporaryPasswordService>();
                    
                    int numberOfUsedTempCodes = await tempPasswordService.NumberOfUsedCodes(stoppingToken);

                    int totalRoundsForUsedTempCodes = numberOfUsedTempCodes / 10;

                    int maxRounds = Math.Max(totalRoundsForUsedTempCodes, totalRoundsForUsedVerificationCodes);
                    int round = 0;

                    while (maxRounds >= round)
                    {

                        IEnumerable<VerificationCode>? usedCode = await codeService.RetrieveUsedCodes(round, stoppingToken);

                        IEnumerable<TemporaryPassword>? usedTempCodes = await tempPasswordService.RetrieveUsedCodes(round, stoppingToken);

                        if (usedCode is not null)
                        {
                            foreach (var code in usedCode)
                            {
                                await codeService.RemoveCodes(code, stoppingToken);
                            }

                        }

                        if (usedTempCodes is not null)
                        {
                            foreach (var tempCode in usedTempCodes)
                            {
                                await tempPasswordService.RemoveCodes(tempCode, stoppingToken);
                            }
                        }

                        round++;
                    }
                }

                await Task.Delay(timer, stoppingToken);
            }

        }

    }

}
