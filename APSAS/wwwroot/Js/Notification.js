// =============================
// 🔔 BIẾN TOÀN CỤC
// =============================
const bellContainer = document.getElementById("bellContainer");
const notificationsPopup = document.getElementById("notificationsPopup");
const notificationsDiv = document.getElementById("notifications");
const badge = document.getElementById("badge");
const bell = document.getElementById("bell");
const sound = document.getElementById("notifSound");

let count = 0;
let currentGroup = null;

// =============================
// ⚙️ KIỂM TRA TRUY CẬP
// =============================
function checkAccess() {
    const token = localStorage.getItem("token");
    if (!token) {
        window.location.href = "Login.html";
        return false;
    }
    return true;
}

// =============================
// 📩 HIỂN THỊ THÔNG BÁO
// =============================
function addNotification(id, title, message, time) {
    const noNotificationMsg = document.getElementById("noNotificationMsg");
    if (noNotificationMsg) noNotificationMsg.remove();

    const div = document.createElement("div");
    div.className = "notification";
    div.dataset.id = id;

    div.innerHTML = `
        <b>${title}</b><br>
        <p>${(message ?? "(Không có nội dung)").replace(/\n/g, "<br>")}</p>
        <time>${new Date(time).toLocaleString()}</time><br>
        <button class="mark-btn" onclick="markAsRead('${id}', this)">✅ Đã xem</button>
    `;
    notificationsDiv.prepend(div);

    count++;
    badge.textContent = count;
    badge.style.display = 'block';

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
// 🔌 KẾT NỐI SIGNALR
// =============================
const connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL)
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect()
    .build();

// Nhận thông báo mới
connection.on("NotifyNew", (dto) => {
    console.log("📩 Received notification:", dto);
    const id = dto.id ?? crypto.randomUUID();
    const title = dto.title ?? "(Không có tiêu đề)";
    const message = dto.message ?? "(Không có nội dung)";
    const time = dto.createdAtUtc ?? new Date().toISOString();
    addNotification(id, title, message, time);
});

// =============================
// 🔌 JOIN GROUP THEO USERID
// =============================
async function joinSignalRGroup(userId) {
    if (!userId) return;
    if (currentGroup === userId) return; // đã join

    if (connection.state === signalR.HubConnectionState.Connected) {
        await connection.stop();
    }

    await connection.start();
    await connection.invoke("JoinGroup", userId.toString());
    currentGroup = userId;
    console.log(`✅ Joined SignalR group by userId ${userId}`);

    await loadUnreadNotifications(userId);
}

// =============================
// 📤 LOAD THÔNG BÁO CHƯA ĐỌC
// =============================
async function loadUnreadNotifications(userId) {
    if (!userId) return console.warn("⚠️ Không có userId!");

    try {
        const token = localStorage.getItem("token");
        const res = await fetch(`${API_BASE}/unread?userId=${userId}`, {
            headers: token ? { "Authorization": `Bearer ${token}` } : {}
        });

        if (!res.ok) throw new Error(`Server error ${res.status}`);
        const data = await res.json();

        notificationsDiv.innerHTML = "";
        count = 0;

        if (!data.length) {
            notificationsDiv.innerHTML = '<p id="noNotificationMsg">Không có thông báo chưa đọc.</p>';
            badge.textContent = 0;
            badge.style.display = 'none';
            return;
        }

        data.forEach(n => addNotification(n.id, n.title, n.message, n.createdAtUtc));
        console.log(`📬 Loaded ${data.length} unread notifications`);
    } catch (err) {
        console.error("❌ Lỗi load unread:", err);
        notificationsDiv.innerHTML = '<p id="noNotificationMsg">Lỗi khi tải thông báo.</p>';
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
            if (count === 0) {
                badge.style.display = 'none';
                notificationsDiv.innerHTML = '<p id="noNotificationMsg">Không có thông báo chưa đọc.</p>';
            }
        }
    } catch (err) {
        console.error("❌ Lỗi khi markAsRead:", err);
    }
}

// =============================
// ✅ MARK ALL
// =============================
document.getElementById("markAllBtn").addEventListener("click", async () => {
    const all = document.querySelectorAll(".notification");
    if (!all.length) return alert("✅ Không còn thông báo chưa đọc.");

    const token = localStorage.getItem("token");
    const ids = Array.from(all).map(n => n.dataset.id);

    try {
        for (const id of ids) {
            await fetch(`${API_BASE}/markasread?id=${id}`, {
                method: "POST",
                headers: token ? { "Authorization": `Bearer ${token}` } : {}
            });
        }

        notificationsDiv.innerHTML = '<p id="noNotificationMsg">Không có thông báo chưa đọc.</p>';
        count = 0;
        badge.textContent = 0;
        badge.style.display = 'none';
    } catch (err) {
        console.error("❌ Lỗi khi mark all:", err);
    }
});

// =============================
// 🧭 LOGIC MỞ/ĐÓNG POPUP
// =============================
bell.addEventListener("click", (event) => {
    event.stopPropagation();
    notificationsPopup.classList.toggle("visible");
});

document.addEventListener("click", (event) => {
    if (notificationsPopup.classList.contains("visible") && !bellContainer.contains(event.target)) {
        notificationsPopup.classList.remove("visible");
    }
});

// =============================
// 🔌 AUTO KẾT NỐI KHI LOAD
// =============================
window.addEventListener("DOMContentLoaded", async () => {
    if (!checkAccess()) return;

    const userId = localStorage.getItem("userId");
    if (!userId) {
        console.warn("⚠️ Không tìm thấy userId trong localStorage, bỏ qua SignalR join.");
        return;
    }

    await joinSignalRGroup(userId);
});
