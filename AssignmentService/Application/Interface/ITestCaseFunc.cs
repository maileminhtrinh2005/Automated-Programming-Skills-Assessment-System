using AssignmentService.Application.DTO;


namespace AssignmentService.Application.Interface
{
    public interface ITestCaseFunc
    {
        public Task<TestCaseDTO>  CallJudgeZero(string sourcecode, int languageId, string submissionUrl);// hien tai la local host, sau build len docker se khac 

        public Task<bool> SaveTestCase(AssignmentRequest request, string url);

    }
}
