// FeedbackService.Infrastructure/Handlers/GenerateFeedbackHandler.cs
using System;
using System.Threading.Tasks;
using ShareLibrary;
using ShareLibrary.Event;

namespace FeedbackService.Infrastructure.Handlers
{
    /// <summary>
    /// Lắng nghe CodeSubmittedEvents -> tính điểm/tạo nội dung -> publish FeedbackGeneratedEvent
    /// </summary>
    public class GenerateFeedbackHandler : IEventHandler<FeedbackGeneratedEvent>
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

            // Tạo feedback text (ví dụ)
            var feedbackText =
                $"Output: {e.Output}\n" +
                $"Status: {e.Status}\n" +
                $"ExecutionTime: {e.ExecutionTime}s\n" +
                $"MemoryUsed: {e.MemoryUsed}KB";

            // Chấm điểm đơn giản
            var score = e.Status == "Accepted" ? 10.0 : 0.0;


            var feedbackEvent = new FeedbackGeneratedEvent
            {
                SubmissionId = Guid.NewGuid(),
                
                Score = score,
                ResultStatus = e.Status,
                Feedback = feedbackText,

            };

            _eventBus.Publish(feedbackEvent);
            Console.WriteLine("[FeedbackService] Published FeedbackGeneratedEvent");

            await Task.CompletedTask;
        }

        public Task Handle(FeedbackGeneratedEvent @event)
        {
            throw new NotImplementedException();
        }
    }
}