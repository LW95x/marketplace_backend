using Marketplace.DataAccess.Entities;
using Marketplace.DataAccess.Services;
using Marketplace.Helpers;
using Microsoft.AspNetCore.Identity;

namespace Marketplace.BusinessLayer
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<IEnumerable<User>> FetchUsersAsync()
        {
            return await _userRepository.GetUsersAsync();
        }

        public async Task<User?> FetchUserByIdAsync(string userId)
        {
            return await _userRepository.GetUserByIdAsync(userId);
        }

        public async Task<IdentityResult> AddUser(User user, string password)
        {
            return await _userRepository.CreateUser(user, password);
        }

        public async Task<IdentityResult> RemoveUser(User user)
        {
            return await _userRepository.DeleteUser(user);
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            return await _userRepository.UpdateUserAsync(user);
        }

        public async Task<IdentityResult> EditUserPasswordAsync(User user, string currentPassword, string newPassword)
        {
            return await _userRepository.ChangeUserPasswordAsync(user, currentPassword, newPassword);
        }

        public async Task<bool> LoginUser(string userName, string password)
        {
            return await _userRepository.LoginUserAsync(userName, password);
        }

        public async Task<Result> LogoutUser()
        {
            return await _userRepository.LogoutUserAsync();
        }
    }
}
