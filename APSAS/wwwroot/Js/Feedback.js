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
        window.location.href = "Login.html";
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
    const res = await apiFetch(`/GetSubmissions/${studentId}`);
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

// ======== NHẬN XÉT CHI TIẾT (CHẤM THEO TESTCASE) ========
async function generateDetailFeedback(submissionId) {
    try {
        out(`🔍 Đang lấy result cho submission ${submissionId}...`);

        // Lấy result của submission
        const result = await fetchResultBySubmission(submissionId);
        console.log("📡 Kết quả từ API GetYourResult:", result);

        const submissions = await fetchSubmissionsByStudent(localStorage.getItem("studentId"));
        const submission = submissions.find(s => s.submissionId == submissionId);

        if (!submission) return alert("❌ Không tìm thấy submission.");

        console.log("📄 submission data:", submission);
        console.log("📄 assignmentId:", submission.assignmentId);

        // ===== Gọi test case từ AssignmentService =====
        const testcasesRes = await apiFetch(`/GetTestCaseById/${submission.assignmentId}`);
        console.log("📡 Gọi API test case:", testcasesRes.status, testcasesRes.url);

        if (!testcasesRes.ok) throw new Error(await testcasesRes.text());
        const testcasesRaw = await testcasesRes.json();
        console.log("📦 TestcasesRaw:", testcasesRaw);

        // 🔄 Chuẩn hóa dữ liệu testcase & result
        const testcases = testcasesRaw.map(tc => ({
            input: tc.input ?? tc.Input ?? "",
            expectedOutput: tc.expectedOutput ?? tc.ExpectedOutput ?? ""
        }));

        const results = Array.isArray(result)
            ? result
            : result?.testResults ?? [];

        // ✅ CHỈNH LẠI CHUẨN HÓA CHO ĐÚNG DỮ LIỆU SubmissionService
        const normalizedResults = testcases.map((tc, i) => {
            const match = results[i];
            return {
                input: tc.input ?? "",
                expectedOutput: tc.expectedOutput ?? "",
                actualOutput: match ? (match.output ?? "Không có") : "Không có",
                status: match ? (match.passed ? "Passed" : "Failed") : "Chưa chạy",
                executionTime: match ? (match.executionTime ?? 0) : 0,
                memoryUsed: match ? (match.memoryUsed ?? 0) : 0,
                errorMessage: match?.errorMessage ?? ""
            };
        });

        console.log("✅ normalizedResults gửi sang FeedbackService:", normalizedResults);

        // ===== Gửi sang FeedbackService =====
        const payload = {
            submissionId,
            assignmentTitle: submission?.assignmentTitle || "Không rõ",
            sourceCode: submission?.code || submission?.sourceCode || "Không có source code",
            testResults: normalizedResults
        };

        out("📤 Gửi sang FeedbackService để sinh nhận xét chi tiết...");
        const res = await apiFetch(`/testcasesubmit`, {
            method: "POST",
            body: JSON.stringify(payload)
        });

        if (!res.ok) throw new Error(await res.text());
        const data = await res.json();
        out("✅ Nhận xét chi tiết:", data);

        // ======== HIỂN THỊ NHẬN XÉT ========
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

        // ======== HIỂN THỊ CHI TIẾT TESTCASE ========
        const detailSection = $("detailSection");
        const detailBody = $("tblDetails").querySelector("tbody");
        detailBody.innerHTML = "";

        if (Array.isArray(data.testCaseFeedback) && data.testCaseFeedback.length > 0) {
            detailSection.style.display = "block";

            let passCount = 0, failCount = 0;
            data.testCaseFeedback.forEach(tc => {
                const status = (tc.status || "").toLowerCase();
                if (status.includes("pass") || status.includes("đúng")) passCount++;
                else if (status.includes("fail") || status.includes("sai")) failCount++;
            });

            const summaryRow = document.createElement("tr");
            summaryRow.innerHTML = `
                <td colspan="5" style="text-align:center; background:#f7f7f7; font-weight:bold;">
                    Tổng kết: ${passCount}/${data.testCaseFeedback.length} test pass (${Math.round(passCount / data.testCaseFeedback.length * 100)}%)
                </td>`;
            detailBody.appendChild(summaryRow);

            data.testCaseFeedback.forEach((tc, i) => {
                const row = document.createElement("tr");
                const status = (tc.status || "").toLowerCase();
                let emoji = "⚠️";
                if (status.includes("pass") || status.includes("đúng")) emoji = "✅";
                else if (status.includes("fail") || status.includes("sai")) emoji = "❌";

                row.innerHTML = `
                  <td>${i + 1}</td>
                  <td>${emoji} ${tc.status ?? "Chưa chạy"}</td>
                  <td>${tc.input ?? tc.name ?? "Không có"}</td>
                  <td>${tc.expectedOutput ?? "—"}</td>
                  <td>${tc.comment ?? "(Không có nhận xét)"}</td>
                `;
                detailBody.appendChild(row);
            });
        } else {
            detailSection.style.display = "block";
            detailBody.innerHTML = `
                <tr><td colspan="5" style="text-align:center;color:gray;">
                    ⚠️ Không có nhận xét chi tiết cho từng test case.
                </td></tr>`;
        }

    } catch (err) {
        alert("❌ Lỗi khi sinh nhận xét chi tiết: " + err.message);
        console.error(err);
    }
}

// ======== NHẬN XÉT TỔNG QUÁT (KHÔNG ĐỔI) ========
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

// ======== NÚT ========
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

$("btnClear").onclick = () => location.reload();
$("btnClear2").onclick = () => $("manualFeedback").value = "";
$("btnGenerate").onclick = () => {
    const studentId = localStorage.getItem("selectedStudentId") || localStorage.getItem("studentId");
    if (!studentId) return alert("⚠️ Không có studentId!");
    generateProgressFeedback(studentId);
};
