const gatewayUrl = "http://localhost:5261";
const loginEndpoint = "/Login";
const changePwdEndpoint = "/ChangePassword";

const $ = id => document.getElementById(id);
const showMessage = (el, text) => el.textContent = text;
const switchView = view => {
    $("loginCard").classList.toggle("hidden", view !== "login");
    $("changeCard").classList.toggle("hidden", view !== "change");
};

// ==== LOGIN ====
$("loginBtn").addEventListener("click", async (e) => {
    e.preventDefault();
    const username = $("username").value.trim();
    const password = $("password").value;
    const msg = $("loginMsg");

    if (!username || !password)
        return showMessage(msg, "⚠️ Vui lòng nhập username và mật khẩu.");

    $("loginBtn").disabled = true;
    $("loginBtn").textContent = "Đang đăng nhập...";

    try {
        const res = await fetch(gatewayUrl + loginEndpoint, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ username, password })
        });

        const data = await res.json();
        if (!res.ok) throw new Error(data.message || "Sai thông tin đăng nhập.");

        // ✅ Lưu thông tin
        localStorage.setItem("token", data.token);
        localStorage.setItem("role", data.roleName);
        localStorage.setItem("username", data.username);

        // ✅ Điều hướng theo role
        setTimeout(() => {
            switch (data.roleName) {
                case "Admin": window.location.href = "Adminpage.html"; break;
                case "Lecturer": window.location.href = "DashboardLecturer.html"; break;
                case "Student": window.location.href = "Dashboard.html"; break;
                default:
                    localStorage.clear();
                    showMessage(msg, "❌ Tài khoản không được phép truy cập.");
            }
        }, 300);
    } catch (err) {
        showMessage(msg, err.message);
    } finally {
        $("loginBtn").disabled = false;
        $("loginBtn").textContent = "Đăng nhập";
    }
});

// ==== ĐỔI MẬT KHẨU ====
$("toChangePwdBtn").addEventListener("click", () => {
    $("cpUsername").value = $("username").value.trim();
    switchView("change");
});

$("backToLoginBtn").addEventListener("click", () => switchView("login"));

$("changeBtn").addEventListener("click", async (e) => {
    e.preventDefault();
    const username = $("cpUsername").value.trim();
    const oldPassword = $("oldPassword").value;
    const newPassword = $("newPassword").value;
    const msg = $("changeMsg");

    if (!username || !oldPassword || !newPassword)
        return showMessage(msg, "⚠️ Vui lòng nhập đầy đủ thông tin.");

    $("changeBtn").disabled = true;
    $("changeBtn").textContent = "Đang xử lý...";

    try {
        const res = await fetch(gatewayUrl + changePwdEndpoint, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ Username: username, OldPassword: oldPassword, NewPassword: newPassword })
        });

        const data = await res.json();
        if (!res.ok) throw new Error(data.message || "Đổi mật khẩu thất bại.");

        showMessage(msg, "✅ Đã cập nhật mật khẩu mới!");
        setTimeout(() => switchView("login"), 1200);
    } catch (err) {
        showMessage(msg, err.message);
    } finally {
        $("changeBtn").disabled = false;
        $("changeBtn").textContent = "Cập nhật mật khẩu";
    }
});
