using AssignmentService.Application.DTO;
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


        public  async Task<List<TestCaseDTO>?> GetTestCases (int assignmentId)
        {
            var testcases = await _testcaseRepo.GetTestCases(assignmentId);
            if (testcases == null ) return null;    
            return testcases;
        }

        public async Task<bool> DeleteTestcase (int id)
        {
            if(id<0) return false;
            bool isSuccess = await _testcaseRepo.DeleteTestCase(id);
            if(!isSuccess) return false;
            return true;
        }

        public async Task<bool> UpdateTestcase(TestCaseDTO tc)
        {
            if (tc==null) return false;
            bool isSuccess = await _testcaseRepo.UpdateTestCase(tc.TestCaseId,tc.Input??string.Empty,tc.ExpectedOutput??string.Empty,tc.Weight);
            if (!isSuccess) return false;
            return true;
        }
    }
}
