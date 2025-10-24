// FeedbackService.Infrastructure/Handlers/GenerateFeedbackHandler.cs
using System;
using System.Threading.Tasks;
using ShareLibrary;
using ShareLibrary.Event;

namespace FeedbackService.Infrastructure.Handlers
{
    /// Nghe CodeSubmittedEvents -> chấm điểm -> Publish FeedbackGeneratedEvent
    public class GenerateFeedbackHandler : IEventHandler<CodeSubmittedEvents>
    {
        private readonly IEventBus _eventBus;

        public GenerateFeedbackHandler(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public async Task Handle(CodeSubmittedEvents e)
        {
            Console.WriteLine("==========================================");
            Console.WriteLine("[FeedbackService] Received CodeSubmittedEvents");
            Console.WriteLine($"AssignmentId : {e.AssignmentId}");
            Console.WriteLine($"Status       : {e.Status}");
            Console.WriteLine($"Output       : {e.Output}");
            Console.WriteLine($"ExecTime     : {e.ExecutionTime}s | Mem: {e.MemoryUsed}KB");
            Console.WriteLine("==========================================");

            var feedbackText =
                $"Output: {e.Output}\n" +
                $"Status: {e.Status}\n" +
                $"ExecutionTime: {e.ExecutionTime}s\n" +
                $"MemoryUsed: {e.MemoryUsed}KB";

            double score = e.Status == "Accepted" ? 10.0 : 0.0;

            var feedbackEvent = new FeedbackGeneratedEvent
            {
                SubmissionId = Guid.NewGuid(),
                Score = score,
                ResultStatus = e.Status,
                Feedback = feedbackText,
            };
            Console.WriteLine("[FeedbackService] >>> About to publish FeedbackGeneratedEvent");
            // Lưu ý: dùng đúng overload Publish(event)
            _eventBus.Publish(feedbackEvent);
            Console.WriteLine("[FeedbackService] Published FeedbackGeneratedEvent");

            await Task.CompletedTask;
        }
    }
}
