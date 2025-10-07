using AssignmentService.Application.DTO;
using AssignmentService.Application.Interface;
using AssignmentService.Domain;
using System.Text;
using System.Text.Json;

namespace AssignmentService.Infrastructure
{
    public class TestCaseFunc : ITestCaseFunc
    {
        private readonly HttpClient _httpClient;
        private readonly ICrudAssignment _assignment;
        private readonly AssignmentDbContext _context;
        public TestCaseFunc(HttpClient httpClient, ICrudAssignment assignment, AssignmentDbContext context)
        {
            _httpClient = httpClient;
            _assignment = assignment;
            _context = context;
        }

        public async Task<TestCaseDTO> CallJudgeZero(string sourcecode, int languageId, string submissionUrl)
        {

            var body =new  
            {
                SourceCode= sourcecode,
                LanguageId =languageId,
            };


            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var httpResponse = await _httpClient.PostAsync($"{submissionUrl}/api/Submission/SubmitAndShow", content);


            if (!httpResponse.IsSuccessStatusCode) return null;

            var result= await httpResponse.Content.ReadAsStringAsync();

            var jsonDoc = JsonDocument.Parse(result);
            var root = jsonDoc.RootElement;

            root.TryGetProperty("output", out var expectedoutput);
            root.TryGetProperty("executionTime", out var executiontime);
            root.TryGetProperty("memoryUsed", out var memoryused);
            root.TryGetProperty("status", out var status);
            root.TryGetProperty("errorMessage", out var errormessage);


            double time = 0;
            if (executiontime.ValueKind == JsonValueKind.String)
                double.TryParse(executiontime.GetString(), out time);
            else if (executiontime.ValueKind == JsonValueKind.Number)
                time = executiontime.GetDouble();

            int memory = 0;
            if (memoryused.ValueKind == JsonValueKind.Number)
                memory = memoryused.GetInt32();

            string output = expectedoutput.GetString() ?? "";
            string Status =status.GetString()??"";
            string errorMessage = errormessage.GetString() ?? "";

            TestCaseDTO testcase = new TestCaseDTO
            {
                ExpectedOutput= output,
                Status=Status,
                MemoryUsed=memory,
                ExecutionTime=time,
                ErrorMessage=errorMessage
            };

            return testcase;
        }

        public async Task<bool> SaveTestCase(AssignmentRequest request, string url)
        {
            if (request.sourceCode == null || request.languageId <0) return false;

            var assignment = await _assignment.AddAssignment(request);
            if (assignment == null) return false;
            var testcase= await CallJudgeZero(request.sourceCode, request.languageId,url);



            TestCase testcases = new TestCase
            {
                AssignmentId=assignment.AssignmentId,
                Input=request.sourceCode,
                Status= testcase.Status,
                ExpectedOutput= testcase.ExpectedOutput,
                MemoryUsed = testcase.MemoryUsed,
                ExecutionTime = testcase.ExecutionTime,
                ErrorMessage=testcase.ErrorMessage,
                Weight=request.weight,
            };
            _context.testCases.Add(testcases);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
