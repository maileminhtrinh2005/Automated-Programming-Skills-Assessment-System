
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

let studentsData = []; // Lưu dữ liệu sinh viên

async function loadStudents() {
    const token = localStorage.getItem("token");

    try {
        const res = await fetch(apiUrl, {
            headers: { "Authorization": `Bearer ${token}` }
        });

        if (!res.ok) throw new Error("Không thể tải dữ liệu sinh viên!");

        const data = await res.json();
        studentsData = data; // Lưu dữ liệu

        const tbody = document.querySelector("#studentTable tbody");
        tbody.innerHTML = data.map(s =>
            `<tr>
                <td>${s.username}</td>
                <td>${s.fullName}</td>
                <td><button onclick="showID(${s.userID}, '${s.username}')">Xem ID</button></td>
            </tr>`
        ).join("");

    } catch (err) {
        alert(err.message);
    }
}

// Hiển thị bảng ID chỉ của sinh viên được chọn
function showID(userID, username) {
    const idTable = document.getElementById("idTable");
    const tbody = idTable.querySelector("tbody");

    tbody.innerHTML = `
        <tr>
            <td>${userID}</td>
            <td>${username}</td>
        </tr>
    `;

    idTable.style.display = "table"; // Hiển thị bảng
}



