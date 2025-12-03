using AspNetCoreGeneratedDocument;
using AuthApiBackend.Configurations;
using AuthApiBackend.Interfaces.IServices.ISendNotification;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Org.BouncyCastle.Crypto.Prng;
using RazorLight;

namespace AuthApiBackend.Services
{
    public class Notification : INotification
    {
        private readonly RazorLightEngine _engine;
        private readonly EmailConfig emailConfig;


        public Notification(IOptions<EmailConfig> options)
        {
            _engine = new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(typeof(Program)) // looks inside this assembly
                .EnableDebugMode()
                .UseMemoryCachingProvider()
                .Build();

            emailConfig = options.Value;
        }

        public async Task SendNotification(DTOs.TemplatesDto.NotificationDto notification)
        {

            var templateKey = GetEmbeddedTemplateKey(notification.TemplateName);

            var findTemplate = _engine.Handler.Cache.RetrieveTemplate(templateKey);

            if (!findTemplate.Success)
            {
               await _engine.CompileTemplateAsync(templateKey);
                findTemplate = _engine.Handler.Cache.RetrieveTemplate(templateKey);
            }

            var fetchTemplate = await _engine.RenderTemplateAsync(findTemplate.Template.TemplatePageFactory(), notification);

            await SendEmail(notification.ToEmail, notification.Subject, fetchTemplate);
        }

        private string GetEmbeddedTemplateKey(string templateName)
        {
            var assembly = typeof(Program).Assembly;
            var resources = assembly.GetManifestResourceNames();

            var key = resources.FirstOrDefault(r => r.EndsWith(templateName, StringComparison.OrdinalIgnoreCase));

            if (key == null)
                throw new FileNotFoundException($"Embedded template '{templateName}' not found. Available templates: {string.Join(", ", resources)}");

            return key;
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
