using AssignmentService.Application.DTO;


namespace AssignmentService.Application.Interface
{
    public interface ITestCaseFunc
    {
        public Task<bool> SaveTestCase(AssignmentRequest request);


        // input = du lieu dung de cho hoc sinh test
        public Task<bool> AddTestCase(int assingmentId, string input, string expectedOutput,double weight);

    }
}
