using AssignmentService.Application.DTO;
using AssignmentService.Application.Interface;
using AssignmentService.Domain;
using ShareLibrary;
using ShareLibrary.Event;


namespace AssignmentService.Infrastructure
{
    public class TestCaseFunc : ITestCaseFunc
    {
        private readonly ICrudAssignment _assignment;
        private readonly AssignmentDbContext _context;
        private readonly IEventBus _eventBus;
        public TestCaseFunc(ICrudAssignment assignment, AssignmentDbContext context,IEventBus eventBus)
        {
            _assignment = assignment;
            _context = context;
            _eventBus = eventBus;
        }

        public async Task<bool> SaveTestCase(AssignmentRequest request)
        {
            if (request.sourceCode == null || request.languageId <0) return false;

            var assignment = await _assignment.AddAssignment(request);
            if (assignment == null) return false;
            
            TestCase testcases = new TestCase
            {
                AssignmentId=assignment.AssignmentId,
                Input=request.sourceCode,
                Status= "pending",
                ExpectedOutput= " ",
                MemoryUsed = 0,
                ExecutionTime = 0,
                ErrorMessage=" ",
                Weight=request.weight,
            };
            _context.testCases.Add(testcases);
            await _context.SaveChangesAsync();

            PublishCode(testcases.AssignmentId,request.sourceCode,request.languageId);

            return true;
        }

        private void PublishCode(int assignmentId,string sourceCode, int languageId)
        {
            Console.WriteLine(assignmentId);
            var eventMess = new CodeSubmittedEvents
            {
                AssignmentId=assignmentId,
                SourceCode = sourceCode,
                LanguageId = languageId
            };

            _eventBus.Publish(eventMess);

            Console.WriteLine("check");
        }
    }
}
