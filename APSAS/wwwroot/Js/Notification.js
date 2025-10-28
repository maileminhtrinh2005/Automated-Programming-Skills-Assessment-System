const notificationsDiv = document.getElementById("notifications");
const badge = document.getElementById("badge");
let count = 0;

// =============================
// 🧱 HÀM HIỂN THỊ THÔNG BÁO
// =============================
function addNotification(title, message, time) {
    const div = document.createElement("div");
    div.className = "notification";
    div.innerHTML = `
        <b>${title}</b><br>
        ${message}<br>
        <time>${new Date(time).toLocaleString()}</time>
    `;
    notificationsDiv.prepend(div);
    count++;
    badge.textContent = count;
}

// =============================
// ⚙️ KẾT NỐI SIGNALR HUB
// =============================
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5261/notificationhub") // Gateway port (5261)
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Lắng nghe sự kiện server gửi
connection.on("NotifyNew", (dto) => {
    console.log("📩 Received notification:", dto);
    addNotification(dto.title, dto.message, dto.createdAtUtc);
});

// =============================
// 🔘 NÚT KẾT NỐI HUB
// =============================
document.getElementById("connectBtn").addEventListener("click", async () => {
    try {
        // ❗Chỉ kết nối khi đang ở trạng thái Disconnected
        if (connection.state === signalR.HubConnectionState.Disconnected) {
            await connection.start();
            console.log("✅ Connected to NotificationHub!");
            alert("✅ Connected to NotificationHub!");

            // ✅ Hỏi Student ID để join đúng group
            const studentId = prompt("Nhập Student ID để join nhóm:");
            if (studentId) {
                await connection.invoke("JoinGroup", studentId);
                console.log(`✅ Joined group ${studentId}`);
            } else {
                console.warn("⚠️ Không nhập Student ID, sẽ không nhận thông báo cá nhân.");
            }
        } else {
            console.warn("⚠️ Đã kết nối rồi, không cần connect lại!");
            alert("⚠️ Đã kết nối rồi!");
        }

    } catch (err) {
        console.error("❌ Connection failed:", err);
        alert("❌ Connection failed: " + err);
    }
});
