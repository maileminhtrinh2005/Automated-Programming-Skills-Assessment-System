using SubmissionService.Application.DTOs;
using SubmissionService.Application.Interface;
using System.Text.Json;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SubmissionService.Infrastructure
{
    public class SendToJudge0 : ISendToJudge0
    {
        private readonly HttpClient _httpClient;
        public SendToJudge0(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ResultDTO?> RunCode(string sourceCode, int languageId, string stdin, string urlJudge0)
        {
            if (sourceCode.IsNullOrEmpty() || languageId<0 || urlJudge0.IsNullOrEmpty()) return null;

            var body = new
            {
                source_code = Convert.ToBase64String(Encoding.UTF8.GetBytes(sourceCode)),
                language_id = languageId,
                stdin= Convert.ToBase64String(Encoding.UTF8.GetBytes(stdin))
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            // ✅ gửi base64 nên query phải có base64_encoded=true
            var response = await _httpClient.PostAsync($"{urlJudge0}/submissions?base64_encoded=true&wait=true", content);
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Judge0 raw response: " + result);

            var json = JsonDocument.Parse(result);
            string token = json.RootElement.GetProperty("token").GetString()??"";

            // ✅ lấy kết quả cũng phải để base64_encoded=true
            var ketquaResponse = await _httpClient.GetAsync($"{urlJudge0}/submissions/{token}?base64_encoded=true");
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

                // 
                string DecodeBase64(string? encoded)
                {
                    if (string.IsNullOrEmpty(encoded)) return "";
                    try
                    {
                        var bytes = Convert.FromBase64String(encoded);
                        return Encoding.UTF8.GetString(bytes);
                    }
                    catch
                    {
                        return encoded;
                    }
                }

                string stdout = DecodeBase64(stdoutProp.GetString());
                string stderr = DecodeBase64(stderrProp.GetString());
                string compileOutput = DecodeBase64(compileProp.GetString());

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
                    ErrorMessage = string.IsNullOrEmpty(stderr) ? compileOutput : stderr
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi parse JSON hoặc mapping: " + ex.Message);
                return null;
            }
        }


    }
}
