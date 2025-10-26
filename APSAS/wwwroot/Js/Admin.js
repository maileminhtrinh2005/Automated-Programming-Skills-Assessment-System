const GATEWAY_URL = "http://localhost:5261";
const ADMIN_HUB_URL = "http://localhost:5097/chathub";
const $ = id => document.getElementById(id);

function showMessage(el, text, kind = "info") {
    el.textContent = text;
    el.className = "message " + (kind === "success" ? "success" : kind === "error" ? "error" : "");
}

// ========== API CONFIG ==========
function addAPIConfig() { alert("Chức năng thêm API (chưa implement)."); }
function toggleApiForm(mode) {
    $("addApiForm").style.display = mode === 'add' ? 'block' : 'none';
    $("updateApiForm").style.display = mode === 'update' ? 'block' : 'none';
}
function loadAPIList() { alert("Chức năng hiển thị API list (chưa implement)."); }
function updateAPIConfig() { alert("Chức năng update API (chưa implement)."); }

// ========== USER MANAGEMENT ==========
window._usersMap = {};

document.getElementById("submitUserBtn").addEventListener("click", async () => {
    const username = $("username").value.trim();
    const email = $("email").value.trim();
    const fullname = $("fullname").value.trim();
    const password = $("password").value.trim();
    const roleid = parseInt($("roleid").value);
    const msg = $("userMessage");

    if (!username || !email || !password) {
        showMessage(msg, "⚠️ Vui lòng nhập đủ thông tin!", "error");
        return;
    }

    try {
        const res = await fetch(`${GATEWAY_URL}/AddUser`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                username,
                email,
                fullName: fullname,
                passwordHash: password,
                roleID: roleid
            })
        });

        if (!res.ok) throw new Error(`${res.status} ${res.statusText}`);
        showMessage(msg, "✅ Thêm người dùng thành công!", "success");
        $("username").value = $("email").value = $("fullname").value = $("password").value = "";
        loadUsers();
    } catch (err) {
        showMessage(msg, `🚫 ${err.message}`, "error");
    }
});

async function loadUsers() {
    const container = $("userTableContainer");
    container.innerHTML = "<p>⏳ Đang tải danh sách người dùng...</p>";
    window._usersMap = {};

    try {
        const res = await fetch(`${GATEWAY_URL}/GetAllUsers`);
        if (!res.ok) throw new Error(`${res.status} ${res.statusText}`);
        const users = await res.json();

        users.forEach(u => window._usersMap[u.userID] = u);

        let html = `
            <table>
                <tr>
                    <th>ID</th><th>Username</th><th>Email</th><th>Full Name</th><th>Role</th><th>Actions</th>
                </tr>`;
        users.forEach(u => {
            html += `
                <tr>
                    <td>${u.userID}</td>
                    <td>${u.username}</td>
                    <td>${u.email || ''}</td>
                    <td>${u.fullName || ''}</td>
                    <td>${u.roleName || u.roleID || ''}</td>
                    <td>
                        <button class="btn-edit" onclick="openEditModal(${u.userID})">Sửa</button>
                        <button class="btn-delete" onclick="deleteUser(${u.userID})">Xóa</button>
                    </td>
                </tr>`;
        });
        html += "</table>";
        container.innerHTML = html;
    } catch (err) {
        container.innerHTML = `<p style="color:red;">🚫 ${err.message}</p>`;
    }
}

function openEditModal(userId) {
    const u = window._usersMap[userId];
    if (!u) return alert("Không tìm thấy user để sửa.");
    $("editUserId").value = u.userID;
    $("editUsername").value = u.username || "";
    $("editEmail").value = u.email || "";
    $("editFullname").value = u.fullName || "";
    $("editPassword").value = "";
    $("editRole").value = u.roleID || u.roleId || 3;
    $("editOverlay").style.display = "flex";
}

function closeEditModal() { $("editOverlay").style.display = "none"; }

async function confirmUpdateUser() {
    const id = parseInt($("editUserId").value);
    const username = $("editUsername").value.trim();
    const email = $("editEmail").value.trim();
    const fullName = $("editFullname").value.trim();
    const password = $("editPassword").value.trim();
    const roleID = parseInt($("editRole").value);
    const msg = $("editMessage");

    if (!username || !email) {
        showMessage(msg, "⚠️ Username và Email không được để trống.", "error");
        return;
    }

    const payload = { userID: id, username, email, fullName, roleID };
    if (password) payload.passwordHash = password;

    try {
        const res = await fetch(`${GATEWAY_URL}/UpdateUser`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });
        if (!res.ok) throw new Error(`${res.status} ${res.statusText}`);
        showMessage(msg, "✅ Cập nhật thành công!", "success");
        setTimeout(() => { closeEditModal(); loadUsers(); }, 600);
    } catch (err) {
        showMessage(msg, `🚫 ${err.message}`, "error");
    }
}

async function deleteUser(userId) {
    if (!confirm("Bạn có chắc muốn xóa user này?")) return;
    try {
        const res = await fetch(`${GATEWAY_URL}/DeleteUser/${userId}`, { method: "DELETE" });
        if (!res.ok) throw new Error(`${res.status} ${res.statusText}`);
        alert("✅ Xóa thành công!");
        loadUsers();
    } catch (err) {
        alert("🚫 Lỗi xóa user: " + err.message);
    }
}

function confirmDeleteFromModal() {
    const id = parseInt($("editUserId").value);
    if (!id) return alert("ID user không hợp lệ.");
    if (!confirm("Bạn có chắc muốn xóa user này?")) return;
    deleteUser(id);
    closeEditModal();
}

// Tự động tải danh sách user khi load trang
window.onload = loadUsers;
