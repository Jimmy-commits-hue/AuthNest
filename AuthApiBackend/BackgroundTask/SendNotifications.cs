using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Interfaces.IServices.ISendNotification;

namespace AuthApiBackend.BackgroundTask
{
    public class SendNotifications : BackgroundService
    {
        private readonly IServiceProvider scope;
        private readonly TimeSpan timeSpan = TimeSpan.FromMinutes(1);

        public SendNotifications(IServiceProvider scope)
        {
            this.scope = scope;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                using var service = scope.CreateScope();

                var codeService = service.ServiceProvider.GetRequiredService<IVerificationCodeService>();

                IEnumerable<PendingCode>? pendingCode = await codeService.GetPendingCodeAsync(stoppingToken);

                var emailService = service.ServiceProvider.GetRequiredService<INotification>();



                if (pendingCode is not null)
                {
                    foreach (var code in pendingCode)
                    {
                        var notification = new DTOs.TemplatesDto.NotificationDto
                        {
                            Name = pendingCode.First().FirstName,
                            ToEmail = pendingCode.First().Email,
                            Subject = "Your Verification Code",
                            Surname = pendingCode.First().Surname,
                            VerificationType = Enums.NotificationType.Verification.ToString(),
                            VerificationLink = string.Empty,
                            TemplateName = "VerificationEmail.cshtml",

                        };

                        await emailService.SendNotification(notification);

                        await codeService.UpdateEmailSentAsync(code.Id, stoppingToken);
                    }

                }

                service.Dispose();

                Console.WriteLine("Waiting for next iteration");

                await Task.Delay(timeSpan, stoppingToken);
            }

        }
    }
}