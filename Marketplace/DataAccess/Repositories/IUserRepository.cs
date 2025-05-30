using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;
using Microsoft.AspNetCore.Identity;

namespace Marketplace.DataAccess.Services
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetUsersAsync();
        Task<User?> GetUserByIdAsync(string userId);
        Task<IdentityResult> CreateUser(User user, string password);
        Task<IdentityResult> DeleteUser(User user);
        Task<User> UpdateUserAsync(User user);
        Task<bool> LoginUserAsync(string userName, string password);
        Task<Result> LogoutUserAsync();
        Task<IdentityResult> ChangeUserPasswordAsync(User user, string currentPassword, string newPassword);
        Task<User?> GetUserByUsernameAsync(string userName);    
    }
}
