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

        public async Task<Result> Submited(Request request, string urlJudge0)
        {
            if (request == null) return null;

            var body = new
            {
                source_code = request.SourceCode,
                language_id = request.LanguageId
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{urlJudge0}?base64_encoded=false&wait=true", content);
            if (!response.IsSuccessStatusCode) return null;


            var result = await response.Content.ReadAsStringAsync();

            //Console.WriteLine("Judge0 raw response: " + result);
            var json = JsonDocument.Parse(result);
            string token = json.RootElement.GetProperty("token").GetString();

            var ketquaResponse = await _httpClient.GetAsync($"{urlJudge0}/{token}?base64_encoded=false");
            if (!ketquaResponse.IsSuccessStatusCode) return null;

            var ketqua = await ketquaResponse.Content.ReadAsStringAsync();
            Console.WriteLine(ketqua);


            var jsonDoc = JsonDocument.Parse(ketqua);
            var root = jsonDoc.RootElement;

            //Console.WriteLine("checkkkkk11123123213",root.ToString());
            var stdout = root.GetProperty("stdout").GetString() ?? "";
            Console.WriteLine("checkkkkk", stdout);
            var status = root.GetProperty("status");
            var statusId = status.GetProperty("id").GetInt32();
            var statusDescription = status.GetProperty("description").GetString() ?? "";

            if (statusDescription == "Accepted")
            {
                Result result_ = new Result
                {
                    Output = stdout ?? "",
                    Status = statusDescription ?? ""
                };
                //Console.WriteLine("checkkkkkk",result_.Output);
                return result_;
            }
            else
            {
                Result result_ = new Result
                {
                    Output = stdout ?? "",
                    Status = statusDescription ?? ""
                };

                //Console.WriteLine("akjhsdajksdjkasd", result_.Output);
                return result_;
            }

        }

    }
}
