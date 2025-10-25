const isTeacher = false; // 👈 bạn thay bằng kiểm tra role thật từ JWT hoặc session
let selectedAssignmentId = null;
const baseUrl = "http://localhost:5261";
async function loadAssignments() {
    const res = await fetch(`${baseUrl}/GetAllAssignment`);
    const data = await res.json();
    const container = document.getElementById("assignmentContainer");
    container.innerHTML = "";

    data.forEach(a => {
        const card = document.createElement("div");
        card.className = "assignment-card";
        card.innerHTML = `
      <h3>${a.title}</h3>
    `;
        card.onclick = () => openModal(a);
        container.appendChild(card);
    });

    // Nếu không phải giáo viên thì ẩn nút thêm bài tập
    if (!isTeacher) document.getElementById("addAssignmentBtn").style.display = "none";
}

function openModal(assignment) {
    selectedAssignmentId = assignment.assignmentId;
    document.getElementById("modalTitle").innerText = assignment.title;
    const descContainer = document.getElementById("modalDescription");
    if (descContainer) descContainer.innerText = assignment.description || "Không có mô tả";

    const modal = document.getElementById("assignmentModal");
    modal.style.display = "block";

    // Hiển thị nút tùy role
    document.getElementById("doAssignmentBtn").style.display = isTeacher ? "none" : "inline-block";
    document.getElementById("editAssignmentBtn").style.display = isTeacher ? "inline-block" : "none";
}

document.getElementById("closeModalBtn").onclick = () => {
    document.getElementById("assignmentModal").style.display = "none";
};

document.getElementById("doAssignmentBtn").onclick = () => {
    window.location.href = `/SubmissionPage.html?id=${selectedAssignmentId}`;
};

loadAssignments();

// Mở popup thêm bài tập
document.getElementById("addAssignmentBtn").onclick = () => {
    document.getElementById("addAssignmentModal").style.display = "flex";
};

// Đóng popup khi bấm Hủy
document.getElementById("cancelAddBtn").onclick = () => {
    document.getElementById("addAssignmentModal").style.display = "none";
};

// Submit form thêm bài tập
document.getElementById("addAssignmentForm").onsubmit = async (e) => {
    e.preventDefault();

    const newAssignment = {
        Title: document.getElementById("title").value,
        Description: document.getElementById("description").value,
        SampleTestCase: document.getElementById("sampleTestCase").value,
        Deadline: document.getElementById("deadline").value,
        Difficulty: document.getElementById("difficulty").value,
    };

    const res = await fetch(`${baseUrl}/AddAssignment`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(newAssignment),
    });

    if (res.ok) {
        alert("Thêm bài tập thành công!");
        document.getElementById("addAssignmentModal").style.display = "none";
        loadAssignments(); // Reload lại danh sách
    } else {
        alert("Thêm thất bại!");
    }
};




document.getElementById("editAssignmentBtn").onclick = () => {
    const modal = document.getElementById("editAssignmentModal");
    modal.style.display = "flex";
};

// Đóng popup khi bấm "Hủy"
document.getElementById("cancelEditBtn").onclick = () => {
    document.getElementById("editAssignmentModal").style.display = "none";
};

// ✅ Thêm dòng TestCase mới
document.getElementById("addTestcaseBtn").onclick = () => {
    const tbody = document.getElementById("testcaseBody");
    const row = document.createElement("tr");

    row.innerHTML = `
        <td><textarea class="testcase-input" rows="3" placeholder="Nhập input..."></textarea></td>
        <td><textarea class="testcase-output" rows="3" placeholder="Nhập output mong đợi..."></textarea></td>
        <td><input type="number" class="weight-value" value="1" min="0" step="0.1"></td>
        <td><button class="removeRowBtn">❌</button></td>
    `;

    row.querySelector(".removeRowBtn").onclick = () => row.remove();
    tbody.appendChild(row);
};

// ✅ Lưu chỉnh sửa
document.getElementById("saveEditBtn").onclick = async () => {
    const testCases = [];
    document.querySelectorAll("#testcaseBody tr").forEach(row => {
        const input = row.querySelector(".testcase-input").value.trim();
        const output = row.querySelector(".testcase-output").value.trim();
        const weight = parseFloat(row.querySelector(".weight-value").value) || 1;
        testCases.push({ input, expectedOutput: output, weight });
    });

    const testCaseRequest = {
        AssignmentId: selectedAssignmentId,
        testCaseItems: testCases
    };

    try {

        // Gửi Test Cases
        await fetch(`${baseUrl}/AddTestCase`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(testCaseRequest)
        });

        alert("Cập nhật chi tiết thành công!");
        document.getElementById("editAssignmentModal").style.display = "none";
    } catch (err) {
        alert("Lỗi khi lưu dữ liệu!");
        console.error(err);
    }
};


document.getElementById("viewResourceBtn").onclick = () => {
    if (isTeacher) {
        document.getElementById("resourceModal").style.display = "flex";
    } else {
        // học sinh tạm thời bỏ qua, sau này có thể show nội dung resource
        alert("Chức năng dành cho học sinh sẽ cập nhật sau!");
    }
};

document.getElementById("cancelResourceBtn").onclick = () => {
    document.getElementById("resourceModal").style.display = "none";
};

document.getElementById("resourceForm").onsubmit = async (e) => {
    e.preventDefault();

    const resource = {
        AssignmentId: selectedAssignmentId,
        ResourceTitle: document.getElementById("resourceTitleInput").value,
        ResourceLink: document.getElementById("resourceLinkInput").value,
        ResourceType: document.getElementById("resourceTypeInput").value,
    };

    const res = await fetch(`${baseUrl}/AddResource`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(resource),
    });

    if (res.ok) {
        alert("Thêm Resource thành công!");
        document.getElementById("resourceModal").style.display = "none";
    } else {
        alert("Lỗi khi thêm Resource!");
    }
};


document.getElementById("viewResourceBtn").onclick = async () => {
    if (isTeacher) {
        // Giáo viên → mở popup thêm Resource
        document.getElementById("resourceModal").style.display = "flex";
    } else {
        // Học sinh → load Resource từ server
        try {
            const res = await fetch(`${baseUrl}/GetResourceById/${selectedAssignmentId}`);
            if (!res.ok) throw new Error("Không tìm thấy resource");
            const data = await res.json();

            // Gán dữ liệu vào popup
            document.getElementById("studentResourceTitle").innerText = data.title;
            document.getElementById("studentResourceType").innerText = data.type;
            const linkEl = document.getElementById("studentResourceLink");
            linkEl.href = data.link;
            linkEl.textContent = data.link;

            document.getElementById("studentResourceModal").style.display = "flex";
        } catch (err) {
            alert("❌ Không có hướng dẫn cho bài tập này!");
        }
    }
};

// Đóng popup resource học sinh
document.getElementById("closeStudentResourceBtn").onclick = () => {
    document.getElementById("studentResourceModal").style.display = "none";
};