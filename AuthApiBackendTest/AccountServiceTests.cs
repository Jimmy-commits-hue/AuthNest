using Moq;
using AuthApiBackend.Services;
using AuthApiBackend.Models;
using AuthApiBackend.Utilities;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Exceptions.ExceptionTypes;

namespace AuthApiBackendTest
{
    public class AccountServiceTests
    {

        private readonly Mock<IAccountRepository> accountRepo;
        private readonly AccountService accountService;

        public AccountServiceTests()
        {
            accountRepo = new Mock<IAccountRepository>();
            accountService = new AccountService(accountRepo.Object);
        }

        [Fact]
        public async Task CreateAccountAsync_ShouldCreateAccount()
        {
            string userId = Guid.NewGuid().ToString();
            string password = "#JaBu@5.";

            string passwordHash = HashHelper.HashPassword(password);
            
            var account = new Account
            {
                UserId = userId,
                Password = passwordHash,
            };

            accountRepo.Setup(repo => repo.ExistsAsync(userId, CancellationToken.None))
                       .ReturnsAsync((string?)null);

            accountRepo.Setup(accountRepo => accountRepo.CreateAsync(account, CancellationToken.None))
                       .Returns(Task.CompletedTask);

            await accountService.CreateAccountAsync(userId, password, CancellationToken.None);

            accountRepo.Verify(accountRepo => accountRepo.ExistsAsync(userId, CancellationToken.None), Times.Once);
            accountRepo.Verify(accountRepo => accountRepo.CreateAsync(It.IsAny<Account>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task CreateAccountAsync_ShouldThrowAccountAlreadyExistsException_IfAccountExists()
        {
            string userId = Guid.NewGuid().ToString();
            string password = "#JaBu@5.";

            accountRepo.Setup(repo => repo.ExistsAsync(userId, CancellationToken.None))
                       .ReturnsAsync(userId);

            await Assert.ThrowsAsync<AccountAlreadyExistException>(() => 
                accountService.CreateAccountAsync(userId, password, CancellationToken.None));

            accountRepo.Verify(accountRepo => accountRepo.ExistsAsync(userId, CancellationToken.None), Times.Once);
            accountRepo.Verify(accountRepo => accountRepo.CreateAsync(It.IsAny<Account>(), CancellationToken.None), Times.Never);
        }

        [Fact]
        public async Task UpdateAccountNumber_ShouldUpdateAccountNumber_IfAccountExist()
        {
            string userId = Guid.NewGuid().ToString();
            string lastAccountNumber = "250000001";
            String newAccountNumber = "250000002";

            accountRepo.Setup(repo => repo.ExistsAsync(userId, CancellationToken.None))
                       .ReturnsAsync(userId);

            accountRepo.Setup(repo => repo.GetLastAccountNumberAsync(CancellationToken.None))
                       .ReturnsAsync(lastAccountNumber);

            accountRepo.Setup(repo => repo.UpdateAccountAsync(userId,GenerateCode.GenerateAccountNumber(lastAccountNumber),
                       CancellationToken.None)).Returns(Task.CompletedTask);

            await accountService.UpdateAccountNumber(userId, CancellationToken.None);

            Assert.Equal(newAccountNumber, GenerateCode.GenerateAccountNumber(lastAccountNumber));

            accountRepo.Verify(repo => repo.ExistsAsync(userId, CancellationToken.None), Times.Once);
            accountRepo.Verify(repo => repo.UpdateAccountAsync(It.IsAny<string>(), newAccountNumber, CancellationToken.None), Times.Once);
            accountRepo.Verify(repo => repo.GetLastAccountNumberAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task UpdateAccountNumber_ShouldThrowException_IfAccountDoesNotExist()
        {
            string userId = Guid.NewGuid().ToString();

            accountRepo.Setup(repo => repo.ExistsAsync(userId, CancellationToken.None))
                       .ReturnsAsync((string?)null);

            var ex = await Assert.ThrowsAsync<NoAccountMatchException>(async () => await accountService.UpdateAccountNumber(userId, CancellationToken.None));

            Assert.Equal("Please register first", ex.Message);

            accountRepo.Verify(repo => repo.ExistsAsync(userId, CancellationToken.None), Times.Once);
            accountRepo.Verify(repo => repo.UpdateAccountAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None), Times.Never);
            accountRepo.Verify(repo => repo.GetLastAccountNumberAsync(CancellationToken.None), Times.Never);
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
