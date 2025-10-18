using AuthApiBackend.DTOs.TemplatesDto;

namespace AuthApiBackend.Interfaces.IServices.ISendNotification
{

    public interface INotification
    {

        Task SendNotification(NotificationDto message);

    }

}
