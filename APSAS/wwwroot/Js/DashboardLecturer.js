// ======== SESSION TIMEOUT ========
let inactivityTime = 0;
const maxInactivity = 5 * 60 * 1000; // 5 phút

function resetTimer() {
    inactivityTime = 0;
}

window.onload = () => {
    document.onmousemove = resetTimer;
    document.onkeypress = resetTimer;

    setInterval(() => {
        inactivityTime += 1000;
        if (inactivityTime >= maxInactivity) {
            alert("⏰ Hết phiên làm việc do không hoạt động!");
            localStorage.removeItem("token");
            window.location.href = "DN.html";
        }
    }, 1000);

    checkAccess();
    loadStudents();
};

// ======== ACCESS CONTROL ========
function checkAccess() {
    const token = localStorage.getItem("token");
    const role = localStorage.getItem("role");

    if (!token || role !== "Lecturer") {
        alert("Bạn không có quyền truy cập!");
        window.location.href = "login.html";
    }
}

// ======== LOAD STUDENTS ========
const apiUrl = "http://localhost:5261/GetAllStudents";
let studentsData = [];

async function loadStudents() {
    const token = localStorage.getItem("token");

    try {
        const res = await fetch(apiUrl, {
            headers: { "Authorization": `Bearer ${token}` }
        });

        if (!res.ok) throw new Error("Không thể tải dữ liệu sinh viên!");

        const data = await res.json();
        studentsData = data;

        const tbody = document.querySelector("#studentTable tbody");
        // 👉 Chỉ hiển thị nút "Xem bài tập", không có "Xem ID" và không dùng bảng ID
        tbody.innerHTML = data.map(s =>
            `<tr id="row-${s.userID}">
                <td>${s.username}</td>
                <td>${s.fullName}</td>
                <td>
                    <button class="btn-view-task" onclick="openFeedback(${s.userID})">
                        Xem bài tập
                    </button>
                </td>
            </tr>`
        ).join("");

    } catch (err) {
        alert(err.message);
    }
}

// ======== HIỂN THỊ ID + NÚT XEM BÀI TẬP ========
//function showID(userID, username) {
//    const idTable = document.getElementById("idTable");
//    const tbody = idTable.querySelector("tbody");

//    // Hiển thị bảng có thêm nút "Xem bài tập"
//    tbody.innerHTML = `
//        <tr>
//            <td>${userID}</td>
//            <td>${username}</td>
//            <td>
//                <button onclick="openFeedback(${userID})" class="btn-view-task">
//                    Xem bài tập
//                </button>
//            </td>
//        </tr>
//    `;

//    idTable.style.display = "table"; // Hiện bảng ID
//}

// ======== MỞ TRANG FEEDBACK ========
function openFeedback(studentId) {
    // Lưu ID sinh viên vào localStorage
    localStorage.setItem("selectedStudentId", studentId);

    // 🔧 Đồng bộ key để Feedback.html đọc được
    localStorage.setItem("studentId", studentId);

    // Chuyển sang trang Feedback
    window.location.href = "Feedback.html";
}

