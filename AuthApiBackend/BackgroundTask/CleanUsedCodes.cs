
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Models;

namespace AuthApiBackend.BackgroundTask
{
    /*
    public class CleanUsedCodes : BackgroundService
    {

        private readonly IServiceProvider serviceProvider;
        private readonly TimeSpan timer = TimeSpan.FromMinutes(1);

        public CleanUsedCodes(IServiceProvider serviceProvider)
        {
           this.serviceProvider = serviceProvider;    
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var codeService = scope.ServiceProvider.GetRequiredService<IVerificationCodeService>();

                    var tempPasswordService = scope.ServiceProvider.GetRequiredService<ITemporaryPasswordService>();

                    IEnumerable<VerificationCode>? usedCode = await codeService.RetrieveUsedCodes(stoppingToken);
                    IEnumerable<TemporaryPassword>? usedTempCodes = await tempPasswordService.RetrieveUsedCodes(stoppingToken);

                    if(usedCode is not null)
                    {
                        foreach (var code in usedCode)
                        {
                            await codeService.RemoveCodes(code, stoppingToken);
                        }
                        
                    }

                    if(usedTempCodes is not null)
                    {
                        foreach (var tempCode in usedTempCodes)
                        {
                            await tempPasswordService.RemoveCodes(tempCode, stoppingToken);
                        }
                    }
                }

                await Task.Delay(timer, stoppingToken);
            }

        }

    }*/

}
