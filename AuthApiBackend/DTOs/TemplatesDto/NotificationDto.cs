namespace AuthApiBackend.DTOs.TemplatesDto
{

    public class NotificationDto
    {


        public string Name { get; set; } = string.Empty;

        public string Surname { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string ToEmail { get; set; } = string.Empty;

        public string VerificationLink { get; set; } = string.Empty;

        public string VerificationType { get; set; } = string.Empty;

        public string? AccountNumber { get; set; } = string.Empty;

        public string TemplateName { get; set; } = string.Empty;

    }

}
