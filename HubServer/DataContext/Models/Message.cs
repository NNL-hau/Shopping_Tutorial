namespace HubServer.DataContext.Models
{
    public class Message
    {
        public long Id { get; set; }
        public long ChatId { get; set; }
        public string Content { get; set; } = null!;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public long SequenceId { get; set; }
        public string UserId { get; set; } = null!;
        public virtual Chat Chat { get; set; } = null!;
    }
}
