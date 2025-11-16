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

        public async Task<bool> AccountExists(string accountId, CancellationToken cancellationToken)
        {
            return await db.Account.AsNoTracking().
                         Where(c => c.Id == accountId).
                         AnyAsync(cancellationToken);
        }

        public async Task UpdateAccountAsync(string userId, string accountNumber, CancellationToken cancellationToken)
        {
            var updateAccount = new Account{ Id = userId };

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

            return await db.Account.AsNoTracking().
                         Where(a => a.AccountNumber.StartsWith(currentYearPrefix)).
                         OrderByDescending(a => a.AccountNumber).
                         Select(a => a.AccountNumber).
                         FirstOrDefaultAsync(cancellationToken);
        }

        public async Task UpdateIsEmailSentStatus(string accountId, CancellationToken cancellationToken)
        {
            var account = new Account { Id = accountId };

            db.Account.Attach(account);

            account.IsEmailSent = true;

            db.Entry(account).Property(c => c.IsEmailSent).IsModified = true;

            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> GetNumberOfPendingAccounts(CancellationToken cancellationToken)
        {
            return await db.Account.AsNoTracking().
                         Where(a => a.Status == Enums.AccountStatus.Active && a.IsEmailSent == false).
                         CountAsync(cancellationToken);
        }

        public async Task<IEnumerable<PendingAccountNumbers>?> GetPendingAccounts(int round, CancellationToken cancellationToken)
        {
            return await db.Account.AsNoTracking().
                         Where(a => a.Status == Enums.AccountStatus.Active && a.IsEmailSent == false).
                         OrderBy(a => a.Id).
                         Skip(round * 10).
                         Take(10).
                         Select(p => new PendingAccountNumbers(
                                     p.AccountNumber!,
                                     p.User.ContactDetails!.Email,
                                     p.Id)).
                         ToListAsync(cancellationToken);
        }

        public async Task<string?> GetUserPassword(string accountNumber, CancellationToken cancellationToken)
        {
            return await db.Account.AsNoTracking().
                         Where(account => account.AccountNumber == accountNumber).
                         Select(c => c.Password).
                         FirstOrDefaultAsync(cancellationToken);
        }

        public async Task UpdateFailedLoginAttempts(string accountId, int failedAttempt, CancellationToken cancellationToken)
        {
            var account = new Account { Id = accountId };

            db.Account.Attach(account);

            account.FailedLoginAttempts = failedAttempt;

            db.Entry(account).Property(c => c.FailedLoginAttempts).IsModified = true;

            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<OldPassword?> RetrieveOldPassword(string loginNumber, CancellationToken cancellationToken)
        {
            return await db.Account.AsNoTracking().
                         Where(account => account.AccountNumber == loginNumber).
                         Select(p => new OldPassword(
                                     p.Password,
                                     p.Status,
                                     p.Id,
                                     p.IsLocked)).
                         FirstOrDefaultAsync(cancellationToken);
        }

        public async Task UpdatePassword(string accountId, string NewPassword, CancellationToken cancellationToken)
        {
            var updatePassword = new Account { Id = accountId };

            db.Account.Attach(updatePassword);

            updatePassword.Password = NewPassword;

            db.Entry(updatePassword).Property(c => c.Password).IsModified = true;

            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<string?> GetAccountId(string loginNumber, CancellationToken cancellationToken)
        {
            return await db.Account.AsNoTracking().
                         Where(u => u.AccountNumber == loginNumber).
                         Select(u => u.Id).
                         FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<string?> GetOldPassword(string accountId, CancellationToken cancellationToken)
        {
            return await  db.Account.AsNoTracking().
                          Where(u => u.Id == accountId).
                          Select(u => u.Password).
                          FirstOrDefaultAsync(cancellationToken);
        }

        public async Task DeleteAccount(string accountId, CancellationToken cancellationToken)
        {
            var account = new Account { Id = accountId };

            db.Account.Attach(account);

            account.ExpectedDeleteDate = DateTimeOffset.UtcNow.AddDays(2).ToUnixTimeMilliseconds();
            account.Status = Enums.AccountStatus.Deleted;

            db.Entry(account).Property(u => u.ExpectedDeleteDate).IsModified = true;
            db.Entry(account).Property(u => u.Status).IsModified = true;

            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<VerifyLoginResponse> GetFailedAttemptCount(string loginNumber, CancellationToken cancellationToken)
        {
            return await db.Account.AsNoTracking().
                         Where(a => a.AccountNumber == loginNumber).
                         Select(f => new VerifyLoginResponse(
                                     f.FailedLoginAttempts,
                                     f.IsLocked,
                                     f.Status)).
                         FirstAsync(cancellationToken);
        }

        public async Task LockAccount(string accountId, CancellationToken cancellationToken)
        {
            var account = new Account { Id = accountId };

            db.Account.Attach(account);

            account.LockOutUntilDate = DateTimeOffset.UtcNow.AddMinutes(24).ToUnixTimeSeconds();

            db.Entry(account).Property(c => c.LockOutUntilDate).IsModified = true;

            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task DisableAccount(string accountId, CancellationToken cancellationToken)
        {
            var account = new Account { Id = accountId };

            db.Account.Attach(account);

            account.Status = Enums.AccountStatus.Disabled;

            db.Entry(account).Property(c => c.Status).IsTemporary = true;

            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task EnableAccount(string accountId, CancellationToken cancellationToken)
        {
            var account = new Account { Id = accountId };

            db.Account.Attach(account);

            account.Status = Enums.AccountStatus.Active;

            db.Entry(account).Property(c => c.Status).IsModified = true;

            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> GetNumberOfLockedAccounts(CancellationToken cancellationToken)
        {
            return await db.Account.AsNoTracking().
                         Where(c => c.LockOutUntilDate <= DateTimeOffset.UtcNow.ToUnixTimeSeconds()).
                         CountAsync(cancellationToken);
        }

        public async Task<IEnumerable<LockedAccounts>?> GetLockedAccounts(int round, CancellationToken cancellationToken)
        {
            return await db.Account.AsNoTracking().
                         Where(c => c.LockOutUntilDate <= DateTimeOffset.UtcNow.ToUnixTimeSeconds()).
                         OrderBy(u => u.Id).
                         Skip(round * 10).
                         Take(10).
                         Select(u => new LockedAccounts(
                                     u.Id,
                                     u.User.ContactDetails!.Email)).
                         ToListAsync(cancellationToken);
        }

        public async Task UnlockAccount(string accountId, CancellationToken cancellationToken)
        {
            var account = new Account { Id =  accountId };  

            db.Account.Attach(account);

            account.LockOutUntilDate = null;

            db.Entry(account).Property(c => c.LockOutUntilDate).IsModified = true;

            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateDeleteStatus(string accountId, CancellationToken cancellationToken)
        {
            var account = new Account { Id = accountId };

            db.Account.Attach(account);

            account.ExpectedDeleteDate = 0;

            db.Entry(account).Property(c => c.ExpectedDeleteDate).IsModified = true;

            await db.SaveChangesAsync(cancellationToken);

        }

        public async Task<int> GetNumberOfAccountsToDelete(CancellationToken cancellationToken)
        {
            long datetime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            return await db.Account.AsNoTracking().
                         Where(u => u.ExpectedDeleteDate < datetime && u.Status == Enums.AccountStatus.Deleted).
                         CountAsync(cancellationToken);
        }
        public async Task<IEnumerable<User>?> GetAllDeletedAccounts(int round, CancellationToken cancellationToken)
        {
            long datetime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            return await db.Account.AsNoTracking().
                         Where(u => u.ExpectedDeleteDate < datetime && u.Status == Enums.AccountStatus.Deleted).
                         OrderBy(u => u.Id).
                         Skip(round * 10).
                         Take(10).
                         Select(u => u.User).
                         ToListAsync(cancellationToken);
        }

        public async Task<AccountResponse> GetAccountDetailsUponLogin(string accountdId, CancellationToken cancellationToken)
        {
            return await db.Account.AsNoTracking().
                         Where(c => c.Id == accountdId).
                         Select(c => new AccountResponse(
                                     c.User.FirstName,
                                     c.User.Surname,
                                     c.User.UserRole.Role.RoleName
                               )).FirstAsync(cancellationToken);
        }

    }

}