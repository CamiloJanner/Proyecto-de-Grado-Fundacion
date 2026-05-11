document.addEventListener("DOMContentLoaded", function () {

    const errorBox = document.getElementById("errorClinic");

    function validarPermisoMedico() {

        /*
         * Temporal:
         * Por ahora se deja fijo como "medico".
         * Más adelante lo ideal es validar permisos desde backend con el rol real del usuario autenticado.
         */
        const rolUsuario = "medico";

        if (rolUsuario !== "medico") {

            errorBox.innerText = "No tiene permisos para consultar información clínica.";
            errorBox.style.display = "block";

            document.querySelectorAll(".view-btn").forEach(btn => {
                btn.style.display = "none";
            });
        }
    }

    validarPermisoMedico();

});