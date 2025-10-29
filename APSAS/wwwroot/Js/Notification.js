// =============================
// 🔔 KHAI BÁO BIẾN VÀ KIỂM TRA QUYỀN TRUY CẬP
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

// =============================
// 🧱 HÀM HIỂN THỊ THÔNG BÁO
// =============================
function addNotification(id, title, message, time) {
    const div = document.createElement("div");
    div.className = "notification";
    div.innerHTML = `
        <b>${title}</b><br>
        <p>${(message ?? "(Không có nội dung)").replace(/\n/g, "<br>")}</p>
        <time>${new Date(time).toLocaleString()}</time><br>
        <button class="mark-btn" onclick="markAsRead('${id}', this)">Đã xem</button>
    `;
    notificationsDiv.prepend(div);
    count++;
    badge.textContent = count;

    // 🔔 Hiệu ứng và âm thanh
    bell.classList.add("shake");
    sound.play().catch(() => { });
    setTimeout(() => bell.classList.remove("shake"), 800);
}

// =============================
// ⚙️ KẾT NỐI SIGNALR HUB
// =============================
const HUB_URL = "http://localhost:5216/notificationhub"; // ⚠️ cổng NotificationService

const connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL, {
        accessTokenFactory: () => localStorage.getItem("token")
    })
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect()
    .build();

// =============================
// 📩 LẮNG NGHE SỰ KIỆN SERVER GỬI (REALTIME)
// =============================
connection.on("NotifyNew", (dto) => {
    console.log("📩 Received notification:", dto);
    const id = dto.id ?? dto.Id ?? crypto.randomUUID();
    const title = dto.title ?? dto.Title ?? "(Không có tiêu đề)";
    const message = dto.message ?? dto.Message ?? "(Không có nội dung)";
    const time = dto.createdAtUtc ?? dto.CreatedAtUtc ?? new Date().toISOString();

    // ✅ Chỉ thêm notification mới (IsRead = false)
    addNotification(id, title, message, time);
});

// =============================
// ✅ API: LẤY THÔNG BÁO CHƯA ĐỌC
// =============================
async function loadUnreadNotifications() {
    const studentId = localStorage.getItem("studentId");
    if (!studentId) return;

    try {
        const res = await fetch(`http://localhost:5261/api/Notification/unread?studentId=${studentId}`);
        if (!res.ok) throw new Error("Không thể tải thông báo chưa đọc!");

        const data = await res.json();
        notificationsDiv.innerHTML = "";
        count = 0;

        data.forEach(n => addNotification(n.id, n.title, n.message, n.createdAtUtc));
        console.log(`📬 Loaded ${data.length} unread notifications`);
    } catch (err) {
        console.error("❌ Lỗi load unread:", err);
    }
}

// =============================
// ✅ API: ĐÁNH DẤU ĐÃ XEM
// =============================
async function markAsRead(id, btn) {
    try {
        const res = await fetch(`http://localhost:5261/api/Notification/markasread?id=${id}`, { method: "POST" });
        if (res.ok) {
            btn.parentElement.remove();
            count--;
            badge.textContent = count;
        }
    } catch (err) {
        console.error("❌ Lỗi khi markAsRead:", err);
    }
}

// =============================
// 🔘 NÚT KẾT NỐI HUB
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
                await loadUnreadNotifications(); // ✅ Sau khi join, load unread
            } else {
                console.warn("⚠️ Không nhập Student ID, chỉ nhận thông báo chung (All).");
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
// 🔁 TỰ ĐỘNG RECONNECT
// =============================
connection.onclose(async () => {
    console.warn("⚠️ Mất kết nối SignalR, đang thử kết nối lại...");
    setTimeout(() => startConnection(), 3000);
});

async function startConnection() {
    try {
        await connection.start();
        console.log("✅ Reconnected to NotificationHub!");
    } catch (err) {
        console.error("❌ Reconnect failed:", err);
        setTimeout(startConnection, 5000);
    }
}

// =============================
// 🚀 CHẠY KHI LOAD TRANG
// =============================
window.onload = async () => {
    if (!checkAccess()) return;

    const studentId = localStorage.getItem("studentId");
    if (studentId) {
        await connection.start();
        await connection.invoke("JoinGroup", studentId);
        console.log(`✅ Auto-joined group ${studentId}`);
        await loadUnreadNotifications();
    }
};
