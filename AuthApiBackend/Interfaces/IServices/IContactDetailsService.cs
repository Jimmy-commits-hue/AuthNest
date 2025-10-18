using AuthApiBackend.Models;

namespace AuthApiBackend.Interfaces.IServices
{

    public interface IContactDetailsService
    {

        Task CreateUserContactDetails(string userId,string accountDetails, CancellationToken cancellationToken);

        Task<ContactDetails> GetUserContactDetails(string email, CancellationToken cancellationToken);

        Task UpdateIsEmailVerified(string userId, CancellationToken cancellationToken);
    }

}
