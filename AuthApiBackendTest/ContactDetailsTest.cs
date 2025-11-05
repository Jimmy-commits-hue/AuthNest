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

            var fakeDb = new List<ContactDetails>();

            contactRepo.Setup(contactRepo => contactRepo.CreateAsync(It.IsAny<ContactDetails>(), It.IsAny<CancellationToken>())).
                        Callback<ContactDetails, CancellationToken>((newContact, cancellationToken) =>
                        {
                            fakeDb.Add(newContact);
                        }).
                        Returns(Task.CompletedTask);

            await contactDetailsService.CreateUserContactDetails(userId, email, CancellationToken.None);

            Assert.Single(fakeDb);
            Assert.Equal(userId, fakeDb.First().Id);
            Assert.Equal(email, fakeDb.First().Email);

            contactRepo.Verify(contactRepo => contactRepo.CreateAsync(It.IsAny<ContactDetails>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetUserContactDetails_ShouldReturnContactDetails_IfExists()
        {
            string userId = Guid.NewGuid().ToString();
            string email = "jimmyjabulani01@gmail.com";

            var existingContactDetails = new ContactDetails
            {
                Id = userId,
                Email = email,
            };

            contactRepo.Setup(contactRepo => contactRepo.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(existingContactDetails);

            var result = await contactDetailsService.GetUserContactDetails(userId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.IsType<ContactDetails>(result);

            contactRepo.Verify(contactRepo => contactRepo.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateIsEmailVerified_ShouldUpdateEmailVerificationStatus()
        {
            string contactId = Guid.NewGuid().ToString();

            bool isEmailVerified = false;
            bool isVerified = true;

            contactRepo.Setup(contactRepo => contactRepo.UpdateIsEmailVerified(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        Callback<string, CancellationToken>((Id, CancellationToken) =>
                        {
                            isEmailVerified = isVerified;
                        }).
                        Returns(Task.CompletedTask);

            await contactDetailsService.UpdateIsEmailVerified(contactId, CancellationToken.None);

            Assert.Equal(isVerified, isEmailVerified);

            contactRepo.Verify(contactRepo => contactRepo.UpdateIsEmailVerified(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateEmail_ReplacesOldEmailWithNewEmail()
        {
            string storedEmail = "jimmyjabulani01@gmail.com";
            string newEmail = "jabulanikhabana0@gmail.com";
            string accountId = Guid.NewGuid().ToString();

            contactRepo.Setup(c => c.UpdateEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        Callback<string, string, CancellationToken>((id, email, token) =>
                        {
                            storedEmail = email;
                        }).
                        Returns(Task.CompletedTask);

            await contactDetailsService.UpdateEmail(accountId, newEmail, CancellationToken.None);

            Assert.Equal(newEmail, storedEmail);

            contactRepo.Verify(c => c.UpdateEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

    }

}