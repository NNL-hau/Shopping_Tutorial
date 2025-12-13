using HubServer.DataContext;
using HubServer.DataContext.Models;
using Microsoft.AspNetCore.SignalR;

namespace HubServer.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;
        private readonly ChatContext _context;

        public ChatHub(ILogger<ChatHub> logger, ChatContext context)
        {
            _logger = logger;
            _context = context;
        }
        public async Task SendMessage(string id, string senderId, string receiverId, string user, string message, bool isFirstChat)
        {
            _logger.LogInformation("info {i}, {s}, {r}, {u}, {m}", id, senderId, receiverId, user, message);

            if (!string.IsNullOrEmpty(id))
            {
                var chatId = Convert.ToInt64(id);
                var chat = await _context.Chats.FindAsync(chatId);
                if (chat != null)
                {
                    var newMessage = new Message
                    {
                        ChatId = chatId,
                        Content = message,
                        SentAt = DateTime.UtcNow,
                        UserId = senderId
                    };
                    var preMessage = _context.Messages.Where(m => m.ChatId == chatId).OrderByDescending(m => m.SequenceId).FirstOrDefault();
                    if (preMessage != null)
                    {
                        newMessage.SequenceId = preMessage.SequenceId + 1;
                    }
                    else
                    {
                        newMessage.SequenceId = 1;
                    }
                    await _context.Messages.AddAsync(newMessage);
                    chat.SequenceOrder++;
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                var newChat = new Chat
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    CreateAt = DateTime.UtcNow
                };
                newChat.SequenceOrder = _context.Chats.Count() + 1;
                await _context.Chats.AddAsync(newChat);
                await _context.SaveChangesAsync();
                var newMessage = new Message
                {
                    ChatId = newChat.Id,
                    Content = message,
                    SentAt = DateTime.UtcNow,
                    SequenceId = 1,
                    UserId = senderId
                };
                await _context.Messages.AddAsync(newMessage);
                await _context.SaveChangesAsync();
                id = _context.Chats.OrderByDescending(c => c.Id).FirstOrDefault()?.Id.ToString() ?? string.Empty;
            }

            await Clients.All.SendAsync("ReceiveMessage", id, senderId, receiverId, user, message, isFirstChat);
        }
    }
}
