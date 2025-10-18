using AuthApiBackend.Database;
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthApiBackend.Repositories
{
    public class AccountRepository(AuthApiDbContext db) : IAccountRepository
    {

        public async Task CreateAsync(Account account, CancellationToken cancellationToken)
        {
            db.Account.Add(account);
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<string?> ExistsAsync(string userId, CancellationToken cancellationToken)
        {
            return await db.Account.Where(c => userId.CompareTo(c.UserId) == 0).AsNoTracking().Select(c => c.UserId).
                         FirstOrDefaultAsync(cancellationToken);
        }
        public async Task UpdateAccountAsync(string userId, string accountNumber, CancellationToken cancellationToken)
        {

            var updateAccount = new Account
            {
                UserId = userId,
            };

            db.Account.Attach(updateAccount);

            updateAccount.AccountNumber = accountNumber;
            updateAccount.Status = Enums.AccountStatus.Active;

            db.Entry(updateAccount).Property(c => c.AccountNumber).IsModified = true;
            db.Entry(updateAccount).Property(c => c.Status).IsModified = true;

            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<string?> GetLastAccountNumberAsync(CancellationToken cancellationToken)
        {
            string currentYearPrefix = DateTime.UtcNow.ToString("yy");

            return await db.Account.Where(a => a.AccountNumber.StartsWith(currentYearPrefix))
                .OrderByDescending(a => a.AccountNumber).Select(a => a.AccountNumber)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<PendingAccountNumbers>?> GetPendingAccounts(CancellationToken cancellationToken)
        {

            return await db.Account.AsNoTracking().Where(a => a.Status == Enums.AccountStatus.Active && a.IsEmailSent == false).
                            Select(p => new PendingAccountNumbers
                            {
                                AccountNumber = p.AccountNumber!,
                                Email = p.User.ContactDetails!.Email,
                                AccountId = p.UserId
                            }).ToListAsync(cancellationToken);

        }

        public async Task UpdateIsEmailSentStatus(string accountId, CancellationToken cancellationToken)
        {
            var account = new Account { UserId = accountId };

            db.Account.Attach(account);

            account.IsEmailSent = true;

            db.Entry(account).Property(c => c.IsEmailSent).IsModified = true;

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
