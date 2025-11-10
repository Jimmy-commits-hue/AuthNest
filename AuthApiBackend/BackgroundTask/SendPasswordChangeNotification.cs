using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.DTOs.TemplatesDto;
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Interfaces.IServices.ISendNotification;
using AuthApiBackend.Utilities;

namespace AuthApiBackend.BackgroundTask
{
    
    public class SendPasswordChangeNotification : BackgroundService
    {

        private readonly TimeSpan time = TimeSpan.FromMinutes(1);
        private readonly IServiceProvider serviceProvider;

        public SendPasswordChangeNotification(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while(!stoppingToken.IsCancellationRequested)
            {

                using(var scope = serviceProvider.CreateScope())
                {

                    var tempCodeRepo = scope.ServiceProvider.GetRequiredService<ITemporaryPasswordService>();

                    IEnumerable<ResetPasswordResponse>? pendingPasswords = await tempCodeRepo.GetAllPendingPasswords(stoppingToken);
                    var emailService = scope.ServiceProvider.GetRequiredService<INotification>();

                    if (pendingPasswords is not null)
                    {

                        foreach(var pendingPassword in pendingPasswords)
                        {

                            var notification = new NotificationDto
                            {
                                TempPassword = EncryptData.Decrypt(pendingPassword.password),
                                Subject = "Password Reset",
                                VerificationLink = $"https://localhost:7123/api/v1/home/reset-verify?passwordId={pendingPassword.tempPasswordId}",
                                TemplateName = "PasswordReset.cshtml",
                                ToEmail = pendingPassword.email
                            };

                            await emailService.SendNotification(notification);

                            await tempCodeRepo.UpdatePasswordStatus(pendingPassword.tempPasswordId, stoppingToken);

                        }

                    }

                }

                await Task.Delay(time, stoppingToken);

            }

        }

    }

}
