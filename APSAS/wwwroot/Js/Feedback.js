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
    if (!token) {
        window.location.href = "DN.html";
        return false;
    }
    return true;
}

// ======== PAGE LOAD ========
window.addEventListener("DOMContentLoaded", async () => {
    checkAccess();

    const params = new URLSearchParams(window.location.search);
    const studentId = params.get("studentId") || localStorage.getItem("selectedStudentId");

    if (studentId) {
        console.log("📌 Student ID nhận từ Dashboard:", studentId);
        localStorage.setItem("selectedStudentId", studentId);
        out("✅ Sẵn sàng. Danh sách bài tập đã được tải. Nhấn 'Nhận xét tổng quát' để xem feedback.");
        await loadSubmissionsOnly(studentId);
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

// ======== CHỈ LOAD DANH SÁCH BÀI TẬP ========
async function loadSubmissionsOnly(studentId) {
    try {
        out("📥 Đang tải danh sách bài tập...");
        const submissions = await fetchSubmissionsByStudent(studentId);
        if (!Array.isArray(submissions) || submissions.length === 0)
            return out("❌ Không có submission nào.");

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
        out("✅ Đã tải danh sách bài tập.");
    } catch (err) {
        out("❌ " + err.message);
        console.error(err);
    }
}

// ======== GENERATE FEEDBACK (TỔNG QUÁT) ========
async function generateProgressFeedback(studentId) {
    try {
        out(`⏳ Đang sinh nhận xét tổng quát cho sinh viên ${studentId}...`);

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

        const payload = { studentId, submissions: detailedSubs };
        console.log("📡 POST:", `${GATEWAY}/feedback/generate/bulk`, payload);

        const res = await apiFetch(`/feedback/generate/bulk`, {
            method: "POST",
            body: JSON.stringify(payload)
        });

        if (!res.ok) throw new Error(await res.text());
        const data = await res.json();

        $("aiSummary").value = data.summary || "(Không có)";
        $("aiScore").value = data.overallProgress || "";
        $("manualFeedback").value = data.summary || "";
        out(JSON.stringify(data, null, 2));

    } catch (err) {
        console.error(err);
        out("❌ " + err.message);
    }
}

// ======== CHI TIẾT TEST CASE ========
async function generateDetailFeedback(submissionId) {
    try {
        out("⏳ Đang lấy chi tiết submission " + submissionId + "...");

        const result = await fetchResultBySubmission(submissionId);
        if (!result) throw new Error("Không lấy được result");

        const testResultsRaw = Array.isArray(result) ? result : (result.testResults || []);
        if (!testResultsRaw.length)
            throw new Error("Submission này chưa có test case result nào.");

        const payload = { SubmissionId: submissionId, TestResults: testResultsRaw };

        out("📤 Gửi sang FeedbackService (chi tiết từng testcase)...");
        const res = await apiFetch(`/feedbacktestcase`, {
            method: "POST",
            body: JSON.stringify(payload)
        });

        if (!res.ok) throw new Error(await res.text());
        const data = await res.json();

        $("aiSummary").value = data.summary || "(Không có)";
        $("aiScore").value = data.score ?? "";
        $("manualFeedback").value = data.summary || "";
        out(JSON.stringify(data, null, 2));
    } catch (err) {
        out("❌ " + err.message);
        console.error(err);
    }
}

// ======== GIẢNG VIÊN GỬI LẠI FEEDBACK ========
$("btnSendReviewed")?.addEventListener("click", async () => {
    try {
        const studentId = localStorage.getItem("selectedStudentId");
        const feedbackText = $("manualFeedback")?.value.trim();
        if (!feedbackText) return alert("⚠️ Vui lòng nhập nội dung nhận xét trước khi gửi.");

        const payload = { studentId, feedbackText, comment: "Giảng viên đã chỉnh sửa và gửi lại nhận xét." };
        console.log("📤 Sending reviewed feedback:", payload);

        const res = await apiFetch(`/manual/sendreviewed`, {
            method: "POST",
            body: JSON.stringify(payload)
        });

        if (res.ok) alert("✅ Đã gửi nhận xét thành công!");
        else alert("❌ Gửi thất bại: " + (await res.text()));
    } catch (err) {
        alert("❌ Lỗi khi gửi nhận xét.");
        console.error(err);
    }
});

// ======== BUTTONS ========
$("btnClear").onclick = () => location.reload();
$("btnHealth").onclick = () => out("✅ Gateway hoạt động tốt tại " + GATEWAY);
$("btnGenerate").onclick = () => {
    const studentId = localStorage.getItem("selectedStudentId");
    if (!studentId) return alert("⚠️ Không có studentId!");
    generateProgressFeedback(studentId);
};
