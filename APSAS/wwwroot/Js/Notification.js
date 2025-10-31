// =============================
// 🔔 BIẾN TOÀN CỤC
// =============================
const notificationsDiv = document.getElementById("notifications");
const badge = document.getElementById("badge");
const bell = document.getElementById("bell");
const sound = document.getElementById("notifSound");
let count = 0;

// =============================
// ⚙️ KIỂM TRA TRUY CẬP
// =============================
function checkAccess() {
    const token = localStorage.getItem("token");
    if (!token) {
        window.location.href = "DN.html";
        return false;
    }
    return true;
}

// =============================
// 📩 HIỂN THỊ THÔNG BÁO
// =============================
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
const HUB_URL = "http://localhost:5216/notificationhub";
const API_BASE = "http://localhost:5261/api/Notification";

// =============================
// ⚙️ KẾT NỐI SIGNALR
// =============================
const connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL, { accessTokenFactory: () => localStorage.getItem("token") })
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect()
    .build();

// Nhận thông báo mới
connection.on("NotifyNew", (dto) => {
    console.log("📩 Received notification:", dto);
    const id = dto.id ?? dto.Id ?? crypto.randomUUID();
    const title = dto.title ?? dto.Title ?? "(Không có tiêu đề)";
    const message = dto.message ?? dto.Message ?? "(Không có nội dung)";
    const time = dto.createdAtUtc ?? dto.CreatedAtUtc ?? new Date().toISOString();
    addNotification(id, title, message, time);
});

// =============================
// 📤 LOAD THÔNG BÁO CHƯA ĐỌC
// =============================
async function loadUnreadNotifications() {
    const studentId = localStorage.getItem("studentId");
    if (!studentId) return console.warn("⚠️ Không có studentId!");

    try {
        const token = localStorage.getItem("token");
        const res = await fetch(`${API_BASE}/unread?studentId=${studentId}`, {
            headers: token ? { "Authorization": `Bearer ${token}` } : {}
        });
        if (!res.ok) throw new Error(`Server error ${res.status}`);

        const data = await res.json();
        notificationsDiv.innerHTML = "";
        if (!data.length) {
            notificationsDiv.innerHTML = "<p>Không có thông báo chưa đọc.</p>";
            badge.textContent = 0;
            return;
        }
        count = 0;
        data.forEach(n => addNotification(n.id, n.title, n.message, n.createdAtUtc));
        console.log(`📬 Loaded ${data.length} unread notifications`);
    } catch (err) {
        console.error("❌ Lỗi load unread:", err);
    }
}

// =============================
// ✅ MARK AS READ
// =============================
async function markAsRead(id, btn) {
    try {
        const token = localStorage.getItem("token");
        const res = await fetch(`${API_BASE}/markasread?id=${id}`, {
            method: "POST",
            headers: token ? { "Authorization": `Bearer ${token}` } : {}
        });
        if (res.ok) {
            btn.parentElement.remove();
            count--;
            badge.textContent = Math.max(count, 0);
        }
    } catch (err) {
        console.error("❌ Lỗi khi markAsRead:", err);
    }
}

// ✅ MARK ALL
document.getElementById("markAllBtn").addEventListener("click", async () => {
    const token = localStorage.getItem("token");
    const all = document.querySelectorAll(".notification");
    if (!all.length) return alert("✅ Không còn thông báo chưa đọc.");

    for (const n of all) {
        const id = n.dataset.id;
        await fetch(`${API_BASE}/markasread?id=${id}`, {
            method: "POST",
            headers: token ? { "Authorization": `Bearer ${token}` } : {}
        });
        n.remove();
    }
    count = 0;
    badge.textContent = 0;
});

// =============================
// 🔌 AUTO KẾT NỐI KHI LOAD
// =============================
window.addEventListener("DOMContentLoaded", async () => {
    if (!checkAccess()) return;

    const studentId = localStorage.getItem("studentId");
    if (!studentId) {
        console.warn("⚠️ Chưa có studentId, chưa join group!");
        return;
    }

    try {
        await connection.start();
        await connection.invoke("JoinGroup", studentId);
        console.log(`✅ Joined SignalR group ${studentId}`);
        await loadUnreadNotifications();
    } catch (err) {
        console.error("❌ Không kết nối được SignalR:", err);
    }
});

// =============================
// 🧭 MỞ POPUP KHI BẤM CHUÔNG
// =============================
const popup = document.getElementById("notificationsPopup");
bell.addEventListener("click", () => popup.classList.toggle("visible"));
