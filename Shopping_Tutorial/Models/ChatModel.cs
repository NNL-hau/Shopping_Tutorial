namespace Shopping_Tutorial.Models
{
    public class ChatModel
    {
        public List<ConversationModel> Conversations { get; set; } = [];
        public List<AppUserModel> Users { get; set; } = [];
    }
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
    public class MessageModel
    {
        public string Text { get; set; } = string.Empty;
        public bool IsMine { get; set; }
    }
}
