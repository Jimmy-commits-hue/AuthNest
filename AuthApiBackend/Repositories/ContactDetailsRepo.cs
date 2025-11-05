using AuthApiBackend.Database;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthApiBackend.Repositories
{

    public class ContactDetailsRepo(AuthApiDbContext context) : IContactDetailsRepo
    {

        public async Task CreateAsync(ContactDetails contactDetails, CancellationToken cancellationToken)
        {
            context.ContactDetails.Add(contactDetails);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<ContactDetails?> GetAsync(string userId, CancellationToken cancellationToken)
        {
            return await context.ContactDetails.AsNoTracking().
                         FirstOrDefaultAsync( u => u.Id == userId, cancellationToken);
        }

        public async Task UpdateIsEmailVerified(string emailId, CancellationToken cancellationToken)
        {
            var updateIsEmailVerified = new ContactDetails { Id = emailId };

            context.ContactDetails.Attach(updateIsEmailVerified);

            updateIsEmailVerified.IsEmailVerified = true;

            context.Entry(updateIsEmailVerified).Property(c => c.IsEmailVerified).IsModified = true;

            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateEmail(string accoutId, string newEmail, CancellationToken cancellationToken)
        {
            var updateEmail = new ContactDetails { Id = accoutId };

            context.ContactDetails.Attach(updateEmail);

            updateEmail.Email = newEmail;

            context.Entry(updateEmail).Property(c => c.Email).IsModified = true;

            await context.SaveChangesAsync(cancellationToken);
        }

    }

}