document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("loginForm");
    const toggles = document.querySelectorAll(".password-toggle");
    const submitBtn = document.querySelector(".auth-submit");

    toggles.forEach(btn => {
        btn.addEventListener("click", function () {
            const targetId = this.getAttribute("data-target");
            const input = document.getElementById(targetId);

            if (!input) return;

            if (input.type === "password") {
                input.type = "text";
                this.textContent = "Ocultar";
            } else {
                input.type = "password";
                this.textContent = "Ver";
            }
        });
    });

    if (form) {
        form.addEventListener("submit", function () {
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.textContent = "Validando...";
            }
        });
    }
});