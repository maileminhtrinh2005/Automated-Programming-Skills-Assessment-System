const gatewayUrl = "http://localhost:5261";
const loginEndpoint = "/Login";

const $ = id => document.getElementById(id);
const showMessage = (el, text) => el.textContent = text;

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
        // trinh them
        if (data.roleName === "Student") {
            
            let studentId =
                data.studentId ??
                data.id ??
                data.userId ??
                data.user_id ??
                data.user?.id ??
                data.user?.studentId;

            
            if (!studentId && /^\d+$/.test(data.username)) {
                studentId = data.username;
            }

            if (studentId) {
                localStorage.setItem("studentId", studentId);
                console.log("✅ Đã lưu studentId =", studentId);
            } else {
                console.warn("⚠️ Không thấy studentId trong response:", data);
            }
        }
        await new Promise(r => setTimeout(r, 500));
        console.log("✅ Đã lưu studentId, chuẩn bị điều hướng...");
        // hết trinh them

        // ✅ Điều hướng theo role
        setTimeout(() => {
            switch (data.roleName) {
                case "Admin": window.location.href = "Adminpage.html"; break;
                case "Lecturer": window.location.href = "DashboardLecturer.html"; break;
                case "Student": window.location.href = "StudentDashboard.html"; break;
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

// ==== Nút hỗ trợ ====
$("supportBtn").addEventListener("click", () => {
    window.location.href = "Supportpage.html"; // chuyển sang trang hỗ trợ
});
