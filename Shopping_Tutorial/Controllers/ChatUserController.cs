using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using System.Net;
using System.Security.Claims;

namespace Shopping_Tutorial.Controllers
{
    [Authorize]
    public class ChatUserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ChatUserController> _logger;
        private readonly UserManager<AppUserModel> _userManager;

        public ChatUserController(IHttpClientFactory httpClientFactory, ILogger<ChatUserController> logger, UserManager<AppUserModel> userManager)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("API_Hub");
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await client.GetAsync($"api/conversations/chats/{userId}");
            var model = new ChatModel()
            {
                Users = await _userManager.Users.ToListAsync()
            };
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<ConversationModel>>();
                model.Conversations = result ?? [];
            }
            return View(model);
        }

        [HttpGet("/api/message")]
        public async Task<IActionResult> GetMessages()
        {
            var client = _httpClientFactory.CreateClient("API_Hub");
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await client.GetAsync($"api/conversations/chats/{userId}");
            if (response == null || !response.IsSuccessStatusCode)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            var result = await response.Content.ReadFromJsonAsync<List<ConversationModel>>();
            return Ok(result);
        }
    }
}
