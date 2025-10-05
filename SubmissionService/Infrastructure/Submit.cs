using SubmissionService.Application.DTOs;
using SubmissionService.Application.Interface;
using System.Text.Json;
using System.Text;

namespace SubmissionService.Infrastructure
{
    public class Submit : ISubmit
    {
        private readonly HttpClient _httpClient;
        public Submit(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ResultDTO> Submited(Request request, string urlJudge0)
        {
            if (request == null) return null;

            var body = new
            {
                source_code = request.SourceCode,
                language_id = request.LanguageId
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{urlJudge0}/submissions?base64_encoded=false&wait=true", content);
            if (!response.IsSuccessStatusCode) return null;


            var result = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Judge0 raw response: " + result);
            var json = JsonDocument.Parse(result);
            string token = json.RootElement.GetProperty("token").GetString();

            var ketquaResponse = await _httpClient.GetAsync($"{urlJudge0}/submissions/{token}?base64_encoded=false");
            if (!ketquaResponse.IsSuccessStatusCode) return null;

            var ketqua = await ketquaResponse.Content.ReadAsStringAsync();
            Console.WriteLine(ketqua);


            try
            {
                var jsonDoc = JsonDocument.Parse(ketqua);
                var root = jsonDoc.RootElement;

                root.TryGetProperty("stdout", out var stdoutProp);
                root.TryGetProperty("stderr", out var stderrProp);
                root.TryGetProperty("compile_output", out var compileProp);
                root.TryGetProperty("time", out var timeProp);
                root.TryGetProperty("memory", out var memProp);
                root.TryGetProperty("status", out var statusProp);

                string stdout = stdoutProp.GetString() ?? "";
                string stderr = stderrProp.GetString() ?? "";
                string compileOutput = compileProp.GetString() ?? "";

                double time = 0;
                if (timeProp.ValueKind == JsonValueKind.String)
                    double.TryParse(timeProp.GetString(), out time);
                else if (timeProp.ValueKind == JsonValueKind.Number)
                    time = timeProp.GetDouble();

                int memory = 0;
                if (memProp.ValueKind == JsonValueKind.Number)
                    memory = memProp.GetInt32();

                string statusDescription = statusProp.GetProperty("description").GetString() ?? "";

                Console.WriteLine($"stdout={stdout}, stderr={stderr}, compileOutput={compileOutput}, time={time}, mem={memory}");

                return new ResultDTO
                {
                    Output = stdout,
                    Status = statusDescription,
                    ExecutionTime = time,
                    MemoryUsed = memory,
                    ErrorMessage = stderr ?? compileOutput ?? "none"
                };
            }
            catch (Exception ex)
            { 
                Console.WriteLine("⚠️ Lỗi parse JSON hoặc mapping: " + ex.Message);
                return null;
            }
        }

    }
}
