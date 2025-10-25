const gatewayUrl = "http://localhost:5261"; // API Gateway URL
let selectedLanguageId = 71; // mặc định Python
let testcaseExample = " ";

const urlParams = new URLSearchParams(window.location.search);
const assignmentId = urlParams.get("id");

// 🟢 Load assignment
async function loadAssignment() {
    if (!assignmentId) {
        document.getElementById("assignmentTitle").innerText = "❌ Không tìm thấy ID bài tập!";
        return;
    }
    try {
        const res = await fetch(`${gatewayUrl}/GetAssignmentById/${assignmentId}`);
        if (!res.ok) throw new Error("Không tải được bài tập");
        const data = await res.json();

        document.getElementById("assignmentId").value = data.assignmentId;
        document.getElementById("assignmentTitle").innerText = `🧪 ${data.title}`;
        document.getElementById("assignmentDescription").innerText = data.description || "Không có mô tả.";
        document.getElementById("assignmentSampleTestCase").innerText = data.sampleTestCase || "khoong co testcase";
        testcaseExample = data.sampleTestCase;
        document.getElementById("assignmentDifficulty").innerText = data.difficulty || "Không xác định";
        const date = new Date(data.deadline);
        document.getElementById("assignmentDeadline").innerText = date.toLocaleDateString("vi-VN");
    } catch (err) {
        document.getElementById("assignmentTitle").innerText = "❌ Lỗi: " + err.message;
    }
}// done


// 🟢 select language
document.querySelectorAll(".lang-btn").forEach((btn) => {
    btn.addEventListener("click", () => {
        document.querySelectorAll(".lang-btn").forEach((b) => b.classList.remove("active"));
        btn.classList.add("active");
        selectedLanguageId = parseInt(btn.getAttribute("data-id"));
    });
});// done

// 🟢 Submit code
async function submitCode() {
    const id = document.getElementById("assignmentId").value;
    const body = {
        assignmentId: parseInt(id),
        sourceCode: document.getElementById("codeArea").value,
        languageId: selectedLanguageId
    };

    try {
        const res = await fetch(`${gatewayUrl}/Submit`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });
        if (res.ok) {

            alert("Nộp bài thành công!");
            window.location.href = `/Dashboard.html`;
            return;
        }

        const data = await res.json();
        document.getElementById("result").innerText = JSON.stringify(data, null, 2);
    } catch (err) {
        document.getElementById("result").innerText = "❌ " + err.message;
    }
}// done

// 🟢 Run code
async function runCode() {
    const id = document.getElementById("assignmentId").value;
    const body = {
        assignmentId: parseInt(id),
        sourceCode: document.getElementById("codeArea").value,
        languageId: selectedLanguageId,
        stdin:""
    };

    try {
        const res = await fetch(`${gatewayUrl}/RunCode`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });

        if (!res.ok) throw new Error("Run failed, status " + res.status);

        const result = await res.json();
        const formatted = `
🟢 Status: ${result.status}
⏱️ Time: ${result.executionTime}
💾 Memory: ${result.memoryUsed} KB
📤 Output:
${result.output}

⚠️ Error Message:
${result.errorMessage}
        `;
        document.getElementById("result").innerText = formatted;
    } catch (err) {
        document.getElementById("result").innerText = "❌ " + err.message;
    }
}


// 🟢 Run code with Sample Testcase
async function runWithTestcase() {
    const id = document.getElementById("assignmentId").value;

    const body = {
        assignmentId: parseInt(id),
        sourceCode: document.getElementById("codeArea").value,
        languageId: selectedLanguageId,
        stdin: testcaseExample
    };

    try {
        const res = await fetch(`${gatewayUrl}/RunCode`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });

        if (!res.ok) throw new Error("Run with sample testcase failed: " + res.status);

        const result = await res.json();
        const formatted = `
🧪 Sample Testcase Run
🟢 Status: ${result.status}
⏱️ Time: ${result.executionTime}
💾 Memory: ${result.memoryUsed} KB
📤 Output:
${result.output}

⚠️ Error Message:
${result.errorMessage}
        `;
        document.getElementById("result").innerText = formatted;
    } catch (err) {
        document.getElementById("result").innerText = "❌ " + err.message;
    }
}


loadAssignment();
