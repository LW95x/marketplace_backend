using Marketplace.DataAccess.DbContexts;
using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.DataAccess.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly MarketplaceContext _context;

        public MessageRepository(MarketplaceContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Message>> GetMessagesBetweenUsers(string senderId, string receiverId)
        {
            if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(receiverId))
            {
                throw new ArgumentNullException();
            }

            return await _context.Messages
                .Where(m => 
                (m.SenderId == senderId && m.ReceiverId == receiverId) ||
                (m.SenderId == receiverId && m.ReceiverId == senderId))
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderByDescending(m => m.SentTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetAllUserMessages(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

           var messages = await _context.Messages
                    .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                    .Include(m => m.Sender)
                    .Include(m => m.Receiver)
                    .ToListAsync();

            var groupedConversations = messages
                 .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                 .Select(g => g.OrderByDescending(m => m.SentTime).First())
                 .OrderByDescending(m => m.SentTime)
                 .ToList();

            return groupedConversations;
        }

        public async Task<Result> DeleteMessage(string userId, string messageId)
        {
            try
            {
                var message = await _context.Messages
                    .FirstOrDefaultAsync(m => m.Id.ToString() == messageId);

                if (message == null)
                {
                    return Result.Fail("Message could not be found with this ID.");
                }

                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public async Task<Message> SaveMessage(string senderId, string receiverId, string content)
        {
            var message = new Message(senderId, receiverId, content);

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return message;
        }
    }
}
