using System;
using System.Linq;
using System.Threading.Tasks;
using ShareLibrary;
using ShareLibrary.Event;

namespace FeedbackService.Infrastructure.Handlers
{
    /// <summary>
    /// Lắng nghe TestCaseFetchEvent -> log thông tin test case -> Publish FeedbackGeneratedEvent (đơn giản)
    /// </summary>
    public class GenerateFeedbackHandler : IEventHandler<TestCaseFetchEvent>
    {
        private readonly IEventBus _eventBus;

        public GenerateFeedbackHandler(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public async Task Handle(TestCaseFetchEvent e)
        {
            Console.WriteLine("==========================================");
            Console.WriteLine("[FeedbackService] Received TestCaseFetchEvent");
            Console.WriteLine($"AssignmentId : {e.AssignmentId}");
            Console.WriteLine($"LanguageId   : {e.LanguageId}");
            Console.WriteLine($"SubmissionId : {e.SubmissionId}");
            Console.WriteLine($"#TestCases   : {e.TestCaseList?.Count ?? 0}");
            Console.WriteLine("---- Test cases preview ----");

            if (e.TestCaseList != null && e.TestCaseList.Count > 0)
            {
                int idx = 1;
                foreach (var tc in e.TestCaseList)
                {
                    Console.WriteLine($"  [{idx++}] Id={tc.TestCaseId}, Weight={tc.Weight}");
                    Console.WriteLine($"       Input   : {tc.Input}");
                    Console.WriteLine($"       Expected: {tc.ExpectedOutput}");
                }
            }
            else
            {
                Console.WriteLine("  (no test cases)");
            }
            Console.WriteLine("==========================================");

            //  Tạo text feedback tóm tắt test case
            var feedbackText =
                $"Assignment: {e.AssignmentId}\n" +
                $"Submission: {e.SubmissionId}\n" +
                $"LanguageId: {e.LanguageId}\n" +
                $"TestCases : {e.TestCaseList?.Count ?? 0}\n" +
                string.Join("\n", (e.TestCaseList ?? []).Select((tc, i) =>
                    $"- #{i + 1}: Id={tc.TestCaseId}, Weight={tc.Weight}, Expected={tc.ExpectedOutput}"));

            // 🔹 Tạo event để gửi qua RabbitMQ 
            var feedbackEvent = new FeedbackGeneratedEvent
            {
                SubmissionId = e.SubmissionId,
                StudentId = 0,
                Score = 0,
                Title = $"Tự động sinh feedback cho Assignment #{e.AssignmentId}",
                Message = feedbackText,
                CreatedAtUtc = DateTime.UtcNow
            };

            Console.WriteLine("[FeedbackService] >>> Publishing FeedbackGeneratedEvent");
            _eventBus.Publish(feedbackEvent);
            Console.WriteLine("[FeedbackService] ✅ Published FeedbackGeneratedEvent");

            await Task.CompletedTask;
        }
    }
}
