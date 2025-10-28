console.log("✅ Feedback.js loaded successfully!");
const GATEWAY = "http://localhost:5261";
const $ = (id) => document.getElementById(id);
const out = (msg) => $("out").textContent = msg;

// ========== FETCH FUNCTIONS ==========
async function fetchSubmissionsByStudent(studentId) {
    const res = await fetch(`${GATEWAY}/GetYourSubmission/${studentId}`);
    if (!res.ok) throw new Error("Không lấy được submissions");
    return res.json();
}

async function fetchAssignmentById(id) {
    const res = await fetch(`${GATEWAY}/GetAssignmentByid/${id}`);
    if (!res.ok) return null;
    return res.json();
}

async function fetchResultBySubmission(submissionId) {
    const res = await fetch(`${GATEWAY}/GetYourResult/${submissionId}`);
    if (!res.ok) return null;
    return res.json();
}

// ========== RENDER SUBMISSION LIST ==========
function renderSubmissions(subs) {
    const tbody = $("tblSubmissions").querySelector("tbody");
    tbody.innerHTML = "";
    subs.forEach((s, i) => {
        const row = document.createElement("tr");
        row.innerHTML = `
      <td>${i + 1}</td>
      <td>${s.assignmentTitle || "Không rõ"}</td>
      <td>${s.score ?? "-"}</td>
      <td>${s.status ?? "-"}</td>
      <td>${s.createdAt ?? "-"}</td>
      <td><button class="btnDetail" data-id="${s.submissionId}">🔍 Xem chi tiết</button></td>
    `;
        tbody.appendChild(row);
    });

    // Gán sự kiện nút xem chi tiết
    document.querySelectorAll(".btnDetail").forEach(btn => {
        btn.onclick = () => generateDetailFeedback(btn.dataset.id);
    });
}

// ========== NHẬN XÉT TỔNG QUÁT ==========
async function generateProgressFeedback() {
    try {
        const studentId = $("qStudentId").value.trim();
        if (!studentId) return alert("⚠️ Nhập Student ID trước");

        out("⏳ Đang lấy dữ liệu submissions...");
        const submissions = await fetchSubmissionsByStudent(studentId);
        if (!Array.isArray(submissions) || submissions.length === 0)
            return out("❌ Không có submission nào.");

        const detailedSubs = [];
        for (const s of submissions) {
            const assignment = await fetchAssignmentById(s.assignmentId);
            const result = await fetchResultBySubmission(s.submissionId);

            detailedSubs.push({
                submissionId: s.submissionId,
                assignmentId: s.assignmentId,
                assignmentTitle: assignment?.title || "Không rõ",
                createdAt: s.createdAt || s.submittedAt || "-",
                score: s.score ?? result?.score ?? 0,
                status: s.status ?? result?.status ?? "Unknown",
                sourceCode: s.sourceCode ?? "",
                testResults: result?.testResults ?? []
            });
        }

        renderSubmissions(detailedSubs);

        const payload = { studentId, submissions: detailedSubs };

        out("📤 Gửi dữ liệu sang FeedbackService để nhận xét tổng quát...");
        const res = await fetch(`${GATEWAY}/feedback/generate/bulk`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });

        if (!res.ok) throw new Error(await res.text());
        const data = await res.json();

        $("aiSummary").value = data.summary || "(Không có)";
        $("aiScore").value = data.overallProgress || "";
        out(JSON.stringify(data, null, 2));
        $("manualFeedback").value = data.summary || "";
    } catch (err) {
        console.error(err);
        out("❌ " + err.message);
    }
}

// ========== NHẬN XÉT CHI TIẾT ==========
async function generateDetailFeedback(submissionId) {
    try {
        out("⏳ Đang lấy chi tiết submission " + submissionId + "...");

        // 1️⃣ Gọi API lấy result
        const result = await fetchResultBySubmission(submissionId);
        if (!result) throw new Error("Không lấy được result");

        // 2️⃣ Chuẩn hoá danh sách testResults (API có thể trả mảng hoặc object)
        const testResultsRaw = Array.isArray(result) ? result : (result.testResults || []);

        if (!testResultsRaw.length)
            throw new Error("Submission này chưa có test case result nào.");

        // 3️⃣ Map dữ liệu theo đúng schema backend cần (bổ sung Status nếu thiếu)
        const testResults = testResultsRaw.map((r, i) => ({
            Name: r.name || `Case ${i + 1}`,
            Status: r.status || (r.passed ? "Passed" : "Failed"),  // ✅ fix lỗi thiếu Status
            Input: r.input || "",
            ExpectedOutput: r.expectedOutput || "",
            Output: r.output || "",
            ExecutionTime: r.executionTime || 0,
            MemoryUsed: r.memoryUsed || 0,
            ErrorMessage: r.errorMessage || "",
            Weight: r.weight || 1.0
        }));

        // 4️⃣ Payload gửi sang FeedbackService
        const payload = {
            SubmissionId: submissionId,   // ✅ đúng chữ hoa
            TestResults: testResults      // ✅ đúng chữ hoa
        };

        out("📤 Gửi sang FeedbackService (chi tiết từng testcase)...");

        // 5️⃣ Gửi request đến Gateway
        const res = await fetch(`${GATEWAY}/feedbacktestcase`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });

        if (!res.ok) throw new Error(await res.text());
        const data = await res.json();

        // 6️⃣ Hiển thị kết quả tổng quát
        $("aiSummary").value = data.summary || "(Không có)";
        $("aiScore").value = data.score ?? "";
        out(JSON.stringify(data, null, 2));
        $("manualFeedback").value = data.summary || ""; // them nhan xet vao o nhap tay
        // 7️⃣ Hiển thị chi tiết từng test case
        const tb = $("tblDetails").querySelector("tbody");
        tb.innerHTML = "";
        (data.testCaseFeedback || []).forEach((t, i) => {
            tb.innerHTML += `
            <tr>
                <td>${i + 1}</td>
                <td>${t.status || (t.comment?.includes("Pass") ? "Passed" : "Failed")}</td>
                <td>${t.input || "-"}</td>
                <td>${t.expectedOutput || "-"}</td>
                <td>${t.comment || "-"}</td>
            </tr>`;
        });

        // ✅ Thông báo thành công
        out("✅ Đã nhận xét chi tiết cho submission " + submissionId);

    } catch (err) {
        out("❌ " + err.message);
        console.error(err);
    }
}

// ========== EVENT BINDINGS ==========
$("btnProgress").onclick = generateProgressFeedback;
$("btnClear").onclick = () => location.reload();
$("btnHealth").onclick = () => out("✅ Gateway hoạt động tốt tại " + GATEWAY);
// ========== GỬI NHẬN XÉT SAU KHI GIẢNG VIÊN CHỈNH SỬA ==========
document.getElementById("btnSendReviewed")?.addEventListener("click", async () => {
    try {
        const submissionId = parseInt(document.getElementById("qSubmissionId")?.value || 0);
        const studentId = parseInt(document.getElementById("qStudentId")?.value || 0);
        const feedbackText = document.getElementById("manualFeedback")?.value.trim();

        if (!feedbackText) {
            alert("⚠️ Vui lòng nhập nội dung nhận xét trước khi gửi.");
            return;
        }

        const payload = {
            submissionId: submissionId,
            studentId: studentId,
            feedbackText: feedbackText,
            comment: "Giảng viên đã chỉnh sửa và gửi lại nhận xét."
        };

        console.log("📤 Sending reviewed feedback:", payload);

        const res = await fetch(`${GATEWAY}/manual/sendreviewed`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });

        if (res.ok) {
            alert("✅ Đã gửi nhận xét thành công!");
            console.log("✅ Reviewed feedback sent successfully!");
        } else {
            const err = await res.text();
            alert("❌ Gửi thất bại: " + err);
            console.error("❌ Gửi thất bại:", err);
        }
    } catch (err) {
        console.error(err);
        alert("❌ Lỗi trong quá trình gửi nhận xét.");
    }
});

