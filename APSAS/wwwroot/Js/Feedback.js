console.log("✅ Feedback.js loaded successfully!");
const GATEWAY = "http://localhost:5261";
const $ = (id) => document.getElementById(id);
const out = console.log;

// ======== TOKEN & API FETCH ========
function getToken() {
    return localStorage.getItem("token");
}

async function apiFetch(path, options = {}) {
    const token = getToken();
    const headers = {
        "Content-Type": "application/json",
        ...(token ? { "Authorization": `Bearer ${token}` } : {})
    };

    const res = await fetch(`${GATEWAY}${path}`, { ...options, headers });
    if (res.status === 401) {
        alert("⏰ Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại!");
        localStorage.clear();
        window.location.href = "DN.html";
    }
    return res;
}

// ======== LOAD TRANG ========
window.addEventListener("DOMContentLoaded", async () => {
    let studentId = localStorage.getItem("selectedStudentId") || localStorage.getItem("studentId");

    if (!studentId) {
        alert("⚠️ Không tìm thấy Student ID, hãy quay lại Dashboard!");
        window.location.href = "DashboardLecturer.html";
        return;
    }

    console.log("📦 Student ID:", studentId);
    await loadSubmissionsOnly(studentId);
});

// ======== FETCH DATA ========
async function fetchSubmissionsByStudent(studentId) {
    const res = await apiFetch(`/GetYourSubmission/${studentId}`);
    if (!res.ok) throw new Error("Không lấy được submissions");
    return res.json();
}

async function fetchAssignmentById(id) {
    const res = await apiFetch(`/GetAssignmentByid/${id}`);
    if (!res.ok) return null;
    return res.json();
}

async function fetchResultBySubmission(submissionId) {
    const res = await apiFetch(`/GetYourResult/${submissionId}`);
    if (!res.ok) return null;
    return res.json();
}

// ======== RENDER DANH SÁCH ========
function renderSubmissions(subs) {
    const tbody = $("tblSubmissions").querySelector("tbody");
    tbody.innerHTML = "";

    if (!subs || subs.length === 0) {
        tbody.innerHTML = `<tr><td colspan="6" style="text-align:center;">❌ Không có bài nộp nào</td></tr>`;
        return;
    }

    subs.forEach((s, i) => {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td>${i + 1}</td>
            <td>${s.assignmentTitle || "Không rõ"}</td>
            <td>${s.score ?? "-"}</td>
            <td>${s.status ?? "-"}</td>
            <td>${s.createdAt ?? "-"}</td>
            <td>
                <button class="btnDetail" data-id="${s.submissionId}">
                    🧠 Nhận xét chi tiết
                </button>
            </td>
        `;
        tbody.appendChild(row);
    });

    document.querySelectorAll(".btnDetail").forEach(btn => {
        btn.onclick = async () => {
            const submissionId = btn.dataset.id;
            await generateDetailFeedback(submissionId);
        };
    });
}

// ======== LOAD SUBMISSIONS ========
async function loadSubmissionsOnly(studentId) {
    const submissions = await fetchSubmissionsByStudent(studentId);
    const detailedSubs = [];

    for (const s of submissions) {
        const assignment = await fetchAssignmentById(s.assignmentId);
        detailedSubs.push({
            submissionId: s.submissionId,
            assignmentTitle: assignment?.title || "Không rõ",
            score: s.score ?? "-",
            status: s.status ?? "-",
            createdAt: s.createdAt || s.submittedAt || "-"
        });
    }

    renderSubmissions(detailedSubs);
}

// ======== NHẬN XÉT CHI TIẾT (TỰ LẤY RESULT) ========
async function generateDetailFeedback(submissionId) {
    try {
        out(`🔍 Đang lấy result cho submission ${submissionId}...`);
        const result = await fetchResultBySubmission(submissionId);

        // Kiểm tra dữ liệu hợp lệ
        if (!result || (Array.isArray(result) && result.length === 0))
            return alert("❌ Không tìm thấy dữ liệu test case.");

        // Nếu API trả mảng trực tiếp (chưa có field testResults)
        const testResults = Array.isArray(result)
            ? result
            : (result.testResults || []);

        if (testResults.length === 0)
            return alert("❌ Không có test case nào trong result.");

        // ✅ Chuẩn hóa cấu trúc dữ liệu cho đúng với TestResultDto
        const normalizedResults = testResults.map(tr => ({
            status: tr.status || tr.Status || "Unknown",
            input: tr.input || tr.Input || "",
            expectedOutput: tr.expectedOutput || tr.ExpectedOutput || "",
            actualOutput: tr.actualOutput || tr.ActualOutput || "",
            executionTime: tr.executionTime || tr.ExecutionTime || 0
        }));

        const payload = {
            submissionId,
            testResults: normalizedResults
        };

        out("📤 Gửi sang FeedbackService để sinh nhận xét chi tiết...");
        const res = await apiFetch(`/testcasesubmit`, {
            method: "POST",
            body: JSON.stringify(payload)
        });

        // Bắt lỗi phản hồi
        if (!res.ok) {
            const errText = await res.text();
            throw new Error(errText);
        }

        const data = await res.json();

        // ✅ Hiển thị kết quả nhận xét
        $("feedbackCard").style.display = "block";
        $("summaryText").textContent = data.summary || "(Không có nhận xét)";
        $("progressText").textContent = data.overallProgress || "(Không có)";
        $("manualFeedback").value = data.summary || "";

        // Hiển thị màu trạng thái
        const prog = $("progressText");
        prog.className = "";
        const p = (data.overallProgress || "").toLowerCase();
        if (p.includes("tốt") || p.includes("good")) prog.classList.add("progress-good");
        else if (p.includes("cải thiện") || p.includes("medium")) prog.classList.add("progress-medium");
        else prog.classList.add("progress-bad");

        out("✅ Nhận xét chi tiết:", data);

    } catch (err) {
        alert("❌ Lỗi khi sinh nhận xét chi tiết: " + err.message);
        console.error(err);
    }
}
// ======== NHẬN XÉT TỔNG QUÁT ========
async function generateProgressFeedback(studentId) {
    try {
        const submissions = await fetchSubmissionsByStudent(studentId);
        if (!Array.isArray(submissions) || submissions.length === 0)
            return alert("❌ Không có submission nào.");

        const detailedSubs = [];
        for (const s of submissions) {
            const assignment = await fetchAssignmentById(s.assignmentId);
            const result = await fetchResultBySubmission(s.submissionId);
            detailedSubs.push({
                submissionId: s.submissionId,
                assignmentId: s.assignmentId,
                assignmentTitle: assignment?.title || "Không rõ",
                score: s.score ?? result?.score ?? 0,
                status: s.status ?? result?.status ?? "Unknown",
                sourceCode: s.sourceCode ?? "N/A",
                createdAt: s.createdAt || s.submittedAt || new Date().toISOString(),
                testResults: result?.testResults ?? []
            });
        }

        const payload = { studentId, submissions: detailedSubs };
        const res = await apiFetch(`/feedback/generate/bulk`, {
            method: "POST",
            body: JSON.stringify(payload)
        });

        if (!res.ok) throw new Error(await res.text());
        const data = await res.json();

        $("feedbackCard").style.display = "block";
        $("summaryText").textContent = data.summary || "(Không có nhận xét)";
        $("progressText").textContent = data.overallProgress || "(Không có)";
        $("manualFeedback").value = data.summary || "";

        const prog = $("progressText");
        prog.className = "";
        const p = (data.overallProgress || "").toLowerCase();
        if (p.includes("tốt") || p.includes("good")) prog.classList.add("progress-good");
        else if (p.includes("cải thiện") || p.includes("medium")) prog.classList.add("progress-medium");
        else prog.classList.add("progress-bad");

    } catch (err) {
        alert("❌ Lỗi khi sinh nhận xét: " + err.message);
        console.error(err);
    }
}

// ======== GỬI NHẬN XÉT GIẢNG VIÊN ========
$("btnSendReviewed").addEventListener("click", async () => {
    const studentId = localStorage.getItem("selectedStudentId") || localStorage.getItem("studentId");
    const feedbackText = $("manualFeedback").value.trim();
    if (!feedbackText) return alert("⚠️ Nhập nội dung trước khi gửi!");

    const payload = { studentId, feedbackText, comment: "Giảng viên đã gửi lại nhận xét." };
    const res = await apiFetch(`/manual/sendreviewed`, {
        method: "POST",
        body: JSON.stringify(payload)
    });

    if (res.ok) alert("✅ Đã gửi nhận xét thành công!");
    else alert("❌ Gửi thất bại: " + (await res.text()));
});

// ======== NÚT ========
$("btnClear").onclick = () => location.reload();
$("btnClear2").onclick = () => $("manualFeedback").value = "";
$("btnGenerate").onclick = () => {
    const studentId = localStorage.getItem("selectedStudentId") || localStorage.getItem("studentId");
    if (!studentId) return alert("⚠️ Không có studentId!");
    generateProgressFeedback(studentId);
};
