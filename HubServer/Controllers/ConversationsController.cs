using HubServer.DataContext;
using HubServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HubServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationsController : ControllerBase
    {
        private readonly ILogger<ConversationsController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ChatContext _context;
        private readonly IConfiguration _configuration;

        public ConversationsController(ILogger<ConversationsController> logger, IHttpClientFactory httpClientFactory, ChatContext context, IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("chats/{userId}")]
        public async Task<IActionResult> ChatLoading(string userId)
        {
            _logger.LogInformation(_configuration.GetSection("API").ToString());
            var clientName = _configuration.GetSection("API").Value;
            var client = _httpClientFactory.CreateClient(clientName!);
            var response = await client.GetAsync("api/get/users");
            if (response == null)
            {
                return NotFound();
            }
            var usersInfo = await response.Content.ReadFromJsonAsync<List<UserModel>>() ?? throw new("nullable");
            var chatListReceiver = _context.Chats.Where(c => c.ReceiverId == userId).OrderByDescending(c => c.SequenceOrder).ToList();

            var results = new List<ConversationModel>();

            foreach (var chat in chatListReceiver)
            {
                var name = usersInfo.FirstOrDefault(u => u.Id == chat.SenderId)?.UserName ?? "Unknown User";
                results.Add(new ConversationModel
                {
                    Id = chat.Id,
                    Name = name,
                    SenderId = chat.SenderId,
                    ReceiverId = chat.ReceiverId,
                    Avatar = $"https://ui-avatars.com/api/?name={name}&background=5b6bff&color=fff",
                    LastMessage = chat.Messages.OrderByDescending(m => m.SequenceId).FirstOrDefault()!.Content ?? string.Empty,
                    LastMessageTime = chat.Messages.OrderByDescending(m => m.SequenceId).FirstOrDefault()!.SentAt,
                    Messages = chat.Messages.OrderBy(m => m.SequenceId).Select(m => new MessageModel
                    {
                        Text = m.Content,
                        IsMine = m.UserId == userId,
                    }).ToList()
                });
            }

            var chatListSender = _context.Chats.Where(c => c.SenderId == userId).OrderByDescending(c => c.SequenceOrder).ToList();
            foreach (var chat in chatListSender)
            {
                var name = usersInfo.FirstOrDefault(u => u.Id == chat.ReceiverId)?.UserName ?? "Unknown User";
                results.Add(new ConversationModel
                {
                    Id = chat.Id,
                    Name = name,
                    SenderId = chat.SenderId,
                    ReceiverId = chat.ReceiverId,
                    Avatar = $"https://ui-avatars.com/api/?name={name}&background=5b6bff&color=fff",
                    LastMessage = chat.Messages.OrderByDescending(m => m.SequenceId).FirstOrDefault()!.Content ?? string.Empty,
                    LastMessageTime = chat.Messages.OrderByDescending(m => m.SequenceId).FirstOrDefault()!.SentAt,
                    Messages = chat.Messages.OrderBy(m => m.SequenceId).Select(m => new MessageModel
                    {
                        Text = m.Content,
                        IsMine = m.UserId == userId,
                    }).ToList()
                });
            }
            return Ok(results);
        }
    }
}
