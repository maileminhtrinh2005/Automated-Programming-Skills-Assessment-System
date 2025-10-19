using AssignmentService.Application.DTO;
using Microsoft.EntityFrameworkCore;
using ShareLibrary;
using ShareLibrary.Event;

namespace AssignmentService.Infrastructure
{
    public class TestCaseHandle:IEventHandler <CodeSubmittedEvents>
    {
        private readonly AssignmentDbContext _context;
        public TestCaseHandle(AssignmentDbContext context)
        {
            _context = context;
        }

        public async Task Handle(CodeSubmittedEvents @event)
        {
            //var testcase = new TestCaseDTO
            //{
            //    AssignmentId = @event.AssignmentId,
            //    ExpectedOutput = @event.Output,
            //    Status= @event.Status,
            //    ExecutionTime= @event.ExecutionTime,
            //    MemoryUsed= @event.MemoryUsed,
            //    ErrorMessage= @event.ErrorMessage
            //};

            Console.WriteLine(@event.AssignmentId+"   checkkkkkk");
            var updatetestcase= await _context.testCases.FirstOrDefaultAsync(t => t.AssignmentId == @event.AssignmentId);
            if (updatetestcase == null) throw new Exception();
            updatetestcase.ExpectedOutput = @event.Output;
            updatetestcase.Status = @event.Status;
            updatetestcase.ExecutionTime = @event.ExecutionTime;
            updatetestcase.MemoryUsed = @event.MemoryUsed;
            updatetestcase.ErrorMessage = @event.ErrorMessage;
            await _context.SaveChangesAsync();
            Console.WriteLine("akjdhjksahdjkad");
            //return await Task.CompletedTask;
        }
    }
}
