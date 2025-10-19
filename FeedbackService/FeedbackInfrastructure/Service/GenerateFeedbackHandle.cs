using ShareLibrary;
using ShareLibrary.Event;

namespace FeedbackService.Infrastructure
{
   
    public class GenerateFeedbackHandle : IEventHandler<CodeSubmittedEvents>
    {
        private readonly IEventBus _eventBus;

        public GenerateFeedbackHandle(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public async Task Handle(CodeSubmittedEvents @event)
        {
            Console.WriteLine($"[FeedbackService] Received CodeSubmittedEvents for Assignment {@event.AssignmentId}");

            
            var feedbackText = $"Output: {@event.Output}\nStatus: {@event.Status}\nExecutionTime: {@event.ExecutionTime}s\nMemoryUsed: {@event.MemoryUsed}KB";

            // 🔹 Giả lập tính điểm (ví dụ)
            double score = @event.Status == "Accepted" ? 10.0 : 0.0;

            // 🔹 Tạo event phản hồi
            var feedbackEvent = new FeedbackGeneratedEvent
            {
                SubmissionId = Guid.NewGuid(), // nếu có submissionId thực thì thay vào
                UserEmail = "user@example.com", // nếu biết email thì thay vào
                Score = score,
                ResultStatus = @event.Status,
                Feedback = feedbackText
            };

           
            _eventBus.Publish(feedbackEvent);

            Console.WriteLine($"[FeedbackService] Published FeedbackGeneratedEvent for Assignment {@event.AssignmentId}");

            await Task.CompletedTask;
        }
    }
}
