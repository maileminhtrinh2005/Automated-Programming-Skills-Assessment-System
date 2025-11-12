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
            await openSubmissionModal(submissionId);
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
            createdAt: s.createdAt || s.submittedAt || "-",
            assignmentId: s.assignmentId,
            sourceCode: s.sourceCode || s.code || "(Không có code)"
        });
    }

    renderSubmissions(detailedSubs);
}

// ======== POPUP CHI TIẾT BÀI NỘP ========
async function openSubmissionModal(submissionId) {
    try {
        const studentId = localStorage.getItem("studentId");
        const submissions = await fetchSubmissionsByStudent(studentId);
        const submission = submissions.find(s => s.submissionId == submissionId);
        if (!submission) return alert("Không tìm thấy submission!");

        const result = await fetchResultBySubmission(submissionId);

        $("modalCode").innerText = submission.sourceCode || submission.code || "Không có code";
        $("modalScore").innerText = submission.score ?? "-";
        $("modalStatus").innerText = submission.status ?? "-";
        $("modalSubmittedAt").innerText = submission.createdAt || submission.submittedAt || "-";

        $("submissionModal").style.display = "block";

        $("closeSubmissionModalBtn").onclick = () => {
            $("submissionModal").style.display = "none";
        };

        $("btnFeedbackDetail").onclick = async () => {
            $("submissionModal").style.display = "none";
            await generateDetailFeedback(submissionId);
        };
    } catch (err) {
        console.error("❌ Lỗi khi mở modal:", err);
        alert("Không thể hiển thị thông tin bài nộp!");
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

        // 📦 Gửi yêu cầu sinh nhận xét tổng quát
        const payload = { studentId, submissions: detailedSubs };
        const res = await apiFetch(`/feedback/generate/bulk`, {
            method: "POST",
            body: JSON.stringify(payload)
        });

        if (!res.ok) throw new Error(await res.text());
        const data = await res.json();
        console.log("✅ Kết quả nhận xét tổng quát từ AI:", data);

        // ======== HIỂN THỊ LÊN GIAO DIỆN ========
        $("feedbackCard").style.display = "block";

        const summary = data.summary || "(Không có nhận xét)";
        $("summaryText").textContent = summary;
        $("manualFeedback").value = summary;

        // 🧩 Nếu API có overallProgress thì dùng luôn
        let progressText = data.overallProgress?.trim() || "";

        // 🧠 Nếu không có thì phân tích summary
        if (!progressText) {
            // Chuẩn hóa bỏ dấu tiếng Việt
            const normalizeVietnamese = str => str
                .normalize("NFD")
                .replace(/[\u0300-\u036f]/g, "")
                .toLowerCase();

            const summaryNorm = normalizeVietnamese(summary);

            if (
                summaryNorm.includes("tot") ||
                summaryNorm.includes("chinh xac") ||
                summaryNorm.includes("dung") ||
                summaryNorm.includes("hoat dong dung") ||
                summaryNorm.includes("thanh cong") ||
                summaryNorm.includes("dat yeu cau") ||
                summaryNorm.includes("dat diem toi da") ||
                summaryNorm.includes("tuyet doi") ||
                summaryNorm.includes("hoan thanh") ||
                summaryNorm.includes("nam vung") ||
                summaryNorm.includes("ap dung tot") ||
                summaryNorm.includes("xuat sac")
            ) {
                progressText = "Đạt tiến bộ tốt";
            }
            else if (
                summaryNorm.includes("cai thien") ||
                summaryNorm.includes("chua hoan thien") ||
                summaryNorm.includes("thieu") ||
                summaryNorm.includes("can xu ly") ||
                summaryNorm.includes("mot phan") ||
                summaryNorm.includes("gan dat") ||
                summaryNorm.includes("can nang cao") ||
                summaryNorm.includes("chua toi uu") ||
                summaryNorm.includes("loi nho") ||
                summaryNorm.includes("han che")
            ) {
                progressText = "Có tiến bộ nhưng cần cải thiện";
            }
            else {
                progressText = "Không có tiến bộ";
            }
        }

        // 🚀 Cập nhật UI
        const prog = $("progressText");
        prog.className = "";

        if (progressText.includes("tốt"))
            prog.classList.add("progress-good");
        else if (progressText.includes("cải thiện"))
            prog.classList.add("progress-medium");
        else
            prog.classList.add("progress-bad");

        prog.textContent = progressText;

    } catch (err) {
        alert("❌ Lỗi khi sinh nhận xét tổng quát: " + err.message);
        console.error("🔥 Chi tiết lỗi:", err);
    }
}






// ======== NHẬN XÉT CHI TIẾT ========
async function generateDetailFeedback(submissionId) {
    try {
        out(`🔍 Đang lấy result cho submission ${submissionId}...`);

        const studentId = localStorage.getItem("selectedStudentId") || localStorage.getItem("studentId");
        if (!studentId) {
            alert("❌ Không tìm thấy StudentId!");
            return;
        }

        // 🔹 Lấy kết quả thực thi từ SubmissionService
        const result = await fetchResultBySubmission(submissionId);
        console.log("📡 Kết quả từ API GetYourResult:", result);

        // 🔹 Lấy danh sách bài nộp
        const submissions = await fetchSubmissionsByStudent(studentId);
        const submission = submissions.find(s => s.submissionId == submissionId);
        if (!submission) {
            alert("❌ Không tìm thấy submission.");
            return;
        }

        // 🔹 Lấy danh sách test case từ AssignmentService
        const testcasesRes = await apiFetch(`/GetTestCaseById/${submission.assignmentId}`);
        if (!testcasesRes.ok) throw new Error(await testcasesRes.text());
        const testcasesRaw = await testcasesRes.json();

        // ✅ Chuẩn hóa danh sách test case (luôn có name hiển thị)
        const testcases = (testcasesRaw || []).map((tc, index) => ({
            id: tc.id ?? tc.testCaseId ?? 0,
            name: tc.name ?? `Test case ${index + 1}`,
            input: tc.input ?? tc.Input ?? "",
            expectedOutput: tc.expectedOutput ?? tc.ExpectedOutput ?? ""
        }));

        // ✅ Lấy danh sách kết quả thực thi
        const results = Array.isArray(result)
            ? result
            : result?.testResults ?? [];

        // ✅ Gộp test case và result
        const normalizedResults = testcases.map(tc => {
            const match = results.find(r => +r.testCaseId === +tc.id);
            return {
                name: tc.name,
                input: tc.input || "",
                expectedOutput: tc.expectedOutput || "",
                actualOutput: match?.output ?? "Không có",
                status: match
                    ? (match.passed ? "Đúng" : "Sai")
                    : "Chưa chạy",
                comment: match?.comment ?? ""
            };
        });

        // ✅ Gửi dữ liệu sang FeedbackService
        const payload = {
            studentId: Number(studentId),
            submissionId,
            assignmentTitle: submission.assignmentTitle || "Không rõ",
            sourceCode: submission.code || submission.sourceCode || "Không có source code",
            testResults: normalizedResults
        };

        console.log("📤 Payload gửi FeedbackService:", payload);

        const res = await apiFetch(`/testcasesubmit`, {
            method: "POST",
            body: JSON.stringify(payload)
        });

        if (!res.ok) throw new Error(await res.text());
        const data = await res.json();
        console.log("✅ Kết quả nhận xét chi tiết từ AI:", data);

        // ✅ Chuẩn hóa status trong test case feedback nếu thiếu
        if (Array.isArray(data.testCaseFeedback)) {
            data.testCaseFeedback = data.testCaseFeedback.map((t, i) => {
                let status = t.status?.trim();
                const cmt = (t.comment || "").toLowerCase();

                if (!status) {
                    if (cmt.match(/sai|fail|lỗi|không đúng|chưa chính xác/)) status = "Sai";
                    else if (cmt.match(/đúng|pass|thành công/)) status = "Đúng";
                    else status = normalizedResults[i]?.status || "Chưa chạy";
                }
                return { ...t, status };
            });
        }

        // ======== HIỂN THỊ GIAO DIỆN ========
        const card = $("feedbackCard");
        if (!card) throw new Error("Không tìm thấy #feedbackCard trong DOM.");
        card.style.display = "block";

        // 🧠 Nhận xét tổng quát
        const summary = data.summary || "(Không có nhận xét)";
        $("summaryText").textContent = summary;
        $("manualFeedback").value = summary;

        // 🚀 Tự động xác định tiến bộ
        const summaryLower = summary.toLowerCase();
        const prog = $("progressText");
        let progressLabel = "";
        prog.className = "";

        if (
            summaryLower.includes("tốt") ||
            summaryLower.includes("chính xác") ||
            summaryLower.includes("đúng") ||
            summaryLower.includes("hoạt động đúng") ||
            summaryLower.includes("đạt yêu cầu") ||
            summaryLower.includes("thành công")
        ) {
            progressLabel = "Đạt tiến bộ tốt";
            prog.classList.add("progress-good");
        } else if (
            summaryLower.includes("cải thiện") ||
            summaryLower.includes("chưa hoàn thiện") ||
            summaryLower.includes("thiếu") ||
            summaryLower.includes("cần xử lý") ||
            summaryLower.includes("một phần")
        ) {
            progressLabel = "Có tiến bộ nhưng cần cải thiện";
            prog.classList.add("progress-medium");
        } else {
            progressLabel = "Không có tiến bộ";
            prog.classList.add("progress-bad");
        }

        prog.textContent = progressLabel;

        // ======== HIỂN THỊ BẢNG TEST CASE ========
        const detailSection = $("detailSection");
        const detailBody = $("tblDetails")?.querySelector("tbody");
        if (!detailBody) throw new Error("Thiếu tbody trong bảng test case!");
        detailSection.style.display = "block";
        detailBody.innerHTML = "";

        const testFeedback = data.testCaseFeedback || [];

        if (testFeedback.length > 0) {
            const passCount = testFeedback.filter(t => (t.status || "").includes("Đúng")).length;
            const total = testFeedback.length;

            // Hàng tổng kết
            const summaryRow = document.createElement("tr");
            summaryRow.innerHTML = `
                <td colspan="5" style="text-align:center;background:#f7f7f7;font-weight:bold;">
                    Tổng kết: ${passCount}/${total} test đúng (${Math.round(passCount / total * 100)}%)
                </td>`;
            detailBody.appendChild(summaryRow);

            // Chi tiết từng test
            testFeedback.forEach((tc, i) => {
                const status = (tc.status || "Chưa chạy").toLowerCase();
                const emoji = status.includes("đúng")
                    ? "✅"
                    : status.includes("sai")
                        ? "❌"
                        : "⚠️";

                const row = document.createElement("tr");
                row.innerHTML = `
                    <td>${i + 1}</td>
                    <td>${emoji} ${tc.status ?? "Chưa chạy"}</td>
                    <td>${tc.input || tc.name || `Test case ${i + 1}`}</td>
                    <td>${tc.expectedOutput ?? "—"}</td>
                    <td>${tc.comment ?? "(Không có nhận xét)"}</td>
                `;
                detailBody.appendChild(row);
            });
        } else {
            detailBody.innerHTML = `
                <tr><td colspan="5" style="text-align:center;color:gray;">
                    ⚠️ Không có nhận xét chi tiết cho từng test case.
                </td></tr>`;
        }

    } catch (err) {
        alert("❌ Lỗi khi sinh nhận xét chi tiết: " + err.message);
        console.error("🔥 Chi tiết lỗi:", err);
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
