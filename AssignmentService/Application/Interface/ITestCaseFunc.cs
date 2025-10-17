using AssignmentService.Application.DTO;


namespace AssignmentService.Application.Interface
{
    public interface ITestCaseFunc
    {
        public Task<bool> SaveTestCase(AssignmentRequest request);

    }
}
