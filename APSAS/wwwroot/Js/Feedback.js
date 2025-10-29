console.log("✅ Feedback.js loaded successfully!");
const GATEWAY = "http://localhost:5261";
const $ = (id) => document.getElementById(id);
const out = (msg) => $("out").textContent = msg;

// ======== TOKEN & API FETCH HELPER ========
function getToken() {
    return localStorage.getItem("token");
}

async function apiFetch(path, options = {}) {
    const token = getToken();
    const headers = {
        "Content-Type": "application/json",
        ...(options.headers || {}),
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

// ======== CHECK ACCESS ========
function checkAccess() {
    const token = localStorage.getItem("token");
    const role = localStorage.getItem("role");
    if (!token) {
        window.location.href = "DN.html";
        return false;
    }
}

// ======== AUTO LOAD WHEN PAGE OPENS ========
window.addEventListener("DOMContentLoaded", () => {
    checkAccess();

    const params = new URLSearchParams(window.location.search);
    const studentId = params.get("studentId") || localStorage.getItem("selectedStudentId");

    if (studentId) {
        console.log("📌 Student ID nhận từ Dashboard:", studentId);
        generateProgressFeedback(studentId);
    } else {
        alert("⚠️ Không tìm thấy Student ID! Hãy quay lại Dashboard.");
    }
});

// ======== FETCH FUNCTIONS ========
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

// ======== RENDER SUBMISSIONS ========
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

    document.querySelectorAll(".btnDetail").forEach(btn => {
        btn.onclick = () => generateDetailFeedback(btn.dataset.id);
    });
}

// ======== GENERATE FEEDBACK (TỔNG QUÁT) ========
async function generateProgressFeedback(studentId) {
    try {
        out(`⏳ Đang lấy dữ liệu submissions cho sinh viên ${studentId}...`);

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
        const res = await apiFetch(`/feedback/generate/bulk`, {
            method: "POST",
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

// ======== GENERATE FEEDBACK (CHI TIẾT TEST CASE) ========
async function generateDetailFeedback(submissionId) {
    try {
        out("⏳ Đang lấy chi tiết submission " + submissionId + "...");

        const result = await fetchResultBySubmission(submissionId);
        if (!result) throw new Error("Không lấy được result");

        const testResultsRaw = Array.isArray(result) ? result : (result.testResults || []);
        if (!testResultsRaw.length)
            throw new Error("Submission này chưa có test case result nào.");

        const testResults = testResultsRaw.map((r, i) => ({
            Name: r.name || `Case ${i + 1}`,
            Status: r.status || (r.passed ? "Passed" : "Failed"),
            Input: r.input || "",
            ExpectedOutput: r.expectedOutput || "",
            Output: r.output || "",
            ExecutionTime: r.executionTime || 0,
            MemoryUsed: r.memoryUsed || 0,
            ErrorMessage: r.errorMessage || "",
            Weight: r.weight || 1.0
        }));

        const payload = {
            SubmissionId: submissionId,
            TestResults: testResults
        };

        out("📤 Gửi sang FeedbackService (chi tiết từng testcase)...");
        const res = await apiFetch(`/feedbacktestcase`, {
            method: "POST",
            body: JSON.stringify(payload)
        });

        if (!res.ok) throw new Error(await res.text());
        const data = await res.json();

        $("aiSummary").value = data.summary || "(Không có)";
        $("aiScore").value = data.score ?? "";
        out(JSON.stringify(data, null, 2));
        $("manualFeedback").value = data.summary || "";

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

        out("✅ Đã nhận xét chi tiết cho submission " + submissionId);

    } catch (err) {
        out("❌ " + err.message);
        console.error(err);
    }
}

// ======== MANUAL FEEDBACK (GIẢNG VIÊN GỬI LẠI) ========
document.getElementById("btnSendReviewed")?.addEventListener("click", async () => {
    try {
        const params = new URLSearchParams(window.location.search);
        const studentId = parseInt(params.get("studentId") || localStorage.getItem("selectedStudentId") || 0);
        const feedbackText = document.getElementById("manualFeedback")?.value.trim();

        if (!feedbackText) {
            alert("⚠️ Vui lòng nhập nội dung nhận xét trước khi gửi.");
            return;
        }

        const payload = {
            studentId,
            feedbackText,
            comment: "Giảng viên đã chỉnh sửa và gửi lại nhận xét."
        };

        console.log("📤 Sending reviewed feedback:", payload);

        const res = await apiFetch(`/manual/sendreviewed`, {
            method: "POST",
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

// ======== BUTTONS ========
$("btnClear").onclick = () => location.reload();
$("btnHealth").onclick = () => out("✅ Gateway hoạt động tốt tại " + GATEWAY);
