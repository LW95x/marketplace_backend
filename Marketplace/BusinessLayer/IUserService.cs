using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.BusinessLayer
{
    public interface IUserService
    {
        Task<IEnumerable<User>> FetchUsersAsync();
        Task<User?> FetchUserByIdAsync(string userId);
        Task<IdentityResult> AddUser(User user, string password);
        Task<IdentityResult> RemoveUser(User user);
        Task<IdentityResult> EditUserPasswordAsync(User user, string currentPassword, string newPassword);
        Task<User> UpdateUserAsync(User user);
        Task<bool> LoginUser(string userName, string password);
        Task<Result> LogoutUser();
        Task<User?> FetchUserByUsernameAsync(string username);

    }
}
