namespace HubServer.Models
{
    public class ConversationModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public DateTime LastMessageTime { get; set; }
        public List<MessageModel> Messages { get; set; } = [];
    }
}
