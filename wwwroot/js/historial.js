document.addEventListener("DOMContentLoaded", function () {

    const buscarBtn = document.getElementById("buscarHistorial");
    const limpiarBtn = document.getElementById("limpiarFiltros");
    const consultarBtn = document.getElementById("consultarHistorial");

    const nombre = document.getElementById("filterNombre");
    const fecha = document.getElementById("filterFecha");
    const resultado = document.getElementById("filterResultado");
    const medico = document.getElementById("filterMedico");

    const filas = document.querySelectorAll("#tablaHistorial tr:not(#noResults)");
    const noResults = document.getElementById("noResults");


    function buscarHistorial() {

        const fNombre = nombre.value.toLowerCase();
        const fFecha = fecha.value.toLowerCase();
        const fResultado = resultado.value.toLowerCase();
        const fMedico = medico.value.toLowerCase();

        let resultados = 0;

        filas.forEach(fila => {

            const cols = fila.querySelectorAll("td");

            if (cols.length < 5) return;

            const match =
                cols[1].innerText.toLowerCase().includes(fNombre) &&
                cols[2].innerText.toLowerCase().includes(fFecha) &&
                cols[3].innerText.toLowerCase().includes(fResultado) &&
                cols[4].innerText.toLowerCase().includes(fMedico);

            fila.style.display = match ? "" : "none";

            if (match) resultados++;

        });

        if (resultados === 0) {
            noResults.style.display = "";
        } else {
            noResults.style.display = "none";
        }

    }


    buscarBtn.addEventListener("click", buscarHistorial);
    consultarBtn.addEventListener("click", buscarHistorial);


    document.querySelectorAll(".filters input").forEach(input => {

        input.addEventListener("keypress", function (e) {

            if (e.key === "Enter") {
                buscarHistorial();
            }

        });

    });


    limpiarBtn.addEventListener("click", function () {

        nombre.value = "";
        fecha.value = "";
        resultado.value = "";
        medico.value = "";

        filas.forEach(f => f.style.display = "");

        noResults.style.display = "none";

    });


    document.querySelectorAll(".view-btn").forEach(btn => {

        btn.addEventListener("click", function () {
            return true;
        });

    });

});