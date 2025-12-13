namespace HubServer.DataContext.Models
{
    public class Chat
    {
        public long Id { get; set; }
        public string SenderId { get; set; } = null!;
        public string ReceiverId { get; set; } = null!;
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public long SequenceOrder { get; set; }
        public virtual ICollection<Message> Messages { get; set; } = [];
    }
}
