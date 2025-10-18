using AuthApiBackend.Configurations;
using AuthApiBackend.Interfaces.IServices.ISendNotification;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using RazorLight;

namespace AuthApiBackend.Services
{
    public class Notification : INotification
    {

        private readonly RazorLightEngine _engine;
        private readonly EmailConfig emailConfig;

        public Notification(IOptions<EmailConfig> options)
        {
            _engine = new RazorLightEngineBuilder().UseFileSystemProject(Path.Combine(Directory.GetCurrentDirectory(), "Templates")).
                      UseMemoryCachingProvider().Build();

            emailConfig = options.Value;
        }

        public async Task SendNotification(DTOs.TemplatesDto.NotificationDto notification)
        {

            var retrieveResult = _engine.Handler.Cache.RetrieveTemplate(notification.TemplateName);

            if (!retrieveResult.Success)
            {
                
                await _engine.CompileTemplateAsync(notification.TemplateName);
                retrieveResult = _engine.Handler.Cache.RetrieveTemplate(notification.TemplateName);

                if (!retrieveResult.Success)
                    throw new InvalidOperationException($"Failed to retrieve or compile template '{notification.TemplateName}'");
            }

            var fetchTemplate = await _engine.RenderTemplateAsync(
                retrieveResult.Template.TemplatePageFactory(),
                notification
            );

            await SendEmail(notification.ToEmail, notification.Subject, fetchTemplate);
        }


        public async Task SendEmail(string toEmail, string subject, string message)
        {

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("AuthApi", Environment.GetEnvironmentVariable("FROM_EMAIL")));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = message
            };

            email.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(emailConfig.Host, int.Parse(emailConfig.Port), MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(Environment.GetEnvironmentVariable("FROM_EMAIL"), Environment.GetEnvironmentVariable("EMAIL_PASSWORD"));
            await client.SendAsync(email);
            await client.DisconnectAsync(true);

        }

    }

}
