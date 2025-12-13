namespace HubServer.Models
{
    public class MessageModel
    {
        public string Text { get; set; } = string.Empty;
        public bool IsMine { get; set; }
    }
}
