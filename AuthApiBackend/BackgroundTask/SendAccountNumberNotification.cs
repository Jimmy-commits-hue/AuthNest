using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.DTOs.TemplatesDto;
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Interfaces.IServices.ISendNotification;
using Serilog.Context;

namespace AuthApiBackend.BackgroundTask
{
    
    public class SendAccountNumberNotification : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly TimeSpan timer = TimeSpan.FromMinutes(1);
        private readonly ILogger<SendAccountNumberNotification> logger;

        public SendAccountNumberNotification(IServiceProvider serviceProvider, ILogger<SendAccountNumberNotification> logger)
        {

            this.serviceProvider = serviceProvider;
            this.logger = logger;

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            using (LogContext.PushProperty("JobName", nameof(SendAccountNumberNotification)))
            {

                while (!stoppingToken.IsCancellationRequested)
                {

                    using (var Scope = serviceProvider.CreateAsyncScope())
                    {

                        var accountService = Scope.ServiceProvider.GetRequiredService<IAccountService>();
                        var emailService = Scope.ServiceProvider.GetRequiredService<INotification>();

                        int numberOfPendingAccounts = await accountService.GetNumberOfPendingAccounts(stoppingToken);

                        int totalRounds = numberOfPendingAccounts / 10;

                        int round = 0;

                        while (totalRounds >= round)
                        {
                            IEnumerable<PendingAccountNumbers>? accountNumbers = await accountService.GetPendingAccounts(round, stoppingToken);

                            if (accountNumbers is not null)
                            {

                                foreach (var account in accountNumbers)
                                {

                                    using (LogContext.PushProperty("UserId", account.AccountId))
                                    {

                                        try
                                        {

                                            logger.LogInformation("Sending account number for {UserId} to {Email}", account.AccountId, account.Email);

                                            var notification = new NotificationDto
                                            {
                                                AccountNumber = account.AccountNumber,
                                                Subject = "Allocated Account Number",
                                                ToEmail = account.Email!,
                                                TemplateName = "AccountNumber.cshtml"
                                            };

                                            await emailService.SendNotification(notification);
                                            await accountService.UpdateIsEmailSent(account.AccountId, stoppingToken);

                                            logger.LogInformation("Account number for {UserId} was sent successfully", account.AccountId);

                                        }
                                        catch (Exception ex)
                                        {

                                            logger.LogError(ex, "An Error Occurred when trying to send an \"AccountNumber\" for {UserId} to {ToEmail}",
                                                account.AccountId, account.Email);

                                        }

                                    }

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

}