using Marketplace.DataAccess.DbContexts;
using Marketplace.DataAccess.Entities;
using Marketplace.DataAccess.Services;
using Marketplace.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.DataAccess.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MarketplaceContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public UserRepository(MarketplaceContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        }
        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<IdentityResult> CreateUser(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<IdentityResult> DeleteUser(User user)
        {
            var userDependencies = await _context.Users
                .Include(u => u.Products)
                    .ThenInclude(p => p.Images)
                .Include(u => u.Orders)
                    .ThenInclude(o => o.OrderItems)
                .Include(u => u.ShoppingCart)
                    .ThenInclude(c => c.Items)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            if (userDependencies != null)
            {
                _context.ProductImages.RemoveRange(userDependencies.Products.SelectMany(p => p.Images));
                _context.Products.RemoveRange(userDependencies.Products);

                _context.OrderItems.RemoveRange(userDependencies.Orders.SelectMany(o => o.OrderItems));
                _context.Orders.RemoveRange(userDependencies.Orders);

                _context.ShoppingCartItems.RemoveRange(userDependencies.ShoppingCart.Items);
                _context.ShoppingCarts.Remove(userDependencies.ShoppingCart);

                await _context.SaveChangesAsync();
            }

            return await _userManager.DeleteAsync(user);
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> LoginUserAsync(string userName, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(userName, password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return true;
            }

            return false;
        }

        public async Task<Result> LogoutUserAsync()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public async Task<IdentityResult> ChangeUserPasswordAsync(User user, string currentPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public async Task<User?> GetUserByUsernameAsync(string userName)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
        }
    }
}
