using ShareLibrary;
using ShareLibrary.Event;
using SubmissionService.Application;
using SubmissionService.Application.DTOs;

namespace SubmissionService.Infrastructure
{
    public class RunCodeHandle : IEventHandler<CodeSubmittedEvents>
    {
        private readonly Sub _sub;
        private readonly IEventBus _event;
        public RunCodeHandle(Sub sub,IEventBus eventBus)
        {
            _sub = sub;
            _event = eventBus;
        }

        public async Task Handle(CodeSubmittedEvents @event)
        {
            var body = new Request
            {
                SourceCode=@event.SourceCode,
                LanguageId=@event.LanguageId
            };
            var result = await _sub.SubmitWithResult(body);

            var response = new CodeSubmittedEvents
            {
                AssignmentId=@event.AssignmentId,
                Output= result.Output,
                Status=result.Status,
                ExecutionTime=result.ExecutionTime,
                MemoryUsed=result.MemoryUsed,
                ErrorMessage=result.ErrorMessage,
            };
            _event.Publish(response);
        }
    }
}
