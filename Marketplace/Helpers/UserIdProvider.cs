using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Marketplace.Helpers
{
    public class UserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
