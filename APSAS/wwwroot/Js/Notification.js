// bien toan cuc
const bellContainer = document.getElementById("bellContainer");
const notificationsPopup = document.getElementById("notificationsPopup");
const notificationsDiv = document.getElementById("notifications");
const badge = document.getElementById("badge");
const bell = document.getElementById("bell");
const sound = document.getElementById("notifSound");
let count = 0;

//kiem tra token
function checkAccess() {
    const token = localStorage.getItem("token");
    if (!token) {
        window.location.href = "Login.html";
        return false;
    }
    return true;
}

// hien thi thong bao moi
function addNotification(id, title, message, time) {
    const noMsg = document.getElementById("noNotificationMsg");
    if (noMsg) noMsg.remove();

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
    badge.style.display = "block";

    bell.classList.add("shake");
    sound.play().catch(() => { });
    setTimeout(() => bell.classList.remove("shake"), 800);
}

// cau hinh SignalR va API
const HUB_URL = "http://localhost:5216/notificationhub";
const API_BASE = "http://localhost:5261/api/Notification";

// connect to signalR
let connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL, {
        accessTokenFactory: () => localStorage.getItem("token")
    })
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect()
    .build();

connection.on("NotifyNew", (dto) => {
    console.log("📩 Realtime:", dto);
    addNotification(
        dto.id ?? dto.Id,
        dto.title ?? dto.Title,
        dto.message ?? dto.Message,
        dto.createdAtUtc ?? dto.CreatedAtUtc
    );
});

// load thong bao chua doc
async function loadUnreadNotifications() {
    try {
        const token = localStorage.getItem("token");

        const res = await fetch(`${API_BASE}/unread`, {
            headers: { "Authorization": `Bearer ${token}` }
        });

        if (!res.ok) throw new Error("Lỗi API unread");

        const data = await res.json();
        notificationsDiv.innerHTML = "";
        count = 0;

        if (data.length === 0) {
            notificationsDiv.innerHTML = `<p id="noNotificationMsg">Không có thông báo chưa đọc.</p>`;
            badge.style.display = "none";
            return;
        }

        data.forEach(n => addNotification(n.id, n.title, n.message, n.createdAtUtc));

    } catch (err) {
        console.error("❌ Lỗi load unread:", err);
    }
}

// xem thong bao
async function markAsRead(id, btn) {
    try {
        const token = localStorage.getItem("token");

        await fetch(`${API_BASE}/markasread?id=${id}`, {
            method: "POST",
            headers: { "Authorization": `Bearer ${token}` }
        });

        const div = btn.closest(".notification");
        if (div) div.remove();

        count--;
        badge.textContent = Math.max(count, 0);

        if (count <= 0) {
            badge.style.display = "none";
            notificationsDiv.innerHTML =
                '<p id="noNotificationMsg">Không có thông báo chưa đọc.</p>';
        }
    } catch (err) {
        console.error("❌ markAsRead error:", err);
    }
}

document.getElementById("markAllBtn").addEventListener("click", async () => {
    const token = localStorage.getItem("token");
    const all = document.querySelectorAll(".notification");

    if (all.length === 0) {
        alert("Không còn thông báo chưa đọc.");
        return;
    }

    for (const n of all) {
        const id = n.dataset.id;
        await fetch(`${API_BASE}/markasread?id=${id}`, {
            method: "POST",
            headers: { "Authorization": `Bearer ${token}` }
        });
        n.remove();
    }

    count = 0;
    badge.style.display = "none";
    notificationsDiv.innerHTML =
        '<p id="noNotificationMsg">Không có thông báo chưa đọc.</p>';
});

// pop up cai chuong
bell.addEventListener("click", (e) => {
    e.stopPropagation();
    notificationsPopup.classList.toggle("visible");
});

document.addEventListener("click", (e) => {
    if (notificationsPopup.classList.contains("visible") &&
        !bellContainer.contains(e.target)) {
        notificationsPopup.classList.remove("visible");
    }
});

// auto ket noi lai neu mat ket noi
async function startSignalR() {
    try {
        await connection.start();
        console.log("🔌 SignalR connected");
    } catch (err) {
        console.error("❌ Cannot connect SignalR:", err);
        setTimeout(startSignalR, 2000);
    }
}

window.addEventListener("DOMContentLoaded", async () => {
    if (!checkAccess()) return;

    await startSignalR();
    await loadUnreadNotifications();
});
