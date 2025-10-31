using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.DTOs.TemplatesDto;
using AuthApiBackend.Interfaces.IServices.ISendNotification;
using AuthApiBackend.Utilities;

namespace AuthApiBackend.BackgroundTask
{

    public class SendForgettenLoginNumber : BackgroundService
    {

        private readonly TimeSpan timeSpan = TimeSpan.FromMinutes(1);
        private readonly IServiceProvider serviceProvider;

        public SendForgettenLoginNumber(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {

                using (var scope = serviceProvider.CreateScope())
                {

                    var emailService = scope.ServiceProvider.GetRequiredService<INotification>();

                    while (!SendStudentNumberToClient.resendForgettedLoginNumber.IsEmpty)
                    {

                        ForgottenLoginNumber userData;

                        if (SendStudentNumberToClient.resendForgettedLoginNumber.TryDequeue(out userData!))
                        {

                            var notification = new NotificationDto
                            {
                                AccountNumber = userData.LoginNumber,
                                ToEmail = userData.UserEmail,
                                TemplateName = "RetrieveLoginNumber.cshtml",
                                Subject = "Forgotten Login number"
                            };

                            await emailService.SendNotification(notification);

                        }

                    }

                }

                await Task.Delay(timeSpan, stoppingToken);

            }

        }

    }

}