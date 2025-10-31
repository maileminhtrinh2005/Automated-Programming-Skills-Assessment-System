using System;

namespace ShareLibrary.Event
{
 
    public class NotificationCreatedEvent
    {
        public Guid NotificationId { get; set; }        
        public string? UserEmail { get; set; }              
        public string? Title { get; set; }                 
        public string? Content { get; set; }              
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int StudentId { get; set; }
    }
}
