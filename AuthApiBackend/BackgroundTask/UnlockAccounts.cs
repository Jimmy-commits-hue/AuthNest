
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.DTOs.TemplatesDto;
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Interfaces.IServices.ISendNotification;

namespace AuthApiBackend.BackgroundTask
{

    public class UnlockAccounts : BackgroundService
    {

        private readonly TimeSpan timeSpan = TimeSpan.FromMinutes(1);
        private readonly IServiceProvider serviceProvider;

        public UnlockAccounts(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                using(var scope = serviceProvider.CreateAsyncScope())
                {
                    var accountService = scope.ServiceProvider.GetRequiredService<IAccountService>();
                    var emailService = scope.ServiceProvider.GetRequiredService<INotification>();

                    var numberOfLockedAccounts = await accountService.NumberOfLockedAccounts(stoppingToken);

                    var totalRounds = numberOfLockedAccounts / 10;

                    var round = 0;

                    while (totalRounds >= round)
                    {
                        IEnumerable<LockedAccounts>? accountsToUnlock = await accountService.GetAllLockedAccounts(round, stoppingToken);

                        if (accountsToUnlock is not null)
                        {
                            foreach (var account in accountsToUnlock)
                            {
                                var notification = new NotificationDto
                                {
                                    ToEmail = account.Email,
                                    Subject = "Account Unlocked",
                                    TemplateName = "UnlockAccount.cshtml"
                                };

                                await emailService.SendNotification(notification);
                                await accountService.UnlockAccount(account.accountId, stoppingToken);
                            }
                        }

                        round++;
                    }
                }

                await Task.Delay(timeSpan, stoppingToken);
            }
        }
    }
}
