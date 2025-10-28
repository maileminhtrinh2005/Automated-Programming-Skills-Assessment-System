﻿using System;
using System.Threading.Tasks;
using ShareLibrary;
using ShareLibrary.Event;

namespace FeedbackService.Application.Services
{
    /// <summary>
    /// Service chịu trách nhiệm push dữ liệu feedback ra RabbitMQ
    /// để NotificationService nhận và hiển thị qua SignalR.
    /// </summary>
    public class FeedbackPushService
    {
        private readonly IEventBus _eventBus;

        public FeedbackPushService(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        /// <summary>
        /// Gửi event feedback ra RabbitMQ (title + summary message).
        /// </summary>
        public async Task PushFeedbackAsync(
            int studentId,
            int submissionId,
            string assignmentTitle,
            string summary,
            double score = 0)
        {
            var evt = new FeedbackGeneratedEvent
            {
                StudentId = studentId,
                SubmissionId = submissionId,
                Score = score,
                Title = $"Feedback cho bài '{assignmentTitle}'",
                Message = summary ?? "(Không có nội dung phản hồi)",
                CreatedAtUtc = DateTime.UtcNow
            };

            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine("[FeedbackPushService] 📤 Publish FeedbackGeneratedEvent");
            Console.WriteLine($"Title   : {evt.Title}");
            Console.WriteLine($"Message : {evt.Message}");
            Console.WriteLine($"Student : {evt.StudentId}");
            Console.WriteLine($"Submission: {evt.SubmissionId}");
            Console.WriteLine("----------------------------------------------------");

            _eventBus.Publish(evt);

            Console.WriteLine("[FeedbackPushService] ✅ Đã push event sang NotificationService.");
            await Task.CompletedTask;
        }
    }
}
