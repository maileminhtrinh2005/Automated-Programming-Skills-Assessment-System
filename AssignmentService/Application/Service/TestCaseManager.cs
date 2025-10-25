using AssignmentService.Application.Interface;

namespace AssignmentService.Application.Service
{
    public class TestCaseManager
    {
        private readonly ITestCaseRepository _testcaseRepo;
        public TestCaseManager(ITestCaseRepository testcase)
        {
            _testcaseRepo = testcase;
        }


        public async Task<bool> AddTestCase(int assignmentId, string input, string expectedOutput, double weight)
        {
            if (assignmentId<0|| input ==null || expectedOutput ==null || weight < 0)
            {
                return false;
            }
            if (!  await _testcaseRepo.AddTestCase(assignmentId, input, expectedOutput, weight)) return false;
            return true;
        }
    }
}
