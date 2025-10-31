using AssignmentService.Application.DTO;


namespace AssignmentService.Application.Interface
{
    public interface ITestCaseRepository
    {
        public Task<bool> AddTestCase(int assingmentId, string input, string expectedOutput,double weight);
        public Task<List<TestCaseDTO>?> GetTestCases(int assignmentId);

        public Task<bool> DeleteTestCase(int id);
        public Task<bool> UpdateTestCase(int id,string input, string expectedOutput, double weight);
    }
}
