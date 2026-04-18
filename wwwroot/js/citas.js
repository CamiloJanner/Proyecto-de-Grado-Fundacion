document.addEventListener("DOMContentLoaded", function () {

    const errorBox = document.getElementById("errorClinic");

    const buscarBtn = document.getElementById("buscarCitas");
    const limpiarBtn = document.getElementById("limpiarFiltros");
    const consultarBtn = document.getElementById("consultarPacientes");

    const codigo = document.getElementById("filterCodigo");
    const nombre = document.getElementById("filterNombre");
    const apellido = document.getElementById("filterApellido");
    const tipo = document.getElementById("filterTipo");
    const documento = document.getElementById("filterDocumento");

    const filas = document.querySelectorAll("#tablaCitas tr:not(#noResults)");
    const noResults = document.getElementById("noResults");


    function validarPermisoMedico() {

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


    function buscarCitas() {

        const fCodigo = codigo.value.toLowerCase();
        const fNombre = nombre.value.toLowerCase();
        const fApellido = apellido.value.toLowerCase();
        const fTipo = tipo.value.toLowerCase();
        const fDocumento = documento.value.toLowerCase();

        let resultados = 0;

        filas.forEach(fila => {

            const cols = fila.querySelectorAll("td");

            if (cols.length < 7) return;

            const match =
                cols[0].innerText.toLowerCase().includes(fCodigo) &&
                cols[1].innerText.toLowerCase().includes(fNombre) &&
                cols[3].innerText.toLowerCase().includes(fApellido) &&
                cols[5].innerText.toLowerCase().includes(fTipo) &&
                cols[6].innerText.toLowerCase().includes(fDocumento);

            fila.style.display = match ? "" : "none";

            if (match) resultados++;

        });

        if (resultados === 0) {
            noResults.style.display = "";
        } else {
            noResults.style.display = "none";
        }

    }

    buscarBtn.addEventListener("click", buscarCitas);
    consultarBtn.addEventListener("click", buscarCitas);


    document.querySelectorAll(".filters input").forEach(input => {

        input.addEventListener("keypress", function (e) {

            if (e.key === "Enter") {
                buscarCitas();
            }

        });

    });


    limpiarBtn.addEventListener("click", function () {

        codigo.value = "";
        nombre.value = "";
        apellido.value = "";
        tipo.value = "";
        documento.value = "";

        filas.forEach(f => f.style.display = "");

        noResults.style.display = "none";

    });


    function consultarClinicOnline(cita) {

        return new Promise((resolve, reject) => {

            const simulacionError = false;

            setTimeout(() => {

                if (simulacionError) {
                    reject("Error de comunicación con Clinic Online");
                } else {
                    resolve({
                        cita: cita
                    });
                }

            }, 500);

        });

    }


    document.querySelectorAll(".view-btn").forEach(btn => {

        btn.addEventListener("click", function (e) {

            e.preventDefault();

            const cita = this.dataset.cita;

            consultarClinicOnline(cita)

                .then(data => {

                    sessionStorage.setItem("citaSeleccionada", data.cita);

                    window.location.href = "/Analysis/Index";

                })

                .catch(error => {

                    errorBox.innerText = error;
                    errorBox.style.display = "block";

                });

        });

    });

});