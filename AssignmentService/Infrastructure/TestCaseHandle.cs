using AssignmentService.Application.DTO;
using Microsoft.EntityFrameworkCore;
using ShareLibrary;
using ShareLibrary.Event;

namespace AssignmentService.Infrastructure
{
    public class TestCaseHandle:IEventHandler <CodeSubmittedEvents>
    {
        private readonly AssignmentDbContext _context;
        private readonly IEventBus _eventBus;
        public TestCaseHandle(AssignmentDbContext context,IEventBus eventBus)
        {
            _context = context;
            _eventBus = eventBus;
        }

        public  Task Handle(CodeSubmittedEvents @event)
        {
            // nhan id asssignment tu submission , lay ra danh sach cac testcase cua assingment do
            var testcaselist = _context.testCases.
                Where(tc => tc.AssignmentId == @event.AssignmentId).
                Select(tc => new TestCaseEvent 
                { 
                    TestCaseId = tc.TestCaseId,
                    Input=tc.Input,
                    ExpectedOutput=tc.ExpectedOutput,
                    Weight=tc.Weight         
                }).
                ToList();

            var response = new TestCaseFetchEvent
            {
                AssignmentId = @event.AssignmentId,
                SourceCode= @event.SourceCode,
                LanguageId= @event.LanguageId,
                SubmissionId=@event.SubmissionId,
                TestCaseList =testcaselist
            };
            Console.WriteLine("da check, da nhan duoc akjdjkasd"+ @event.SubmissionId);

            _eventBus.Publish(response);
            return Task.CompletedTask;
        }
    }
}
