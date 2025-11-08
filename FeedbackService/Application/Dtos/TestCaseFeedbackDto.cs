namespace FeedbackService.Application.Dtos
{
    public class TestCaseFeedbackDto
    {
        // 🧩 Tên của testcase, ví dụ "Case 1", "Case 2", ...
        public string? Name { get; set; }

        // 🧠 Nhận xét chi tiết (AI hoặc người chấm sinh ra)
        public string? Comment { get; set; }

        // 🧮 Trạng thái (Passed / Failed / Chưa chạy)
        public string? Status { get; set; }

        // 📥 Input gốc của test case
        public string? Input { get; set; }

        // 📤 Kết quả mong đợi
        public string? ExpectedOutput { get; set; }

        // 📊 Kết quả thực tế (nếu có)
        public string? ActualOutput { get; set; }

        // ⚙️ Thời gian thực thi (tùy chọn)
        public double? ExecutionTime { get; set; }

        // 💾 Bộ nhớ sử dụng (tùy chọn)
        public double? MemoryUsed { get; set; }

        // ⚠️ Thông báo lỗi (nếu có)
        public string? ErrorMessage { get; set; }
    }
}
