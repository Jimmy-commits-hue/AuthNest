using Moq;
using AuthApiBackend.Services;
using AuthApiBackend.Models;
using AuthApiBackend.Interfaces.IRepositories;

namespace AuthApiBackendTest
{

    public class ContactDetailsTest
    {

        private readonly Mock<IContactDetailsRepo> contactRepo;
        private readonly ContactDetailsService contactDetailsService;

        public ContactDetailsTest()
        {
            contactRepo = new Mock<IContactDetailsRepo>();
            contactDetailsService = new ContactDetailsService(contactRepo.Object);
        }

        [Fact]
        public async Task CreateUserContactDetails_ShouldCreateContactDetails()
        {

            string userId = Guid.NewGuid().ToString();
            string email = "jimmyjabulani01@gmail.com";

            var UserContactDetails = new ContactDetails
            {
                UserId = userId,
                Email = email,
            };

            contactRepo.Setup(contactRepo => contactRepo.CreateAsync(UserContactDetails, CancellationToken.None))
                       .Returns(Task.CompletedTask);

            await contactDetailsService.CreateUserContactDetails(userId, email, CancellationToken.None);

            contactRepo.Verify(contactRepo => contactRepo.CreateAsync(It.IsAny<ContactDetails>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task GetUserContactDetails_ShouldReturnContactDetails_IfExists()
        {
            string userId = Guid.NewGuid().ToString();
            string email = "jimmyjabulani01@gmail.com";

            var existingContactDetails = new ContactDetails
            {
                UserId = userId,
                Email = email,
            };

            contactRepo.Setup(contactRepo => contactRepo.GetAsync(userId, CancellationToken.None))
                       .ReturnsAsync(existingContactDetails);

            var result = await contactDetailsService.GetUserContactDetails(userId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.IsType<ContactDetails>(result);

            contactRepo.Verify(contactRepo => contactRepo.GetAsync(userId, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task UpdateIsEmailVerified_ShouldUpdateEmailVerificationStatus()
        {

            string userId = Guid.NewGuid().ToString();

            var existingContactDetails = new ContactDetails
            {
                UserId = userId,
                Email = "jimmyjabulani01@gmail.com",
                IsEmailVerified = false,
            };

            contactRepo.Setup(contactRepo => contactRepo.CreateAsync(existingContactDetails, CancellationToken.None))
                       .Returns(Task.CompletedTask);

            contactRepo.Setup(contactRepo => contactRepo.UpdateIsEmailVerified(userId, CancellationToken.None))
                       .Returns(Task.CompletedTask);

            await contactDetailsService.UpdateIsEmailVerified(userId, CancellationToken.None);

            contactRepo.Verify(contactRepo => contactRepo.UpdateIsEmailVerified(userId, CancellationToken.None), Times.Once);

        }
    }
}
