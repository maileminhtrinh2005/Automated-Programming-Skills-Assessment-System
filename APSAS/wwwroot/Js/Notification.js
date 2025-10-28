const notificationsDiv = document.getElementById("notifications");
const badge = document.getElementById("badge");
const bell = document.getElementById("bell");
const sound = document.getElementById("notifSound");
let count = 0;

// =============================
// 🧱 HÀM HIỂN THỊ THÔNG BÁO
// =============================
function addNotification(title, message, time) {
    const div = document.createElement("div");
    div.className = "notification";
    div.innerHTML = `
        <b>${title}</b><br>
        <p>${(message ?? "(Không có nội dung)").replace(/\n/g, "<br>")}</p>
        <time>${new Date(time).toLocaleString()}</time>
    `;
    notificationsDiv.prepend(div);
    count++;
    badge.textContent = count;

    // 🔔 Rung chuông và phát âm thanh
    bell.classList.add("shake");
    sound.play().catch(() => { });
    setTimeout(() => bell.classList.remove("shake"), 800);
}

// =============================
// ⚙️ KẾT NỐI SIGNALR HUB
// =============================
const HUB_URL = "http://localhost:5216/notificationhub"; // ⚠️ cổng NotificationService

const connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL, { withCredentials: true })
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect()
    .build();

// =============================
// 📩 LẮNG NGHE SỰ KIỆN SERVER GỬI
// =============================
connection.on("NotifyNew", (dto) => {
    console.log("📩 Received notification:", dto);

    // ✅ Hỗ trợ cả camelCase & PascalCase
    const title = dto.title ?? dto.Title ?? "(Không có tiêu đề)";
    const message = dto.message ?? dto.Message ?? "(Không có nội dung)";
    const time = dto.createdAtUtc ?? dto.CreatedAtUtc ?? new Date().toISOString();

    addNotification(title, message, time);
});

// =============================
// 🔘 NÚT KẾT NỐI HUB
// =============================
document.getElementById("connectBtn").addEventListener("click", async () => {
    if (connection.state === signalR.HubConnectionState.Disconnected) {
        try {
            await connection.start();
            console.log("✅ Connected to NotificationHub!");
            alert("✅ Connected to NotificationHub!");

            // ✅ Cho phép nhập StudentId để join nhóm
            const studentId = prompt("Nhập Student ID để join nhóm:");
            if (studentId) {
                await connection.invoke("JoinGroup", studentId);
                console.log(`✅ Joined group ${studentId}`);
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
// 🔁 TỰ ĐỘNG RECONNECT KHI MẤT KẾT NỐI
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
