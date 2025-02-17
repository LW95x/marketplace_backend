using AutoMapper;
using Marketplace.BusinessLayer;
using Marketplace.Controllers;
using Marketplace.DataAccess.Entities;
using Marketplace.DataAccess.Services;
using Marketplace.Helpers;
using Marketplace.MapperProfiles;
using Marketplace.Models;
using Marketplace.Test.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Marketplace.Test
{
    public class UserTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ILogger<UsersController>> _mockControllerLogger;
        private readonly Mock<ILogger<UserAuthController>> _mockAuthControllerLogger;
        private readonly Mock<ILogger<UserService>> _mockServiceLogger;
        private readonly List<User> _testUsers;
        private readonly UsersController _controller;
        private readonly UserService _service;
        private readonly UserAuthController _authController;

        public UserTests()
        {
            var config = new MapperConfiguration(c => c.AddProfile<UserProfile>());
            _mapper = config.CreateMapper();

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration
                .Setup(config => config["SecretForKey"])
                .Returns("VGhpcyBpcyBhIHZlcnkgc2VjdXJlIGtleSBmb3IgdGVzdGluZw==");

            _mockConfiguration = mockConfiguration;

            _mockUserService = new Mock<IUserService>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockControllerLogger = new Mock<ILogger<UsersController>>();
            _mockAuthControllerLogger = new Mock<ILogger<UserAuthController>>();
            _mockServiceLogger = new Mock<ILogger<UserService>>();

            _testUsers = TestDataFactory.GetUsers();

            _controller = new UsersController(_mockUserService.Object, _mapper, _mockControllerLogger.Object);
            _service = new UserService(_mockUserRepository.Object);
            _authController = new UserAuthController(_mockUserService.Object, _mockAuthControllerLogger.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task GetUsers_WhenValidRequest_ReturnsOkWithAllUsers()
        {
            // Arrange
            var users = _testUsers;

            _mockUserService.Setup(service => service.FetchUsersAsync()).ReturnsAsync(users);

            // Act
            var result = await _controller.GetUsers();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<UserForResponseDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedUsers = Assert.IsAssignableFrom<IEnumerable<UserForResponseDto>>(okResult.Value);

            Assert.Equal(users.Count, returnedUsers.Count());
        }

        [Fact]
        public async Task GetUserById_WhenValidUserId_ReturnsOkWithSingleUser()
        {
            // Arrange
            var user = _testUsers[0];
            var userId = _testUsers[0].Id;

            _mockUserService.Setup(service => service.FetchUserByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUser = Assert.IsAssignableFrom<UserForResponseDto>(okResult.Value);

            Assert.Equal(userId, returnedUser.UserId);
        }

        [Fact]
        public async Task GetUserById_WhenInvalidUserId_ReturnsNotFound()
        {
            // Arrange
            var userId = "notAnId";

            _mockUserService.Setup(service => service.FetchUserByIdAsync(userId)).ReturnsAsync((User)null!);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task CreateUser_WhenUserIsValid_ReturnsCreated()
        {
            // Arrange
            var userCreationDto = new UserForCreationDto
            {
                Email = "billybob@outlook.com",
                Password = "billybobbington",
                UserName = "billybob"
            };

            var user = _mapper.Map<User>(userCreationDto);

            _mockUserService.Setup(service => service.AddUser(It.IsAny<User>(), userCreationDto.Password)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.CreateUser(userCreationDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal(201, createdResult.StatusCode);

            var userResult = Assert.IsType<UserForResponseDto>(createdResult.Value);
            Assert.Equal(user.UserName, userResult.UserName);
            Assert.Equal(user.Email, userResult.Email);
        }

        [Fact]
        public async Task CreateUser_WhenInvalidUser_ReturnsBadRequest()
        {
            var userCreationDto = new UserForCreationDto
            {
                Email = "billybob@outlook.com",
                Password = "billybobbington",
                UserName = "name"
            };

            var user = _mapper.Map<User>(userCreationDto);

            _controller.ModelState.AddModelError("UserName", "Username must contain at least 6 characters.");

            // Act
            var result = await _controller.CreateUser(userCreationDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);

            var modelState = badRequestResult.Value as SerializableError;
            Assert.NotNull(modelState);
            Assert.True(modelState.ContainsKey("UserName"));

            var usernameError = modelState["UserName"] as string[];
            Assert.NotNull(usernameError);
            Assert.Contains("Username must contain at least 6 characters.", usernameError);
        }

        [Fact]
        public async Task CreateUser_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var userCreationDto = new UserForCreationDto
            {
                Email = "billybob@outlook.com",
                Password = "billybobbington",
                UserName = "billybob"
            };

            var user = _mapper.Map<User>(userCreationDto);

            _mockUserService.Setup(service => service.AddUser(It.IsAny<User>(), userCreationDto.Password)).ReturnsAsync(IdentityResult.Failed());

            // Act
            var result = await _controller.CreateUser(userCreationDto);

            // Assert
            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, actionResult.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_WhenValidUserId_ReturnsNoContent()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var user = _testUsers[0];

            _mockUserService.Setup(service => service.FetchUserByIdAsync(userId)).ReturnsAsync(user);

            _mockUserService.Setup(service => service.RemoveUser(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            var deletedResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, deletedResult.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_WhenInvalidUserId_ReturnsNotFound()
        {
            // Arrange
            var userId = "notAnId";

            _mockUserService.Setup(service => service.FetchUserByIdAsync(userId)).ReturnsAsync((User)null!);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var user = _testUsers[0];

            _mockUserService.Setup(service => service.FetchUserByIdAsync(userId)).ReturnsAsync(user);

            _mockUserService.Setup(service => service.RemoveUser(user)).ReturnsAsync(IdentityResult.Failed());

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, actionResult.StatusCode);
        }

        [Fact]
        public async Task ChangeUserPassword_WhenValidRequest_ReturnsOk()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var user = _testUsers[0];
            var currentPassword = "current";
            var newPassword = "new";

            _mockUserService.Setup(service => service.FetchUserByIdAsync(userId)).ReturnsAsync(user);

            _mockUserService.Setup(service => service.EditUserPasswordAsync(user, currentPassword, newPassword)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.ChangeUserPassword(userId, currentPassword, newPassword);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task ChangeUserPassword_WhenUserDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var userId = "notAnId";
            var currentPassword = "current";
            var newPassword = "new";

            _mockUserService.Setup(service => service.FetchUserByIdAsync(userId)).ReturnsAsync((User)null!);

            // Act
            var result = await _controller.ChangeUserPassword(userId, currentPassword, newPassword);

            // Assert
            var okResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, okResult.StatusCode);
        }

        [Fact]
        public async Task ChangeUserPassword_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var user = _testUsers[0];
            var currentPassword = "current";
            var newPassword = "new";

            _mockUserService.Setup(service => service.FetchUserByIdAsync(userId)).ReturnsAsync(user);

            _mockUserService.Setup(service => service.EditUserPasswordAsync(user, currentPassword, newPassword)).ReturnsAsync(IdentityResult.Failed());

            // Act
            var result = await _controller.ChangeUserPassword(userId, currentPassword, newPassword);

            // Assert
            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, actionResult.StatusCode);
        }

        [Fact]
        public async Task UpdateUser_WhenValidRequest_ReturnsNoContent()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var user = _testUsers[0];

            var updatedImageUrl = "https://www.imgur.com/aEtFhI";

            var userToPatch = _mapper.Map<UserForUpdateDto>(user);

            var patchDocument = new JsonPatchDocument<UserForUpdateDto>();
            patchDocument.Replace(u => u.ImageUrl, updatedImageUrl);

            var updatedUser = _testUsers[0];
            updatedUser.ImageUrl = updatedImageUrl;

            _mockUserService.Setup(service => service.FetchUserByIdAsync(userId)).ReturnsAsync(user);

            _mockUserService.Setup(service => service.UpdateUserAsync(It.IsAny<User>())).Callback<User>(u =>
            {
                u.ImageUrl = updatedImageUrl;
            })
                .ReturnsAsync(updatedUser);

            // Act
            var result = await _controller.UpdateUser(userId, patchDocument);

            // Assert
            var updatedResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, updatedResult.StatusCode);
        }

        [Fact]
        public async Task UpdateUser_WhenInvalidRequestBody_ReturnsBadRequest()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var user = _testUsers[0];

            var updatedImageUrl = "string";

            var userToPatch = _mapper.Map<UserForUpdateDto>(user);

            var patchDocument = new JsonPatchDocument<UserForUpdateDto>();
            patchDocument.Replace(u => u.ImageUrl, updatedImageUrl);

            var updatedUser = _testUsers[0];
            updatedUser.ImageUrl = updatedImageUrl;

            _controller.ModelState.AddModelError("ImageUrl", "The profile picture URL must be under 255 characters in length.");

            _mockUserService.Setup(service => service.FetchUserByIdAsync(userId)).ReturnsAsync(user);

            _mockUserService.Setup(service => service.UpdateUserAsync(It.IsAny<User>())).Callback<User>(u =>
            {
                u.ImageUrl = updatedImageUrl;
            })
                .ReturnsAsync(updatedUser);

            // Act
            var result = await _controller.UpdateUser(userId, patchDocument);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);

            var modelState = badRequestResult.Value as SerializableError;
            Assert.NotNull(modelState);
            Assert.True(modelState.ContainsKey("ImageUrl"));

            var imageUrlError = modelState["ImageUrl"] as string[];
            Assert.NotNull(imageUrlError);
            Assert.Contains("The profile picture URL must be under 255 characters in length.", imageUrlError);
        }

        [Fact]
        public async Task UpdateUser_WhenInvalidUser_ReturnsNotFound()
        {
            // Arrange
            var userId = "notAnId";

            var updatedImageUrl = "https://www.imgur.com/aEtFhI";

            var patchDocument = new JsonPatchDocument<UserForUpdateDto>();
            patchDocument.Replace(u => u.ImageUrl, updatedImageUrl);

            var updatedUser = _testUsers[0];
            updatedUser.ImageUrl = updatedImageUrl;

            _controller.ModelState.AddModelError("ImageUrl", "The profile picture URL must be under 255 characters in length.");

            _mockUserService.Setup(service => service.FetchUserByIdAsync(userId)).ReturnsAsync((User)null!);

            _mockUserService.Setup(service => service.UpdateUserAsync(It.IsAny<User>())).ReturnsAsync((User)null!);

            // Act
            var result = await _controller.UpdateUser(userId, patchDocument);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task UpdateUser_WhenExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var user = _testUsers[0];

            var updatedImageUrl = "https://www.imgur.com/aEtFhI";

            var userToPatch = _mapper.Map<UserForUpdateDto>(user);

            var patchDocument = new JsonPatchDocument<UserForUpdateDto>();
            patchDocument.Replace(u => u.ImageUrl, updatedImageUrl);

            var updatedUser = _testUsers[0];
            updatedUser.ImageUrl = updatedImageUrl;

            _mockUserService.Setup(service => service.FetchUserByIdAsync(userId)).ReturnsAsync(user);

            _mockUserService.Setup(service => service.UpdateUserAsync(It.IsAny<User>())).ReturnsAsync((User)null!);

            // Act
            var result = await _controller.UpdateUser(userId, patchDocument);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Failed to update the user due to an internal server error.", objectResult.Value);
        }

        [Fact]
        public async Task LoginUser_WhenValidRequest_ReturnsOk()
        {
            // Arrange
            var userLogin = new UserForLoginDto
            {
                UserName = "Billy",
                Password = "Billybob-123"
            };

            var user = _testUsers[0];

            _mockUserService.Setup(service => service.FetchUserByUsernameAsync(userLogin.UserName)).ReturnsAsync(user);

            _mockUserService.Setup(service => service.LoginUser(userLogin.UserName, userLogin.Password)).ReturnsAsync(true);

            // Act
            var result = await _authController.LoginUser(userLogin);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task LoginUser_WhenInvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var user = new UserForLoginDto
            {
                UserName = "billybob"
            };

            _authController.ModelState.AddModelError("Password", "The password field is required.");

            _mockUserService.Setup(service => service.LoginUser(user.UserName, user.Password)).ReturnsAsync(false);

            // Act
            var result = await _authController.LoginUser(user);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task LoginUser_WhenUsernameOrPasswordIncorrect_ReturnsUnauthorized()
        {
            // Arrange
            var user = new UserForLoginDto
            {
                UserName = "billybob",
                Password = "wrongpassword"
            };

            _mockUserService.Setup(service => service.LoginUser(user.UserName, user.Password)).ReturnsAsync(false);

            // Act
            var result = await _authController.LoginUser(user);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
            Assert.Equal("Username or password provided is incorrect.", unauthorizedResult.Value);
        }

        [Fact]
        public async Task LogoutUser_WhenValidRequest_ReturnsNoContent()
        {
            // Arrange
            _mockUserService.Setup(service => service.LogoutUser()).ReturnsAsync(Result.Success);

            // Act
            var result = await _authController.LogoutUser();

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        [Fact]
        public async Task LogoutUser_WhenLogoutFails_ReturnsInternalServerError()
        {
            // Arrange
            _mockUserService.Setup(service => service.LogoutUser()).ReturnsAsync(Result.Fail("Failed to log out user due to an internal server error."));

            // Act
            var result = await _authController.LogoutUser();

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Failed to log out user due to an internal server error.", objectResult.Value);
        }

        [Fact]
        public async Task FetchUsersAsync_WhenValidRequest_ReturnsListOfUsers()
        {
            // Arrange
            var users = _testUsers;

            _mockUserRepository.Setup(repo => repo.GetUsersAsync()).ReturnsAsync(users);

            // Act
            var result = await _service.FetchUsersAsync();

            // Assert
            var returnedUsers = Assert.IsType<List<User>>(result);
            Assert.Equal(users.Count, returnedUsers.Count);

            foreach(var user in users)
            {
                Assert.Contains(returnedUsers, u =>
                    u.Id == user.Id &&
                    u.UserName == user.UserName &&
                    u.Email == user.Email
                );
            }
        }

        [Fact]
        public async Task FetchUserByIdAync_WhenValidUserId_ReturnsUser()
        {
            // Arrange
            var userId = _testUsers[0].Id;
            var user = _testUsers[0];

            _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _service.FetchUserByIdAsync(userId);

            // Assert
            var returnedUser = Assert.IsType<User>(result);
            Assert.NotNull(returnedUser);

            Assert.Equal(userId, returnedUser.Id);
            Assert.Equal(user.UserName, returnedUser.UserName);
            Assert.Equal(user.Email, returnedUser.Email);
        }

        [Fact]
        public async Task FetchUserByIdAync_WhenInvalidUserId_ReturnsNull()
        {
            // Arrange
            var userId = "notAnId";
  

            _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync((User)null!);

            // Act
            var result = await _service.FetchUserByIdAsync(userId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddUser_WhenValidRequest_ReturnsIdentityResultSuccess()
        {
            // Arrange
            var user = _testUsers[0];
            var password = "password";

            _mockUserRepository.Setup(repo => repo.CreateUser(user, password)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.AddUser(user, password);

            // Assert
            var returnedResult = Assert.IsType<IdentityResult>(result);
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task AddUser_WhenInalidRequest_ReturnsIdentityResultFailure()
        {
            // Arrange
            var user = _testUsers[0];
            var password = "password";

            _mockUserRepository.Setup(repo => repo.CreateUser(user, password)).ReturnsAsync(IdentityResult.Failed());

            // Act
            var result = await _service.AddUser(user, password);

            // Assert
            var returnedResult = Assert.IsType<IdentityResult>(result);
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task RemoveUser_WhenValidRequest_ReturnsIdentityResultSuccess()
        {
            // Arrange
            var user = _testUsers[0];

            _mockUserRepository.Setup(repo => repo.DeleteUser(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.RemoveUser(user);

            // Assert
            var returnedResult = Assert.IsType<IdentityResult>(result);
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task RemoveUser_WhenInvalidRequest_ReturnsIdentityResultFailure()
        {
            // Arrange
            var user = new User
            {
                UserName = "Test",
            };

            _mockUserRepository.Setup(repo => repo.DeleteUser(user)).ReturnsAsync(IdentityResult.Failed());

            // Act
            var result = await _service.RemoveUser(user);

            // Assert
            var returnedResult = Assert.IsType<IdentityResult>(result);
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task UpdateUserAsync_WhenValidRequest_ReturnsUpdatedUser()
        {
            // Arrange
            var user = _testUsers[0];
            var updatedUser = _testUsers[0];
            updatedUser.ImageUrl = "https://www.imgur.com/AeIoU";

            _mockUserRepository.Setup(repo => repo.UpdateUserAsync(user)).ReturnsAsync(updatedUser);

            // Act
            var result = await _service.UpdateUserAsync(user);

            // Assert
            var returnedResult = Assert.IsType<User>(result);
            Assert.Equal(updatedUser.ImageUrl, result.ImageUrl);
        }

        [Fact]
        public async Task EditUserPasswordAsync_WhenValidRequest_ReturnsIdentityResultSuccess()
        {
            // Arrange
            var user = _testUsers[0];
            var currentPassword = "current";
            var newPassword = "newpassword";

            _mockUserRepository.Setup(repo => repo.ChangeUserPasswordAsync(user, currentPassword, newPassword)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.EditUserPasswordAsync(user, currentPassword, newPassword);

            // Assert
            var returnedResult = Assert.IsType<IdentityResult>(result);
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task EditUserPasswordAsync_WhenInvalidRequest_ReturnsIdentityResultFailure()
        {
            // Arrange
            var user = _testUsers[0];
            var currentPassword = "a";
            var newPassword = "a";

            _mockUserRepository.Setup(repo => repo.ChangeUserPasswordAsync(user, currentPassword, newPassword)).ReturnsAsync(IdentityResult.Failed());

            // Act
            var result = await _service.EditUserPasswordAsync(user, currentPassword, newPassword);

            // Assert
            var returnedResult = Assert.IsType<IdentityResult>(result);
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task LoginUser_WhenValidRequest_ReturnsTrue()
        {
            // Arrange
            var userName = _testUsers[0].UserName;
            Assert.NotNull(userName);
            var password = "password";

            _mockUserRepository.Setup(repo => repo.LoginUserAsync(userName, password)).ReturnsAsync(true);

            // Act
            var result = await _service.LoginUser(userName, password);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task LoginUser_WhenInvalidRequest_ReturnsFalse()
        {
            // Arrange
            var userName = _testUsers[0].UserName;
            Assert.NotNull(userName);
            var password = "wrong";

            _mockUserRepository.Setup(repo => repo.LoginUserAsync(userName, password)).ReturnsAsync(false);

            // Act
            var result = await _service.LoginUser(userName, password);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task LogoutUser_WhenSuccessful_ReturnsSuccessResult()
        {
            // Arrange
            _mockUserRepository.Setup(repo => repo.LogoutUserAsync()).ReturnsAsync(Result.Success);

            // Act
            var result = await _service.LogoutUser();

            // Assert
            var returnedResult = Assert.IsType<Result>(result);
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task LogoutUser_WhenUnsuccessful_ReturnsFailureResult()
        {
            // Arrange
            _mockUserRepository.Setup(repo => repo.LogoutUserAsync()).ReturnsAsync(Result.Fail("Failed to log out user."));

            // Act
            var result = await _service.LogoutUser();

            // Assert
            var returnedResult = Assert.IsType<Result>(result);
            Assert.False(result.Succeeded);
            Assert.Equal("Failed to log out user.", result.Error);
        }
    }
}
