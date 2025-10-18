using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Models;

namespace AuthApiBackend.Services
{
    public class ContactDetailsService(IContactDetailsRepo contactRepo) : IContactDetailsService
    {

        public async Task CreateUserContactDetails(string userId,string contactDetails, CancellationToken cancellationToken)
        {

            await contactRepo.CreateAsync(new ContactDetails
            {
                UserId = userId,
                Email = contactDetails,
                
            }, cancellationToken);

        }

        public async Task<ContactDetails> GetUserContactDetails(string userId, CancellationToken cancellationToken)
        {

            return await contactRepo.GetAsync(userId, cancellationToken) ??
                throw new Exception("Email does not exist");

        }

        public async Task UpdateIsEmailVerified(string userId, CancellationToken cancellationToken)
        {
            await contactRepo.UpdateIsEmailVerified(userId, cancellationToken);
        }

    }
}
