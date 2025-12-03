using Moq;
using AuthApiBackend.Services;
using AuthApiBackend.Models;
using AuthApiBackend.DTOs;
using AuthApiBackend.Exceptions.ExceptionTypes;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.DTOs.ResponseDtos;
using Microsoft.AspNetCore.JsonPatch;

namespace AuthApiBackendTest
{

    [Collection("Env collection")]
    public class UnitTest1
    {

        private readonly Mock<IUserRepository> userRepo;
        private readonly UserService userService;

        public UnitTest1()
        {
            userRepo = new Mock<IUserRepository>();
            userService = new UserService(userRepo.Object) { };
        }

        [Fact]
        public async Task CreateUserAsync_ReturnUserId_IfUserCreated()
        {
            var fakeDb = new List<User>();

            userRepo.Setup(userRepo => userRepo.GetUser(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                     ReturnsAsync(false);

            userRepo.Setup(userRepo => userRepo.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).
                     Callback<User, CancellationToken>((user, cancellationToken) =>
                     {
                         fakeDb.Add(user);
                     }).
                     Returns(Task.CompletedTask);

            var registerUser = new RegisterDto
            {
                IdNumber = "1234567891012",
                FirstName = "Jimmy",
                Surname = "Khabana",
            };

            var id = await userService.CreateUserAsync(registerUser, CancellationToken.None);

            Assert.IsType<string>(id);
            Assert.Single(fakeDb);

            userRepo.Verify(userRepo => userRepo.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
            userRepo.Verify(userRepo => userRepo.GetUser(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_ThrowsUserAlreadyExistException_IfUserExist()
        {
            userRepo.Setup(userRepo => userRepo.GetUser(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                     ReturnsAsync(true);

            var registerUser = new RegisterDto
            {
                IdNumber = "1234567891012",
                FirstName = "Jimmy",
                Surname = "Khabana",
            };

            var ex = await Assert.ThrowsAnyAsync<UserAlreadyExistException>(async () =>
                     await userService.CreateUserAsync(registerUser, CancellationToken.None));

            userRepo.Verify(userRepo => userRepo.GetUser(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            userRepo.Verify(userRepo => userRepo.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetUserAsync_ReturnUserId_IfUserExist()
        {
            string idNumber = "1234567891012";

            userRepo.Setup(userRepo => userRepo.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new UserResponse( 
                                                    Guid.NewGuid().ToString(), 
                                                    1 
                                                  )
                                 );

            var response = await userService.GetUserIdAsync(idNumber, CancellationToken.None);

            Assert.IsType<UserResponse>(response);

            userRepo.Verify(userRepo => userRepo.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetUserIdAsync_ThrowsUserNotFoundException_IfUserDoesNotExist()
        {
            string idNumber = "1234567891012";

            userRepo.Setup(userRepo => userRepo.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((UserResponse?)null);

            var ex = await Assert.ThrowsAsync<UserNotFoundException>(async () =>
                     await userService.GetUserIdAsync(idNumber, CancellationToken.None));

            Assert.Equal("User does not exist", ex.Message);

            userRepo.Verify(userRepo => userRepo.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task FindUserLoginNumberById_throwsException_IfUserDoesNotExist()
        {
            string nationalId = "0130534589867";

            userRepo.Setup(c => c.GetUserId(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                     ReturnsAsync((ForgottenLoginNumber?)null);

            var ex = await Assert.ThrowsAsync<Exception>(async () =>
                     await userService.FindUserLoginNumberById(nationalId, CancellationToken.None));

            Assert.Equal("An email has been sent to ******@gmail.com", ex.Message);

            userRepo.Verify(c => c.GetUserId(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task FindUserLoginNumberById_ReturnsUserLoginNumberAndEmail_IfUserExist()
        {
            string nationalId = "0130534589867";

            userRepo.Setup(c => c.GetUserId(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                     ReturnsAsync(
                                   new ForgottenLoginNumber(
                                                             UserEmail: "jimmyjabulani01@gmail.com",
                                                             LoginNumber: "250000001"
                                                           )
                                 );

            await userService.FindUserLoginNumberById(nationalId, CancellationToken.None);

            userRepo.Verify(c => c.GetUserId(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetUserPkById_ReturnsUserPk_IfUserExist()
        {
            userRepo.Setup(c => c.GetUserPkById(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                     ReturnsAsync(Guid.NewGuid().ToString());

            var userPk = await userService.GetUserPkById("0107288586865", CancellationToken.None);

            Assert.NotNull(userPk);

            userRepo.Verify(c => c.GetUserPkById(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetUserPkById_ThrowsException_WhenNoMatch()
        {
            userRepo.Setup(c => c.GetUserPkById(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                     ReturnsAsync((string?)null);

            var ex = await Assert.ThrowsAsync<NoAccountMatchException>(async () =>
                     await userService.GetUserPkById("8993930022243", CancellationToken.None));

            Assert.Equal("No account associated with the user", ex.Message);

            userRepo.Verify(c => c.GetUserPkById(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteUser_RemovesUser_IfGracePeriodForAccountRetrievalEnds()
        {
            var user = new User { Id = Guid.NewGuid().ToString() };

            var fakeDb = new List<User> { user };

            userRepo.Setup(c => c.DeleteUser(It.IsAny<User>(), It.IsAny<CancellationToken>())).
                     Callback<User, CancellationToken>((user, cancellationToken) =>
                     {
                         fakeDb.Remove(user);
                     }).
                     Returns(Task.CompletedTask);

            await userService.DeleteUser(user, CancellationToken.None);

            Assert.Empty(fakeDb);

            userRepo.Verify(c => c.DeleteUser(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserDataPartially_UpdateFirstNameOnly_IfSurnameIsNull()
        {
            var user = new User { Id = Guid.NewGuid().ToString(), FirstName = "Jabulani", Surname = "Khabana" };

            var fakeDb = new List<User> {user};

            var userUpdate = new JsonPatchDocument<UserPatchDetails>();
            userUpdate.Replace(c => c.FirstName, "Jimmy");

            var useer = new UserPatchDetails();
            userUpdate.ApplyTo(useer);

            userRepo.Setup(c => c.PatchUserDetails(It.IsAny<string>(), It.IsAny<JsonPatchDocument<UserPatchDetails>>(), 
                                It.IsAny<UserPatchDetails>(), It.IsAny<CancellationToken>())).
                    Callback<string, JsonPatchDocument<UserPatchDetails>, UserPatchDetails, CancellationToken>
                                ((Id, patch, userr, cancellationToken) =>
                                {
                                    var dbUser = fakeDb.First(u => u.Id == Id);
                                    foreach(var op in patch.Operations)
                                    {
                                        switch (op.path.ToLower())
                                        {
                                            case "/firstname": dbUser.FirstName = userr.FirstName ?? dbUser.FirstName; break;
                                            case "/surname":   dbUser.Surname = userr.Surname ?? dbUser.Surname; break;
                                    }
                                }
                                }).
                                Returns(Task.CompletedTask);

            await userService.UpdateUserPartially(user.Id, userUpdate, useer, CancellationToken.None);

            Assert.Equal("Jimmy", user.FirstName);
            Assert.Equal("Khabana", user.Surname);

            userRepo.Verify(c => c.PatchUserDetails(It.IsAny<string>(), It.IsAny<JsonPatchDocument<UserPatchDetails>>(),
                               It.IsAny<UserPatchDetails>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserDataPartially_UpdateFirstNameAndSurname_WhenBothNotNull()
        {
            var user = new User { Id = Guid.NewGuid().ToString(), FirstName = "Jabulani", Surname = "Khabana" };

            var fakeDb = new List<User> { user };

            var userUpdate = new JsonPatchDocument<UserPatchDetails>();
            userUpdate.Replace(c => c.FirstName, "Jimmy");
            userUpdate.Replace(c => c.Surname, "Munyai");

            var useer = new UserPatchDetails();
            userUpdate.ApplyTo(useer);

            userRepo.Setup(c => c.PatchUserDetails(It.IsAny<string>(), It.IsAny<JsonPatchDocument<UserPatchDetails>>(),
                                It.IsAny<UserPatchDetails>(), It.IsAny<CancellationToken>())).
                    Callback<string, JsonPatchDocument<UserPatchDetails>, UserPatchDetails, CancellationToken>
                                ((Id, patch, userr, cancellationToken) =>
                                {
                                    var dbUser = fakeDb.First(u => u.Id == Id);
                                    foreach (var op in patch.Operations)
                                    {
                                        switch (op.path.ToLower())
                                        {
                                            case "/firstname": dbUser.FirstName = userr.FirstName ?? dbUser.FirstName; break;
                                            case "/surname": dbUser.Surname = userr.Surname ?? dbUser.Surname; break;
                                        }
                                    }
                                }).
                                Returns(Task.CompletedTask);

            await userService.UpdateUserPartially(user.Id, userUpdate, useer, CancellationToken.None);

            Assert.Equal("Jimmy", user.FirstName);
            Assert.Equal("Munyai", user.Surname);

            userRepo.Verify(c => c.PatchUserDetails(It.IsAny<string>(), It.IsAny<JsonPatchDocument<UserPatchDetails>>(),
                               It.IsAny<UserPatchDetails>(), It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task UpdateUserDataPartially_UpdateSurnameOnly_WhenFirstNameIsNull()
        {
            var user = new User { Id = Guid.NewGuid().ToString(), FirstName = "Jabulani", Surname = "Khabana" };

            var fakeDb = new List<User> { user };

            var userUpdate = new JsonPatchDocument<UserPatchDetails>();
            userUpdate.Replace(c => c.Surname, "Munyai");

            var useer = new UserPatchDetails();
            userUpdate.ApplyTo(useer);

            userRepo.Setup(c => c.PatchUserDetails(It.IsAny<string>(), It.IsAny<JsonPatchDocument<UserPatchDetails>>(),
                                It.IsAny<UserPatchDetails>(), It.IsAny<CancellationToken>())).
                    Callback<string, JsonPatchDocument<UserPatchDetails>, UserPatchDetails, CancellationToken>
                                ((Id, patch, userr, cancellationToken) =>
                                {
                                    var dbUser = fakeDb.First(u => u.Id == Id);
                                    foreach (var op in patch.Operations)
                                    {
                                        switch (op.path.ToLower())
                                        {
                                            case "/firstname": dbUser.FirstName = userr.FirstName ?? dbUser.FirstName; break;
                                            case "/surname": dbUser.Surname = userr.Surname ?? dbUser.Surname; break;
                                        }
                                    }
                                }).
                                Returns(Task.CompletedTask);

            await userService.UpdateUserPartially(user.Id, userUpdate, useer, CancellationToken.None);

            Assert.Equal("Jabulani", user.FirstName);
            Assert.Equal("Munyai", user.Surname);

            userRepo.Verify(c => c.PatchUserDetails(It.IsAny<string>(), It.IsAny<JsonPatchDocument<UserPatchDetails>>(),
                               It.IsAny<UserPatchDetails>(), It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task UpdateUserDataPartially_UpdateNone_WhenBothAreNull()
        {
            var user = new User { Id = Guid.NewGuid().ToString(), FirstName = "Jabulani", Surname = "Khabana" };

            var fakeDb = new List<User> { user };

            var userUpdate = new JsonPatchDocument<UserPatchDetails>();

            var useer = new UserPatchDetails();
            userUpdate.ApplyTo(useer);

            userRepo.Setup(c => c.PatchUserDetails(It.IsAny<string>(), It.IsAny<JsonPatchDocument<UserPatchDetails>>(),
                                It.IsAny<UserPatchDetails>(), It.IsAny<CancellationToken>())).
                    Callback<string, JsonPatchDocument<UserPatchDetails>, UserPatchDetails, CancellationToken>
                                ((Id, patch, userr, cancellationToken) =>
                                {
                                    var dbUser = fakeDb.First(u => u.Id == Id);
                                    foreach (var op in patch.Operations)
                                    {
                                        switch (op.path.ToLower())
                                        {
                                            case "/firstname": dbUser.FirstName = userr.FirstName ?? dbUser.FirstName; break;
                                            case "/surname": dbUser.Surname = userr.Surname ?? dbUser.Surname; break;
                                        }
                                    }
                                }).
                                Returns(Task.CompletedTask);

            await userService.UpdateUserPartially(user.Id, userUpdate, useer, CancellationToken.None);

            Assert.Equal("Jabulani", user.FirstName);
            Assert.Equal("Khabana", user.Surname);

            userRepo.Verify(c => c.PatchUserDetails(It.IsAny<string>(), It.IsAny<JsonPatchDocument<UserPatchDetails>>(),
                               It.IsAny<UserPatchDetails>(), It.IsAny<CancellationToken>()), Times.Once);
        }

    }

}