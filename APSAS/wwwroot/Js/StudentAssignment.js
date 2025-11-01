// -------------------- CONFIG --------------------
const baseUrl = "http://localhost:5261";
let selectedAssignmentId = null;
const token = localStorage.getItem("token");

// -------------------- HÀM FETCH KÈM TOKEN + CHECK EXPIRED --------------------
async function fetchWithToken(url, options = {}) {
    const token = localStorage.getItem("token");
    const headers = {
        "Content-Type": "application/json",
        ...options.headers,
        "Authorization": `Bearer ${token}`
    };

    const res = await fetch(url, { ...options, headers });

    // ✅ Nếu token hết hạn hoặc không hợp lệ
    if (res.status === 401) {
        alert("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại!");
        localStorage.clear(); // xoá token + info user
        window.location.href = "/Login.html"; // redirect về login
        return; // dừng tiếp
    }
    return res;
}
// -------------------- LOAD DANH SÁCH BÀI TẬP --------------------
async function loadAssignments() {
    try {
        const res = await fetchWithToken(`${baseUrl}/GetAllAssignment`);
        const data = await res.json();

        const container = document.getElementById("assignmentContainer");
        container.innerHTML = "";

        data.forEach(a => {
            const card = document.createElement("div");
            card.className = "assignment-card";
            card.innerHTML = `<h3>${a.title}</h3>`;
            card.onclick = () => openModal(a);
            container.appendChild(card);
        });
    } catch (err) {
        console.error("Lỗi khi tải danh sách bài tập:", err);
        alert("Không thể tải danh sách bài tập!");
    }
}

// -------------------- MỞ MODAL BÀI TẬP --------------------
function openModal(assignment) {
    selectedAssignmentId = assignment.assignmentId;
    document.getElementById("modalTitle").innerText = assignment.title;
    document.getElementById("modalDescription").innerText = assignment.description || "Không có mô tả";
    document.getElementById("doAssignmentBtn").style.display = "inline-block";
    const modal = document.getElementById("assignmentModal");
    modal.style.display = "block";
    // Đóng popup bài tập
    document.getElementById("closeModalBtn").onclick = () => {
        document.getElementById("assignmentModal").style.display = "none";
    };
    // direct submission page
    document.getElementById("doAssignmentBtn").onclick = () => {
        if (selectedAssignmentId) {
            localStorage.setItem("currentAssignmentId", selectedAssignmentId);
            window.location.href = `/SubmissionPage.html`;
        }
    };
}


// -------------------- XEM RESOURCE --------------------
document.getElementById("viewResourceBtn").onclick = async () => {
    if (!selectedAssignmentId) return;

    try {
        const res = await fetch(`${baseUrl}/GetResourceById/${selectedAssignmentId}`, {
            headers: { "Authorization": `Bearer ${token}` }
        });
        if (!res.ok) throw new Error("Không tìm thấy resource");
        const data = await res.json();

        document.getElementById("studentResourceTitle").innerText = data.title;
        document.getElementById("studentResourceType").innerText = data.type;
        const linkEl = document.getElementById("studentResourceLink");
        linkEl.href = data.link;
        linkEl.textContent = data.link;

        document.getElementById("studentResourceModal").style.display = "flex";
    } catch (err) {
        console.warn(err);
        alert("sẽ cập nhật trong thời gian tới!");
    }

    // Đóng popup Resource
    document.getElementById("closeStudentResourceBtn").onclick = () => {
        document.getElementById("studentResourceModal").style.display = "none";
    };

};

//-------------------------submissionssssss-----------------------
async function loadSubmissions() {
    const container = document.getElementById("submissionContainer");
    container.innerHTML = "<p>Đang tải...</p>";

    try {
        const res = await fetchWithToken(`${baseUrl}/GetMySubmission`)
        const submissions = await res.json();
        if (!submissions || submissions.length === 0) {
            container.innerHTML = "<p>Bạn chưa nộp bài nào.</p>";
            return;
        }

        container.innerHTML = "";

        for (const sub of submissions) {
            const resassignment = await fetchWithToken(`${baseUrl}/GetAssignmentById/${sub.assignmentId}`);
            const assigment = await resassignment.json();
            const card = document.createElement("div");
            card.className = "submission-card";
            card.innerHTML = `
                <h4> bai tap: ${assigment.title} </h4>
                <p><strong>score:</strong> ${sub.score}</p>
            `;
            card.onclick = () => showSubmissionDetail(sub);
            container.appendChild(card);
        }
    } catch (err) {
        console.error(err);
        container.innerHTML = "<p>Lỗi khi tải danh sách bài nộp.</p>";
    }
}
function showSubmissionDetail(submission) {
    const modal = document.getElementById("submissionModal");

    document.getElementById("modalLanguage").textContent = submission.languageName;
    document.getElementById("modalCode").textContent = submission.code;
    document.getElementById("modalScore").textContent = submission.score;
    document.getElementById("modalStatus").textContent = submission.status;
    document.getElementById("modalSubmittedAt").textContent = new Date(submission.submittedAt).toLocaleString();

    modal.style.display = "block";
    document.getElementById("viewResultBtn").onclick = () => {
        modal.style.display = "none";
        loadSubmissionResult(submission.submissionId);
    };

    document.getElementById("closeSubmissionModalBtn").onclick = () =>
        modal.style.display = "none";
}

// 🔹 Gọi API /GetYourResult/{id}
async function loadSubmissionResult(submissionId) {
    const container = document.getElementById("resultList");
    container.innerHTML = "<p>Đang tải kết quả...</p>";

    try {
        const res = await fetchWithToken(`${baseUrl}/GetYourResult/${submissionId}`)
        const results = await res.json();
        document.getElementById("submissionResultModal").style.display = "flex";

        if (!results || results.length === 0) {
            container.innerHTML = "<p>Không có kết quả nào.</p>";
            return;
        }
        let count = 0;
        container.innerHTML = "";


        results.forEach(r => {
            const card = document.createElement("div");
            count++;
            card.className = `result-card ${r.passed ? 'passed' : 'failed'}`;
            card.innerHTML = `
                <h4>Test Case ${count}</h4>
                <p><strong>Kết quả:</strong> <pre>${r.output || "(Không có output)"}</pre></p>
                <p><strong>Trạng thái:</strong> ${r.status || (r.passed ? '✅' : '❌')}</p>
                <p><strong>Thời gian chạy:</strong> ${r.executionTime} ms</p>
                <p><strong>Bộ nhớ dùng:</strong> ${r.memoryUsed} KB</p>
                ${r.errorMessage ? `<p><strong>Lỗi:</strong> <pre>${r.errorMessage}</pre></p>` : ""}
            `;
            container.appendChild(card);
        });

        document.getElementById("submissionResultModal").style.display = "block";

    } catch (err) {
        container.innerHTML = "<p>Lỗi khi tải kết quả.</p>";
        console.error(err);
    }

    document.getElementById("closeResultModalBtn").onclick = () => {
        document.getElementById("submissionResultModal").style.display = "none";
    };
}

// --- Xử lý dropdown người dùng ---
const userBtn = document.getElementById("userBtn");
const userDropdown = document.getElementById("userDropdown");

// Toggle hiển thị menu
userBtn.addEventListener("click", () => {
    userDropdown.style.display =
        userDropdown.style.display === "block" ? "none" : "block";
});

// Ẩn menu khi click ra ngoài
window.addEventListener("click", (e) => {
    if (!userBtn.contains(e.target) && !userDropdown.contains(e.target)) {
        userDropdown.style.display = "none";
    }
});

// Nút đăng xuất
document.getElementById("logoutBtn").onclick = () => {
    if (confirm("Bạn có chắc muốn đăng xuất không?")) {
        localStorage.clear();
        window.location.href = "/Login.html";
    }
};

// Nút xem hồ sơ
document.getElementById("viewProfile").onclick = () => {
    window.location.href = "/Profile.html";
};

// Nút đổi mật khẩu
document.getElementById("changePassword").onclick = () => {
    window.location.href = "/ChangePassword.html";
};

// -------------------- INIT --------------------
loadAssignments();
loadSubmissions();




