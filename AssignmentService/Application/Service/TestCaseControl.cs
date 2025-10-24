using AssignmentService.Application.Interface;

namespace AssignmentService.Application.Service
{
    public class TestCaseControl
    {
        private string _submissionUrl = "http://localhost:5090";
        private readonly ITestCaseFunc _testcase;
        public TestCaseControl(ITestCaseFunc testcase)
        {
            _testcase = testcase;
        }


        public async Task<bool> AddTestCase(int assignmentId, string input, string expectedOutput, double weight)
        {
            if (assignmentId<0|| input ==null || expectedOutput ==null || weight < 0)
            {
                return false;
            }
            if (!  await _testcase.AddTestCase(assignmentId, input, expectedOutput, weight)) return false;
            return true;
        }
    }
}
