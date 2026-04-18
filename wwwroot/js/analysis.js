document.addEventListener("DOMContentLoaded", function () {

    const inputLeft = document.getElementById("imageInputLeft");
    const inputRight = document.getElementById("imageInputRight");

    const startLeft = document.getElementById("startAnalysisLeft");
    const startRight = document.getElementById("startAnalysisRight");

    const typeSelect = document.getElementById("retinopathyType");
    const gradeSelect = document.getElementById("retinopathyGrade");

    let resultadoLeft = "";
    let resultadoRight = "";

    let reporteGenerado = false;

    const OBSERVACIONES = "Observaciones: Este resultado es soporte para la evaluación clínica y debe interpretarse junto con la historia clínica.";

    const LIMITACIONES = "Limitaciones: Si la imagen presenta artefactos la detección puede verse afectada.";


    function autoResize(textarea) {
        textarea.style.height = "auto";
        textarea.style.height = textarea.scrollHeight + "px";
    }


    /* ===============================
       CARGAR IMAGEN
    =============================== */

    function cargarImagen(input, previewId, statusId) {

        input.addEventListener("change", function (e) {

            const file = e.target.files[0];
            if (!file) return;

            const extension = file.name.split(".").pop().toLowerCase();

            if (extension !== "jpg" && extension !== "jpeg") {
                alert("⚠️ Solo se permiten imágenes JPG.");
                input.value = "";
                return;
            }

            const reader = new FileReader();

            reader.onload = function () {

                const preview = document.getElementById(previewId);

                preview.style.backgroundImage = "url(" + reader.result + ")";
                preview.style.backgroundSize = "cover";
                preview.style.backgroundPosition = "center";

                document.getElementById(statusId).innerHTML =
                    "Imagen cargada: " + file.name;

            }

            reader.readAsDataURL(file);

        });
    }

    cargarImagen(inputLeft, "previewLeft", "imageStatusLeft");
    cargarImagen(inputRight, "previewRight", "imageStatusRight");


    /* ===============================
       ANALISIS SIMULADO
    =============================== */

    function analizar(input, resultId, buttonsId, lado) {

        if (reporteGenerado) {
            alert("El análisis ya fue finalizado para esta cita.");
            return;
        }

        if (!input.files || input.files.length === 0) {
            alert("⚠️ Debes cargar una imagen primero.");
            return;
        }

        document.getElementById("loadingSection").style.display = "block";

        setTimeout(function () {

            document.getElementById("loadingSection").style.display = "none";

            const resultadoModelo = "Retinopatía Diabética no Proliferativa con un 92%";

            document.getElementById(resultId).innerHTML = resultadoModelo;

            if (lado === "left") {
                resultadoLeft = resultadoModelo;
            } else {
                resultadoRight = resultadoModelo;
            }

            document.getElementById(buttonsId).style.display = "block";

        }, 3000);
    }

    startLeft.addEventListener("click", function () {
        analizar(inputLeft, "analysisResultLeft", "resultButtonsLeft", "left");
    });

    startRight.addEventListener("click", function () {
        analizar(inputRight, "analysisResultRight", "resultButtonsRight", "right");
    });


    /* ===============================
       ACEPTAR RESULTADO
    =============================== */

    function aceptar(lado, buttonsId, acceptedId, textareaId) {

        document.getElementById(buttonsId).style.display = "none";

        document.getElementById(acceptedId).innerHTML = "✔ Aceptado";

        const resultado = lado === "left" ? resultadoLeft : resultadoRight;

        const textoFinal =
            "Resultado: " + resultado +
            "\n\n" +
            OBSERVACIONES +
            "\n\n" +
            LIMITACIONES;

        const textarea = document.getElementById(textareaId);

        textarea.value = textoFinal;

        autoResize(textarea);
    }

    document.getElementById("acceptLeft").addEventListener("click", function () {
        aceptar("left", "resultButtonsLeft", "acceptedLabelLeft", "summaryLeft");
    });

    document.getElementById("acceptRight").addEventListener("click", function () {
        aceptar("right", "resultButtonsRight", "acceptedLabelRight", "summaryRight");
    });


    /* ===============================
       RETINOPATIA Y GRADOS
    =============================== */

    typeSelect.addEventListener("change", function () {

        const type = this.value;

        gradeSelect.innerHTML = "";

        const defaultOption = document.createElement("option");
        defaultOption.text = "Seleccionar";
        defaultOption.value = "";
        gradeSelect.appendChild(defaultOption);

        if (type === "diabetica") {

            gradeSelect.disabled = false;

            const grados = ["Leve", "Moderada", "Severa", "Proliferativa"];

            grados.forEach(g => {

                const option = document.createElement("option");
                option.text = g;
                option.value = g.toLowerCase();

                gradeSelect.appendChild(option);

            });

        } else {

            gradeSelect.disabled = true;

        }

    });


    /* ===============================
       GENERAR REPORTE PDF
    =============================== */

    document.getElementById("btnReport").addEventListener("click", function () {

        if (reporteGenerado) {
            alert("⚠️ El reporte ya fue generado.");
            return;
        }

        const obsLeft = document.getElementById("obsLeft").value.trim();
        const obsRight = document.getElementById("obsRight").value.trim();

        const imgLeft = document.getElementById("imageInputLeft").files.length > 0;
        const imgRight = document.getElementById("imageInputRight").files.length > 0;

        if (!imgLeft && !imgRight) {
            alert("⚠️ Debes cargar al menos una imagen para generar el reporte.");
            return;
        }

        if (imgLeft && obsLeft === "") {
            alert("⚠️ Debes registrar observaciones para el ojo izquierdo.");
            return;
        }

        if (imgRight && obsRight === "") {
            alert("⚠️ Debes registrar observaciones para el ojo derecho.");
            return;
        }

        const fecha = new Date().toLocaleString();

        document.getElementById("validationInfo").innerHTML =
            "<b>Médico responsable:</b> Médico<br>" +
            "<b>Fecha validación:</b> " + fecha;

        const codigo = document.getElementById("codigoCita").innerText.replace("CITA ", "");
        const fechaArchivo = new Date().toISOString().split("T")[0];

        const element = document.getElementById("reportContent");


        /* ===============================
           OCULTAR BOTONES EN EL PDF
        =============================== */

        document.querySelectorAll(".hide-on-report").forEach(el => {
            el.style.display = "none";
        });


        /* ===============================
           EXPANDIR TEXTAREAS
        =============================== */

        document.querySelectorAll("textarea").forEach(t => {
            t.style.height = "auto";
            t.style.height = t.scrollHeight + "px";
        });


        /* ===============================
           SUPER FIX PDF (EVITA CORTES)
        =============================== */

        document.querySelectorAll("textarea").forEach(t => {

            const div = document.createElement("div");
            div.style.whiteSpace = "pre-wrap";
            div.style.fontSize = "14px";
            div.style.lineHeight = "1.6";
            div.innerText = t.value;

            t.parentNode.replaceChild(div, t);

        });


        const opt = {
            margin: 0.3,
            filename: `Reporte_Cita_${codigo}_${fechaArchivo}.pdf`,
            image: { type: 'jpeg', quality: 1 },
            html2canvas: {
                scale: 4,
                scrollY: 0,
                useCORS: true,
                windowWidth: document.body.scrollWidth
            },
            jsPDF: {
                unit: 'in',
                format: 'letter',
                orientation: 'portrait'
            },
            pagebreak: {
                mode: ['css', 'legacy']
            }
        };

        html2pdf().set(opt).from(element).save();

        reporteGenerado = true;

        document.querySelectorAll(".analyze-btn").forEach(b => b.disabled = true);
        document.querySelectorAll(".file-btn").forEach(b => b.disabled = true);
        document.getElementById("btnReport").disabled = true;

    });


    /* ===============================
       AUTO EXPAND TEXTAREA
    =============================== */

    document.querySelectorAll(".auto-expand").forEach(textarea => {

        textarea.addEventListener("input", function () {

            this.style.height = "auto";
            this.style.height = this.scrollHeight + "px";

        });

    });

    /* ===============================
   ZOOM IMAGENES
=============================== */

    function activarZoom(previewId) {

        const preview = document.getElementById(previewId);
        const overlay = document.getElementById("imageOverlay");

        preview.addEventListener("click", function () {

            if (!preview.style.backgroundImage) return;

            if (!preview.classList.contains("zoomed")) {

                preview.classList.add("zoomed");
                overlay.style.display = "block";

            } else {

                preview.classList.remove("zoomed");
                overlay.style.display = "none";

            }

        });

    }

    activarZoom("previewLeft");
    activarZoom("previewRight");

});