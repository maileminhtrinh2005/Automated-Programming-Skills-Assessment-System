// =============================
// 🔔 BIẾN TOÀN CỤC
// =============================
const notificationsDiv = document.getElementById("notifications");
const badge = document.getElementById("badge");
const bell = document.getElementById("bell");
const sound = document.getElementById("notifSound");
let count = 0;

function checkAccess() {
    const token = localStorage.getItem("token");
    if (!token) {
        window.location.href = "DN.html";
        return false;
    }
    return true;
}

// hien thi thông báo mới
function addNotification(id, title, message, time) {
    const div = document.createElement("div");
    div.className = "notification";
    div.setAttribute("data-id", id);

    div.innerHTML = `
    <b>${title}</b><br>
    <p>${(message ?? "(Không có nội dung)").replace(/\n/g, "<br>")}</p>
    <time>${new Date(time).toLocaleString()}</time><br>
    <button class="mark-btn" onclick="markAsRead('${id}', this)">✅ Đã xem</button>
  `;
    notificationsDiv.prepend(div);

    count++;
    badge.textContent = count;

    bell.classList.add("shake");
    sound.play().catch(() => { });
    setTimeout(() => bell.classList.remove("shake"), 800);
}

// =============================
// ⚙️ CẤU HÌNH SERVER
// =============================
// 👉 PORT 5261 = API Gateway (REST API)
// 👉 PORT 5216 = NotificationService (SignalR Hub)
const HUB_URL = "http://localhost:5216/notificationhub";
const API_BASE = "http://localhost:5261/api/Notification"; // ✅ ĐÃ SỬA DỨT ĐIỂM 404

// =============================
// ⚙️ KẾT NỐI SIGNALR
// =============================
const connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL, { accessTokenFactory: () => localStorage.getItem("token") })
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect()
    .build();

// =============================
// 📩 LẮNG NGHE SỰ KIỆN SERVER GỬI
// =============================
connection.on("NotifyNew", (dto) => {
    console.log("📩 Received notification:", dto);
    const id = dto.id ?? dto.Id ?? crypto.randomUUID();
    const title = dto.title ?? dto.Title ?? "(Không có tiêu đề)";
    const message = dto.message ?? dto.Message ?? "(Không có nội dung)";
    const time = dto.createdAtUtc ?? dto.CreatedAtUtc ?? new Date().toISOString();

    addNotification(id, title, message, time);
});

// =============================
// 📨 THÔNG BÁO CHƯA ĐỌC
// =============================
async function loadUnreadNotifications() {
    const studentId = localStorage.getItem("studentId");
    if (!studentId) {
        console.warn("⚠️ Không có studentId trong localStorage!");
        return;
    }

    try {
        const token = localStorage.getItem("token");
        // ✅ Dùng URL tuyệt đối để tránh 404
        const res = await fetch(`${API_BASE}/unread?studentId=${studentId}`, {
            headers: token ? { "Authorization": `Bearer ${token}` } : {}
        });

        if (!res.ok) {
            console.error(`❌ Lỗi server: ${res.status} ${res.statusText}`);
            throw new Error("Không thể tải thông báo chưa đọc!");
        }

        const data = await res.json();
        if (!Array.isArray(data) || data.length === 0) {
            console.log("✅ Không có thông báo chưa đọc!");
            notificationsDiv.innerHTML = "<p>Không có thông báo chưa đọc.</p>";
            badge.textContent = 0;
            return;
        }

        // ✅ Lưu cache vào localStorage
        localStorage.setItem("unreadCache", JSON.stringify(data));
        renderNotifications(data);
        console.log(`📬 Loaded ${data.length} unread notifications từ server`);
    } catch (err) {
        console.error("❌ Lỗi load unread:", err);
        const saved = JSON.parse(localStorage.getItem("unreadCache") || "[]");
        renderNotifications(saved);
    }
}

// ham render thông báo
function renderNotifications(list) {
    notificationsDiv.innerHTML = "";
    count = 0;
    list.forEach(n => addNotification(n.id, n.title, n.message, n.createdAtUtc));
}

// =============================
// ✅ ĐÁNH DẤU 1 THÔNG BÁO LÀ ĐÃ XEM
// =============================
async function markAsRead(id, btn) {
    try {
        const token = localStorage.getItem("token");
        // ✅ Sửa URL tuyệt đối để tránh 404
        const res = await fetch(`${API_BASE}/markasread?id=${id}`, {
            method: "POST",
            headers: token ? { "Authorization": `Bearer ${token}` } : {}
        });

        if (res.ok) {
            console.log(`✅ Marked notification ${id} as read`);
            btn.parentElement.remove();
            count--;
            badge.textContent = count;
        } else {
            const msg = await res.text();
            console.error(`❌ markAsRead failed: ${msg}`);
        }
    } catch (err) {
        console.error("❌ Lỗi khi markAsRead:", err);
    }
}

// =============================
// ✅ ĐÁNH DẤU TẤT CẢ LÀ ĐÃ XEM
// =============================
document.getElementById("markAllBtn").addEventListener("click", async () => {
    const allNotis = document.querySelectorAll(".notification");
    if (allNotis.length === 0) {
        alert("✅ Không còn thông báo chưa đọc.");
        return;
    }

    const token = localStorage.getItem("token");
    for (const noti of allNotis) {
        const id = noti.getAttribute("data-id");
        try {
            await fetch(`${API_BASE}/markasread?id=${id}`, {
                method: "POST",
                headers: token ? { "Authorization": `Bearer ${token}` } : {}
            });
            noti.remove();
        } catch (err) {
            console.error("❌ Lỗi khi mark all:", err);
        }
    }

    count = 0;
    badge.textContent = 0;
    alert("✅ Đã đánh dấu tất cả là đã xem!");
});

// =============================
// 🔌 NÚT KẾT NỐI
// =============================
document.getElementById("connectBtn").addEventListener("click", async () => {
    if (connection.state === signalR.HubConnectionState.Disconnected) {
        try {
            await connection.start();
            console.log("✅ Connected to NotificationHub!");
            alert("✅ Connected to NotificationHub!");

            const studentId = prompt("Nhập Student ID để join nhóm:");
            if (studentId) {
                await connection.invoke("JoinGroup", studentId);
                console.log(`✅ Joined group ${studentId}`);
                localStorage.setItem("studentId", studentId);
                await loadUnreadNotifications();
            } else {
                console.warn("⚠️ Không nhập Student ID, chỉ nhận thông báo chung.");
            }
        } catch (err) {
            console.error("❌ Connection failed:", err);
            alert("❌ Connection failed: " + err);
        }
    } else {
        alert("⚠️ Đã kết nối rồi!");
    }
});

// =============================
// 🚀 LOAD TRANG
// =============================
window.onload = async () => {
    if (!checkAccess()) return;

    const studentId = localStorage.getItem("studentId");
    if (studentId) {
        try {
            await connection.start();
            await connection.invoke("JoinGroup", studentId);
            console.log(`✅ Auto-joined group ${studentId}`);
            await loadUnreadNotifications();
        } catch (err) {
            console.error("❌ Không kết nối được SignalR:", err);
        }
    } else {
        console.warn("⚠️ Chưa có studentId, không thể join group!");
    }
};
