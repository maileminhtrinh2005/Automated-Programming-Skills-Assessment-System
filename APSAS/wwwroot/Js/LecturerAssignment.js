const baseUrl = "http://localhost:5261";
let selectedAssignmentId = null;
const token = localStorage.getItem("token");
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

// add assigmnet//////////////////////////////////////////////////////////////////////////////////
document.getElementById("addAssignmentBtn").onclick = () => {
    document.getElementById("addAssignmentModal").style.display = "flex";
};
document.getElementById("cancelAddBtn").onclick = () => {
    document.getElementById("addAssignmentModal").style.display = "none";
};
document.getElementById("addAssignmentForm").onsubmit = async (e) => {
    e.preventDefault();
    const newAssignment = {
        Title: document.getElementById("title").value,
        Description: document.getElementById("description").value,
        SampleTestCase: document.getElementById("sampleTestCase").value,
        Deadline: document.getElementById("deadline").value,
        Difficulty: document.getElementById("difficulty").value,
    };

    const res = await fetchWithToken(`${baseUrl}/AddAssignment`, {
        method: "POST",
        body: JSON.stringify(newAssignment),
    });

    if (res.ok) {
        alert("✅ Thêm bài tập thành công!");
        document.getElementById("addAssignmentModal").style.display = "none";
        loadAssignments();
    } else {
        alert("❌ Thêm thất bại!");
    }
};
//////////////////////////////////////////////////////////////////////////////////////////////////

//////////view resource//////////////////////////////////////////////////////////////////////////////
document.getElementById("viewResourceBtn").onclick = async () => {
    if (!selectedAssignmentId) return;

    try {
        const res = await fetchWithToken(`${baseUrl}/GetResourceById/${selectedAssignmentId}`);
        if (!res.ok) throw new Error("Không tìm thấy resource");
        const data = await res.json();

        document.getElementById("viewResourceTitle").innerText = data.title;
        document.getElementById("viewResourceType").innerText = data.type;
        const linkEl = document.getElementById("viewResourceLink");
        linkEl.href = data.link;
        linkEl.textContent = data.link;

        document.getElementById("viewResourceModal").style.display = "flex";
    } catch (err) {
        console.warn(err);
        alert("sẽ cập nhật trong thời gian tới!");
    }

    // Đóng popup Resource
    document.getElementById("closeViewResourceBtn").onclick = () => {
        document.getElementById("viewResourceModal").style.display = "none";
    };

};
////////////////////////////////////////////////////////////////////////////////////////////////////

////////////////////////////////them testcase///////////////////////////////////////////////////////
document.getElementById("editAssignmentBtn").onclick = () => {
    document.getElementById("editAssignmentModal").style.display = "flex";
};

document.getElementById("cancelEditBtn").onclick = () => {
    document.getElementById("editAssignmentModal").style.display = "none";
};

// 📘 Thêm testcase mới
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
        await fetchWithToken(`${baseUrl}/AddTestCase`, {
            method: "POST",
            body: JSON.stringify(testCaseRequest)
        });
        alert("✅ Cập nhật chi tiết thành công!");
        document.getElementById("editAssignmentModal").style.display = "none";
    } catch (err) {
        alert("❌ Lỗi khi lưu dữ liệu!");
        console.error(err);
    }
};
///////////////////////////////////////////////////////////////////////////////////////


// 📘 Mở popup thêm Resource
document.getElementById("editResourceBtn").onclick = () => {
    document.getElementById("resourceModal").style.display = "flex";
};

// 📘 Đóng popup Resource
document.getElementById("cancelResourceBtn").onclick = () => {
    document.getElementById("resourceModal").style.display = "none";
};

// 📘 Gửi Resource mới
document.getElementById("resourceForm").onsubmit = async (e) => {
    e.preventDefault();
    const resource = {
        AssignmentId: selectedAssignmentId,
        ResourceTitle: document.getElementById("resourceTitleInput").value,
        ResourceLink: document.getElementById("resourceLinkInput").value,
        ResourceType: document.getElementById("resourceTypeInput").value,
    };

    const res = await fetchWithToken(`${baseUrl}/AddResource`, {
        method: "POST",
        body: JSON.stringify(resource),
    });

    if (res.ok) {
        alert("✅ Thêm Resource thành công!");
        document.getElementById("resourceModal").style.display = "none";
    } else {
        alert("❌ Lỗi khi thêm Resource!");
    }
};


loadAssignments();



