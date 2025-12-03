using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Interfaces.IServices.ISendNotification;
using AuthApiBackend.Utilities;
using Serilog.Context;

namespace AuthApiBackend.BackgroundTask
{
    
    public class SendVerificationCodeNotification : BackgroundService
    {

        private readonly IServiceProvider scope;
        private readonly TimeSpan timeSpan = TimeSpan.FromMinutes(1);
        private readonly ILogger<SendVerificationCodeNotification> logger;

        public SendVerificationCodeNotification(IServiceProvider scope, ILogger<SendVerificationCodeNotification> logger)
        {
            this.scope = scope;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            
            using (LogContext.PushProperty("JobName", nameof(SendVerificationCodeNotification)))
            {
                
                while (!stoppingToken.IsCancellationRequested)
                {

                    using (var service = scope.CreateScope())
                    {

                        var codeService = service.ServiceProvider.GetRequiredService<IVerificationCodeService>();

                        int verificationCodeCount = await codeService.NumberOfPendingCodes(stoppingToken);

                        int numberOfTurns = (int)verificationCodeCount / 10;
                        int round = 0;

                        while (numberOfTurns >= round && verificationCodeCount > 0)
                        {


                            IEnumerable<PendingCode>? pendingCode = await codeService.GetPendingCodeAsync(round,stoppingToken);

                            var emailService = service.ServiceProvider.GetRequiredService<INotification>();

                            if (pendingCode is not null)
                            {

                                foreach (var code in pendingCode)
                                {

                                    using (LogContext.PushProperty("UserId", code.Email))
                                    {

                                        try
                                        {

                                            logger.LogInformation("Sending verification code to {Email}", code.Email);

                                            var notification = new DTOs.TemplatesDto.NotificationDto
                                            {

                                                Name = pendingCode.First().FirstName,
                                                ToEmail = pendingCode.First().Email,
                                                Subject = "Your Verification Code",
                                                Surname = pendingCode.First().Surname,
                                                VerificationType = Enums.NotificationType.Verification.ToString(),
                                                VerificationLink = $"https://localhost:7123/api/v1/home/verify?CodeId={pendingCode.First().Id}",
                                                VerificationCode = EncryptData.Decrypt(pendingCode.First().Code),
                                                TemplateName = "VerificationEmail.cshtml",

                                            };

                                            try
                                            {
                                                await emailService.SendNotification(notification);
                                            }catch(Exception ex)
                                            {
                                                Console.WriteLine(ex.Message);
                                            }
                                                await codeService.UpdateEmailSentAsync(code.Id, stoppingToken);

                                            logger.LogInformation("Code was sent successfully to {Email}", code.Email);
                                            
                                        }
                                        catch (Exception ex)
                                        {
                                            logger.LogError(ex, "An error occurred when sending a code to {Email} {FirstName} {Surname}",
                                                code.Email, code.FirstName, code.Surname);

                                        }

                                    }

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

}