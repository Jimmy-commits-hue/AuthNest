using Moq;
using AuthApiBackend.Services;
using AuthApiBackend.Models;
using AuthApiBackend.Utilities;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Exceptions.ExceptionTypes;
using Microsoft.Extensions.Logging;

namespace AuthApiBackendTest
{
    public class AccountServiceTests
    {

        private readonly Mock<IAccountRepository> accountRepo;
        private readonly AccountService accountService;
        private readonly Mock<ILogger<AccountService>> logger;

        public AccountServiceTests()
        {
            accountRepo = new Mock<IAccountRepository>();
            logger = new Mock<ILogger<AccountService>>();
            accountService = new AccountService(accountRepo.Object, logger.Object);
        }

        [Fact]
        public async Task CreateAccountAsync_ShouldCreateAccount()
        {
            string userId = Guid.NewGuid().ToString();
            string password = "#JaBu@5.";

            string passwordHash = HashHelper.Hash(password);
            
            var account = new Account
            {
                UserId = userId,
                Password = passwordHash,
            };

            accountRepo.Setup(repo => repo.GetUserIdAsync(userId, CancellationToken.None))
                       .ReturnsAsync((string?)null);

            accountRepo.Setup(accountRepo => accountRepo.CreateAsync(account, CancellationToken.None))
                       .Returns(Task.CompletedTask);

            await accountService.CreateAccountAsync(userId, password, CancellationToken.None);

            accountRepo.Verify(accountRepo => accountRepo.GetUserIdAsync(userId, CancellationToken.None), Times.Once);
            accountRepo.Verify(accountRepo => accountRepo.CreateAsync(It.IsAny<Account>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task CreateAccountAsync_ShouldThrowAccountAlreadyExistsException_IfAccountExists()
        {
            string userId = Guid.NewGuid().ToString();
            string password = "#JaBu@5.";

            accountRepo.Setup(repo => repo.GetUserIdAsync(userId, CancellationToken.None))
                       .ReturnsAsync(userId);

            await Assert.ThrowsAsync<AccountAlreadyExistException>(() => 
                accountService.CreateAccountAsync(userId, password, CancellationToken.None));

            accountRepo.Verify(accountRepo => accountRepo.GetUserIdAsync(userId, CancellationToken.None), Times.Once);
            accountRepo.Verify(accountRepo => accountRepo.CreateAsync(It.IsAny<Account>(), CancellationToken.None), Times.Never);
        }

        [Fact]
        public async Task UpdateAccountNumber_UpdateAccountNumber_IfAccountExist()
        {
            string userId = Guid.NewGuid().ToString();
            string lastAccountNumber = "250000001";
            String newAccountNumber = "250000002";

            accountRepo.Setup(repo => repo.GetUserIdAsync(userId, CancellationToken.None))
                       .ReturnsAsync(userId);

            accountRepo.Setup(repo => repo.GetLastAccountNumberAsync(CancellationToken.None))
                       .ReturnsAsync(lastAccountNumber);

            accountRepo.Setup(repo => repo.UpdateAccountAsync(userId,GenerateCode.GenerateAccountNumber(lastAccountNumber),
                       CancellationToken.None)).Returns(Task.CompletedTask);

            await accountService.UpdateAccountNumber(userId, CancellationToken.None);

            Assert.Equal(newAccountNumber, GenerateCode.GenerateAccountNumber(lastAccountNumber));

            accountRepo.Verify(repo => repo.GetUserIdAsync(userId, CancellationToken.None), Times.Once);
            accountRepo.Verify(repo => repo.UpdateAccountAsync(It.IsAny<string>(), newAccountNumber, CancellationToken.None), Times.Once);
            accountRepo.Verify(repo => repo.GetLastAccountNumberAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task GetPendingAccounts_ReturnsPendingAccounts_IfAny()
        {

            accountRepo.Setup(accountRepo => accountRepo.GetPendingAccounts(CancellationToken.None))
                       .ReturnsAsync(new List<PendingAccountNumbers>
                       {
                           new PendingAccountNumbers { AccountId = Guid.NewGuid().ToString(), AccountNumber = "250000001", 
                               Email = "jimmy@gmail.com" },
                           new PendingAccountNumbers { AccountId = Guid.NewGuid().ToString(), AccountNumber = "250000002", 
                               Email = "jimmy@gmail.com" }
                       });

            var result = await accountService.GetPendingAccounts(CancellationToken.None);

            Assert.IsType<List<PendingAccountNumbers>>(result);

            accountRepo.Verify(accountRepo => accountRepo.GetPendingAccounts(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task GetPendingEmails_ReturnsNull_IfNone()
        {

            accountRepo.Setup(accountRepo => accountRepo.GetPendingAccounts(CancellationToken.None))
                       .ReturnsAsync((IEnumerable<PendingAccountNumbers>?)null);

            var result = await accountService.GetPendingAccounts(CancellationToken.None);

            Assert.Null(result);

            accountRepo.Verify(accountRepo => accountRepo.GetPendingAccounts(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task UpdateIsEmailSent_ShouldUpdateEmailSentStatus()
        {

            string accountId = Guid.NewGuid().ToString();

            accountRepo.Setup(accountRepo => accountRepo.UpdateIsEmailSentStatus(accountId, CancellationToken.None))
                       .Returns(Task.CompletedTask);

            await accountService.UpdateIsEmailSent(accountId, CancellationToken.None);

            accountRepo.Verify(accountRepo => accountRepo.UpdateIsEmailSentStatus(accountId, CancellationToken.None), Times.Once);
        }

    }
}
