using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.DTOs.TemplatesDto;
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Interfaces.IServices.ISendNotification;
using Serilog.Context;

namespace AuthApiBackend.BackgroundTask
{

    public class SendAccountNumber : BackgroundService
    {

        private readonly IServiceProvider serviceProvider;
        private readonly TimeSpan timer = TimeSpan.FromMinutes(1);
        private readonly ILogger<SendAccountNumber> logger;

        public SendAccountNumber(IServiceProvider serviceProvider, ILogger<SendAccountNumber> logger)
        {

            this.serviceProvider = serviceProvider;
            this.logger = logger;

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            using (LogContext.PushProperty("JobName", nameof(SendAccountNumber)))
            {

                while (!stoppingToken.IsCancellationRequested)
                {

                    using var Scope = serviceProvider.CreateAsyncScope();

                    var accountService = Scope.ServiceProvider.GetRequiredService<IAccountService>();
                    var emailService = Scope.ServiceProvider.GetRequiredService<INotification>();

                    IEnumerable<PendingAccountNumbers>? accountNumbers = await accountService.GetPendingAccounts(stoppingToken);

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
                                        ToEmail = account.Email!,
                                        TemplateName = "AccountNumber.cshtml",

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

                    Scope.Dispose();

                    await Task.Delay(timer, stoppingToken);

                }

            }

        }

    }

}