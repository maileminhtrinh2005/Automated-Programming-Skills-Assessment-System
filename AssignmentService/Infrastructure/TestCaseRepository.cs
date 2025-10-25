using AssignmentService.Application.DTO;
using AssignmentService.Application.Interface;
using AssignmentService.Domain;
using Microsoft.EntityFrameworkCore;



namespace AssignmentService.Infrastructure
{
    public class TestCaseRepository : ITestCaseRepository
    {
        private readonly AssignmentDbContext _context;
        public TestCaseRepository( AssignmentDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddTestCase(int assingmentId, string input, string expectedOutput, double weight)
        {
            if (assingmentId<0 || input == null || expectedOutput == null)
            {
                return false;
            }

            var testcase = new TestCase
            {
                AssignmentId = assingmentId,
                Input = input,
                ExpectedOutput = expectedOutput,
                Weight = weight,
                Status = " ",
                ExecutionTime = 0,
                MemoryUsed=0,
                ErrorMessage=" "
            };
            _context.testCases.Add(testcase);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<TestCaseDTO>?> GetTestCases(int assignmentId)
        {
            var testcases = await _context.testCases.
                Where(tc => tc.AssignmentId == assignmentId).
                Select(tc => new TestCaseDTO
                {
                    TestCaseId = tc.TestCaseId,
                    Input = tc.Input,
                    ExpectedOutput = tc.ExpectedOutput,
                    Weight = tc.Weight
                }).ToListAsync();
            if  (testcases==null) {  return null; }
            return testcases;
        }

    }
}
