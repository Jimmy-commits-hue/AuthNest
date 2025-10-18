using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.DTOs.TemplatesDto;
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Interfaces.IServices.ISendNotification;

namespace AuthApiBackend.BackgroundTask
{
    public class SendAccountNumber : BackgroundService
    {

        private readonly IServiceProvider serviceProvider;
        private readonly TimeSpan timer = TimeSpan.FromMinutes(1);

        public SendAccountNumber(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
              
                using var Scope = serviceProvider.CreateAsyncScope();

                var accountService = Scope.ServiceProvider.GetRequiredService<IAccountService>();
                var emailService = Scope.ServiceProvider.GetRequiredService<INotification>();

                IEnumerable<PendingAccountNumbers>? accountNumbers = await  accountService.GetPendingAccounts(stoppingToken);

                if(accountNumbers is not null)
                {
                    foreach (var account in accountNumbers)
                    {
                        var notification = new NotificationDto
                        {
                            AccountNumber = account.AccountNumber,
                            ToEmail = account.Email!,
                            TemplateName = "AccountNumber.cshtml",
                        };


                        await emailService.SendNotification(notification);
                        await accountService.UpdateIsEmailSent(account.AccountId, stoppingToken);
                    }
                }

                Scope.Dispose();

                await Task.Delay(timer, stoppingToken);
            }
        }
    }
}
