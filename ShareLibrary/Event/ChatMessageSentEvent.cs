namespace ShareLibrary.Event
{
    public class ChatMessageEvent
    {
        public string Sender { get; set; }      // "student" hoặc "admin"
        public string Receiver { get; set; }    // "admin" hoặc "student"
        public string Message { get; set; }
        //public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
