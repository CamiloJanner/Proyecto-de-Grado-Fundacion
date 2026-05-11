document.addEventListener("DOMContentLoaded", function () {

    const inputLeft = document.getElementById("imageInputLeft");
    const inputRight = document.getElementById("imageInputRight");

    const startLeft = document.getElementById("startAnalysisLeft");
    const startRight = document.getElementById("startAnalysisRight");

    const typeSelect = document.getElementById("retinopathyType");
    const gradeSelect = document.getElementById("retinopathyGrade");

    const typeSelectSecond = document.getElementById("retinopathyTypeSecond");
    const gradeSelectSecond = document.getElementById("retinopathyGradeSecond");

    const validationRowPrimary = document.getElementById("validationRowPrimary");
    const validationRowSecondary = document.getElementById("validationRowSecondary");
    const eyeLabelPrimary = document.getElementById("eyeLabelPrimary");
    const eyeLabelSecondary = document.getElementById("eyeLabelSecondary");
    const addSecondEyeWrapper = document.getElementById("addSecondEyeWrapper");
    const btnAddSecondEye = document.getElementById("btnAddSecondEye");

    const previewLeft = document.getElementById("previewLeft");
    const previewRight = document.getElementById("previewRight");

    const overlay = document.getElementById("imageOverlay");

    const antiforgeryToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value || "";
    const analyzeEndpoint = `${window.location.pathname}?handler=Analyze`;
    const guardarReporteEndpoint = `${window.location.pathname}?handler=GuardarReporte`;

    let confidenceLeft = null;
    let confidenceRight = null;

    let probabilityPositiveLeft = null;
    let probabilityPositiveRight = null;

    let probabilityNegativeLeft = null;
    let probabilityNegativeRight = null;

    let diabeticMetricsLeft = null;
    let diabeticMetricsRight = null;

    let hypertensiveMetricsLeft = null;
    let hypertensiveMetricsRight = null;

    const METRICAS_VALIDACION_DIABETICA = {
        f1: 0.8429,
        recall: 0.8469,
        rocAuc: 0.9703
    };

    let overlayZoomImage = overlay.querySelector(".overlay-zoom-image");
    if (!overlayZoomImage) {
        overlayZoomImage = document.createElement("div");
        overlayZoomImage.className = "overlay-zoom-image";
        overlay.appendChild(overlayZoomImage);
    }

    let resultadoLeft = "";
    let resultadoRight = "";

    let reporteGenerado = false;

    let ojosAceptados = [];
    let segundaFilaAgregada = false;

    const OBSERVACIONES = "Este resultado es soporte para la evaluación clínica y debe interpretarse junto con la historia clínica.";
    const LIMITACIONES = "Si la imagen presenta artefactos la detección puede verse afectada.";

    function autoResize(textarea) {
        textarea.style.height = "auto";
        textarea.style.height = textarea.scrollHeight + "px";
    }

    function obtenerCitaIdActual() {
        const params = new URLSearchParams(window.location.search);
        return params.get("cita");
    }

    function construirTextoResumen(resultadoFinal, datosDiabetica, datosHipertensiva) {

        let texto =
            "RESULTADO FINAL:\n" + resultadoFinal +
            "\n\n" +
            "MODELO DE RETINOPATÍA DIABÉTICA:\n" +
            "Resultado: " + (datosDiabetica?.label ?? "N/D") +
            "\n" +
            "Confianza: " + (datosDiabetica?.confidence ?? 0) + "%" +
            "\n" +
            "Probabilidad positiva: " + (datosDiabetica?.probabilityPositive ?? 0) + "%" +
            "\n" +
            "Probabilidad negativa: " + (datosDiabetica?.probabilityNegative ?? 0) + "%" +
            "\n\n" +
            "MODELO DE RETINOPATÍA HIPERTENSIVA:\n" +
            "Resultado: " + (datosHipertensiva?.label ?? "N/D") +
            "\n" +
            "Confianza: " + (datosHipertensiva?.confidence ?? 0) + "%" +
            "\n" +
            "Probabilidad positiva: " + (datosHipertensiva?.probabilityPositive ?? 0) + "%" +
            "\n" +
            "Probabilidad negativa: " + (datosHipertensiva?.probabilityNegative ?? 0) + "%";

        if (
            METRICAS_VALIDACION_DIABETICA.f1 !== null &&
            METRICAS_VALIDACION_DIABETICA.recall !== null &&
            METRICAS_VALIDACION_DIABETICA.rocAuc !== null
        ) {
            texto +=
                "\n\n" +
                "MÉTRICAS GLOBALES DE VALIDACIÓN DEL MODELO DIABÉTICO:\n" +
                "F1-score: " + METRICAS_VALIDACION_DIABETICA.f1 +
                "\n" +
                "Recall: " + METRICAS_VALIDACION_DIABETICA.recall +
                "\n" +
                "ROC AUC: " + METRICAS_VALIDACION_DIABETICA.rocAuc;
        }

        texto +=
            "\n\n" +
            "OBSERVACIONES:\n" + OBSERVACIONES +
            "\n\n" +
            "LIMITACIONES:\n" + LIMITACIONES;

        return texto;
    }

    function construirHtmlResumen(resultadoFinal, datosDiabetica, datosHipertensiva) {

        let html =
            "<strong>RESULTADO FINAL:</strong><br>" + resultadoFinal +
            "<br><br>" +
            "<strong>MODELO DE RETINOPATÍA DIABÉTICA:</strong><br>" +
            "Resultado: " + (datosDiabetica?.label ?? "N/D") +
            "<br>" +
            "Confianza: " + (datosDiabetica?.confidence ?? 0) + "%" +
            "<br>" +
            "Probabilidad positiva: " + (datosDiabetica?.probabilityPositive ?? 0) + "%" +
            "<br>" +
            "Probabilidad negativa: " + (datosDiabetica?.probabilityNegative ?? 0) + "%" +
            "<br><br>" +
            "<strong>MODELO DE RETINOPATÍA HIPERTENSIVA:</strong><br>" +
            "Resultado: " + (datosHipertensiva?.label ?? "N/D") +
            "<br>" +
            "Confianza: " + (datosHipertensiva?.confidence ?? 0) + "%" +
            "<br>" +
            "Probabilidad positiva: " + (datosHipertensiva?.probabilityPositive ?? 0) + "%" +
            "<br>" +
            "Probabilidad negativa: " + (datosHipertensiva?.probabilityNegative ?? 0) + "%";

        if (
            METRICAS_VALIDACION_DIABETICA.f1 !== null &&
            METRICAS_VALIDACION_DIABETICA.recall !== null &&
            METRICAS_VALIDACION_DIABETICA.rocAuc !== null
        ) {
            html +=
                "<br><br>" +
                "<strong>MÉTRICAS GLOBALES DE VALIDACIÓN DEL MODELO DIABÉTICO:</strong><br>" +
                "F1-score: " + METRICAS_VALIDACION_DIABETICA.f1 +
                "<br>" +
                "Recall: " + METRICAS_VALIDACION_DIABETICA.recall +
                "<br>" +
                "ROC AUC: " + METRICAS_VALIDACION_DIABETICA.rocAuc;
        }

        html +=
            "<br><br>" +
            "<strong>OBSERVACIONES:</strong><br>" + OBSERVACIONES +
            "<br><br>" +
            "<strong>LIMITACIONES:</strong><br>" + LIMITACIONES;

        return html;
    }

    function obtenerNombreOjo(lado) {
        return lado === "left" ? "Ojo izquierdo" : "Ojo derecho";
    }

    function crearOpcionesGrado(selectGrado) {
        selectGrado.innerHTML = "";

        const defaultOption = document.createElement("option");
        defaultOption.text = "Seleccionar";
        defaultOption.value = "";
        selectGrado.appendChild(defaultOption);

        const grados = ["Leve", "Moderada", "Severa", "Proliferativa"];

        grados.forEach(g => {
            const option = document.createElement("option");
            option.text = g;
            option.value = g.toLowerCase();
            selectGrado.appendChild(option);
        });

        selectGrado.disabled = false;
    }

    function resetearGrado(selectGrado) {
        selectGrado.innerHTML = "";

        const defaultOption = document.createElement("option");
        defaultOption.text = "Seleccionar";
        defaultOption.value = "";
        selectGrado.appendChild(defaultOption);

        selectGrado.disabled = true;
    }

    function configurarTipoYGrado(tipoSelectActual, gradoSelectActual) {
        tipoSelectActual.addEventListener("change", function () {
            if (this.value === "diabetica") {
                crearOpcionesGrado(gradoSelectActual);
            } else {
                resetearGrado(gradoSelectActual);
            }
        });
    }

    configurarTipoYGrado(typeSelect, gradeSelect);
    configurarTipoYGrado(typeSelectSecond, gradeSelectSecond);

    function registrarOjoAceptado(lado) {
        if (!ojosAceptados.includes(lado)) {
            ojosAceptados.push(lado);
        }

        actualizarValidacionDinamica();
    }

    function quitarOjoAceptado(lado) {
        ojosAceptados = ojosAceptados.filter(ojo => ojo !== lado);

        if (ojosAceptados.length < 2) {
            segundaFilaAgregada = false;
            validationRowSecondary.style.display = "none";
            addSecondEyeWrapper.style.display = "none";
        }

        actualizarValidacionDinamica();
    }

    function actualizarValidacionDinamica() {

        validationRowPrimary.style.display = "none";
        validationRowSecondary.style.display = "none";
        addSecondEyeWrapper.style.display = "none";

        if (ojosAceptados.length === 0) {
            return;
        }

        eyeLabelPrimary.textContent = obtenerNombreOjo(ojosAceptados[0]);
        validationRowPrimary.style.display = "flex";

        if (ojosAceptados.length >= 2) {
            eyeLabelSecondary.textContent = obtenerNombreOjo(ojosAceptados[1]);

            if (segundaFilaAgregada) {
                validationRowSecondary.style.display = "flex";
            } else {
                addSecondEyeWrapper.style.display = "flex";
            }
        }
    }

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

                preview.classList.remove("empty-preview-report");
                preview.style.backgroundImage = "url(" + reader.result + ")";
                preview.style.backgroundSize = "cover";
                preview.style.backgroundPosition = "center";
                preview.style.backgroundRepeat = "no-repeat";

                document.getElementById(statusId).innerHTML = "Imagen cargada: " + file.name;
            };

            reader.readAsDataURL(file);
        });
    }

    cargarImagen(inputLeft, "previewLeft", "imageStatusLeft");
    cargarImagen(inputRight, "previewRight", "imageStatusRight");

    async function analizar(input, resultId, buttonsId, lado) {

        if (reporteGenerado) {
            alert("El análisis ya fue finalizado para esta cita.");
            return;
        }

        if (!input.files || input.files.length === 0) {
            alert("⚠️ Debes cargar una imagen primero.");
            return;
        }

        document.getElementById("loadingSection").style.display = "block";

        try {
            const citaId = obtenerCitaIdActual();

            if (!citaId) {
                alert("⚠️ No se encontró la cita asociada al análisis.");
                return;
            }

            const formData = new FormData();
            formData.append("image", input.files[0]);
            formData.append("lado", lado);
            formData.append("citaId", citaId);

            const response = await fetch(analyzeEndpoint, {
                method: "POST",
                body: formData,
                credentials: "same-origin",
                headers: {
                    "RequestVerificationToken": antiforgeryToken
                }
            });

            const data = await response.json();

            if (!response.ok || !data.success) {
                throw new Error(data.message || "No se pudo obtener respuesta del modelo.");
            }

            const resultadoFinal = `${data.label} con un ${data.confidence}%`;

            document.getElementById(resultId).innerHTML =
                "<div class='analysis-result-main'><strong>Resultado final:</strong> " + resultadoFinal + "</div>" +
                "<div class='analysis-result-block'><strong>Modelo de retinopatía diabética:</strong><br>" +
                "Resultado: " + (data.diabetic?.label ?? "N/D") +
                " | Confianza: " + (data.diabetic?.confidence ?? 0) + "%" +
                " | Prob. positiva: " + (data.diabetic?.probabilityPositive ?? 0) + "%" +
                "</div>" +
                "<div class='analysis-result-block'><strong>Modelo de retinopatía hipertensiva:</strong><br>" +
                "Resultado: " + (data.hypertensive?.label ?? "N/D") +
                " | Confianza: " + (data.hypertensive?.confidence ?? 0) + "%" +
                " | Prob. positiva: " + (data.hypertensive?.probabilityPositive ?? 0) + "%" +
                "</div>";

            if (lado === "left") {
                resultadoLeft = resultadoFinal;
                confidenceLeft = data.confidence;
                probabilityPositiveLeft = data.diabetic?.probabilityPositive ?? 0;
                probabilityNegativeLeft = data.diabetic?.probabilityNegative ?? 0;

                diabeticMetricsLeft = data.diabetic ?? null;
                hypertensiveMetricsLeft = data.hypertensive ?? null;
            } else {
                resultadoRight = resultadoFinal;
                confidenceRight = data.confidence;
                probabilityPositiveRight = data.diabetic?.probabilityPositive ?? 0;
                probabilityNegativeRight = data.diabetic?.probabilityNegative ?? 0;

                diabeticMetricsRight = data.diabetic ?? null;
                hypertensiveMetricsRight = data.hypertensive ?? null;
            }

            document.getElementById(buttonsId).style.display = "block";
        }
        catch (error) {
            console.error(error);
            alert("⚠️ Error al analizar la imagen: " + error.message);
        }
        finally {
            document.getElementById("loadingSection").style.display = "none";
        }
    }

    startLeft.addEventListener("click", function () {
        analizar(inputLeft, "analysisResultLeft", "resultButtonsLeft", "left");
    });

    startRight.addEventListener("click", function () {
        analizar(inputRight, "analysisResultRight", "resultButtonsRight", "right");
    });

    function aceptar(lado, buttonsId, acceptedId, summaryId) {

        document.getElementById(buttonsId).style.display = "none";
        document.getElementById(acceptedId).innerHTML = "✔ Aceptado";

        const resultado = lado === "left" ? resultadoLeft : resultadoRight;
        const datosDiabetica = lado === "left" ? diabeticMetricsLeft : diabeticMetricsRight;
        const datosHipertensiva = lado === "left" ? hypertensiveMetricsLeft : hypertensiveMetricsRight;

        const resumen = document.getElementById(summaryId);

        if (!resumen) {
            console.error("No se encontró el contenedor del resumen:", summaryId);
            return;
        }

        resumen.innerHTML = construirHtmlResumen(
            resultado,
            datosDiabetica,
            datosHipertensiva
        );

        registrarOjoAceptado(lado);
    }

    document.getElementById("acceptLeft").addEventListener("click", function () {
        aceptar("left", "resultButtonsLeft", "acceptedLabelLeft", "summaryLeft");
    });

    document.getElementById("acceptRight").addEventListener("click", function () {
        aceptar("right", "resultButtonsRight", "acceptedLabelRight", "summaryRight");
    });

    function repetirAnalisis(lado) {

        cerrarZoom();

        if (lado === "left") {
            inputLeft.value = "";
            resultadoLeft = "";
            confidenceLeft = null;
            probabilityPositiveLeft = null;
            probabilityNegativeLeft = null;
            diabeticMetricsLeft = null;
            hypertensiveMetricsLeft = null;

            previewLeft.style.backgroundImage = "";
            previewLeft.classList.remove("empty-preview-report");

            document.getElementById("imageStatusLeft").innerHTML = "";
            document.getElementById("analysisResultLeft").innerHTML = "";
            document.getElementById("resultButtonsLeft").style.display = "none";
            document.getElementById("acceptedLabelLeft").innerHTML = "";
            document.getElementById("summaryLeft").innerHTML = "";

            quitarOjoAceptado("left");
        } else {
            inputRight.value = "";
            resultadoRight = "";

            confidenceRight = null;
            probabilityPositiveRight = null;
            probabilityNegativeRight = null;

            diabeticMetricsRight = null;
            hypertensiveMetricsRight = null;

            previewRight.style.backgroundImage = "";
            previewRight.classList.remove("empty-preview-report");

            document.getElementById("imageStatusRight").innerHTML = "";
            document.getElementById("analysisResultRight").innerHTML = "";
            document.getElementById("resultButtonsRight").style.display = "none";
            document.getElementById("acceptedLabelRight").innerHTML = "";
            document.getElementById("summaryRight").innerHTML = "";

            quitarOjoAceptado("right");
        }
    }

    document.getElementById("repeatLeft").addEventListener("click", function () {
        repetirAnalisis("left");
    });

    document.getElementById("repeatRight").addEventListener("click", function () {
        repetirAnalisis("right");
    });

    btnAddSecondEye.addEventListener("click", function () {
        if (ojosAceptados.length < 2) return;

        segundaFilaAgregada = true;
        actualizarValidacionDinamica();
    });

    function cerrarZoom() {
        overlay.classList.remove("active");
        overlay.style.display = "none";
        overlayZoomImage.style.backgroundImage = "";
        document.body.style.overflow = "";
    }

    function abrirZoom(preview) {
        const imagenActual = preview.style.backgroundImage;

        if (!imagenActual || imagenActual === "none") {
            return;
        }

        overlayZoomImage.style.backgroundImage = imagenActual;
        overlay.style.display = "flex";
        overlay.classList.add("active");
        document.body.style.overflow = "hidden";
    }

    function activarZoom(previewId) {
        const preview = document.getElementById(previewId);

        preview.addEventListener("click", function (e) {
            e.stopPropagation();
            abrirZoom(preview);
        });
    }

    overlay.addEventListener("click", function (e) {
        if (e.target === overlay || e.target === overlayZoomImage) {
            cerrarZoom();
        }
    });

    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape") {
            cerrarZoom();
        }
    });

    activarZoom("previewLeft");
    activarZoom("previewRight");

    function crearBloqueTextoReporte(texto) {
        const div = document.createElement("div");
        div.style.whiteSpace = "pre-wrap";
        div.style.wordBreak = "break-word";
        div.style.lineHeight = "1.6";
        div.style.fontSize = "13px";
        div.style.minHeight = "140px";
        div.style.border = "1px solid #c9d8e2";
        div.style.borderRadius = "12px";
        div.style.background = "#fff";
        div.style.padding = "10px";
        div.innerText = texto;
        return div;
    }

    function crearBloqueSelectReporte(texto) {
        const div = document.createElement("div");
        div.style.minHeight = "36px";
        div.style.border = "1px solid #c9d8e2";
        div.style.borderRadius = "10px";
        div.style.background = "#fff";
        div.style.padding = "8px 10px";
        div.style.fontSize = "12px";
        div.style.lineHeight = "1.3";
        div.style.wordBreak = "break-word";
        div.style.whiteSpace = "normal";
        div.style.display = "flex";
        div.style.alignItems = "center";
        div.innerText = texto;
        return div;
    }

    document.getElementById("btnReport").addEventListener("click", async function () {

        if (reporteGenerado) {
            alert("⚠️ El reporte ya fue generado.");
            return;
        }

        cerrarZoom();

        const obsLeft = document.getElementById("obsLeft").value.trim();
        const obsRight = document.getElementById("obsRight").value.trim();

        const imgLeft = inputLeft.files.length > 0;
        const imgRight = inputRight.files.length > 0;

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

        const medicoResponsable =
            document.getElementById("medicoResponsableNombre")?.innerText.trim() ||
            "Médico";

        document.getElementById("validationInfo").innerHTML =
            "<b>Médico responsable:</b> " + medicoResponsable + "<br>" +
            "<b>Fecha validación:</b> " + fecha;

        const codigo = document.getElementById("codigoCita").innerText.replace("CITA ", "").trim();
        const fechaArchivo = new Date().toISOString().split("T")[0];
        const element = document.getElementById("reportContent");

        element.classList.add("report-mode");

        const elementosOcultos = [];
        element.querySelectorAll(".hide-on-report").forEach(el => {
            elementosOcultos.push({
                elemento: el,
                displayOriginal: el.style.display
            });
            el.style.display = "none";
        });

        const textareasReemplazadas = [];
        element.querySelectorAll("textarea").forEach(t => {
            const bloque = crearBloqueTextoReporte(t.value);
            const parent = t.parentNode;
            textareasReemplazadas.push({
                original: t,
                reemplazo: bloque,
                parent: parent
            });
            parent.replaceChild(bloque, t);
        });

        const selectsReemplazados = [];
        element.querySelectorAll("select").forEach(s => {
            const textoSeleccionado = s.options[s.selectedIndex]
                ? s.options[s.selectedIndex].text
                : "";

            const bloque = crearBloqueSelectReporte(textoSeleccionado);
            const parent = s.parentNode;

            selectsReemplazados.push({
                original: s,
                reemplazo: bloque,
                parent: parent
            });

            parent.replaceChild(bloque, s);
        });

        const previewsMarcados = [];

        previewLeft.classList.remove("empty-preview-report");
        previewRight.classList.remove("empty-preview-report");

        if (!imgLeft) {
            previewLeft.style.backgroundImage = "";
            previewLeft.classList.add("empty-preview-report");
            previewsMarcados.push(previewLeft);
        }

        if (!imgRight) {
            previewRight.style.backgroundImage = "";
            previewRight.classList.add("empty-preview-report");
            previewsMarcados.push(previewRight);
        }

        try {
            await new Promise(resolve => setTimeout(resolve, 300));

            const opt = {
                margin: 0.2,
                filename: `Reporte_Cita_${codigo}_${fechaArchivo}.pdf`,
                image: { type: "jpeg", quality: 1 },
                html2canvas: {
                    scale: 2.5,
                    scrollX: 0,
                    scrollY: 0,
                    useCORS: true,
                    backgroundColor: "#ffffff",
                    windowWidth: element.scrollWidth,
                    windowHeight: element.scrollHeight
                },
                jsPDF: {
                    unit: "in",
                    format: "letter",
                    orientation: "landscape"
                },
                pagebreak: {
                    mode: ["avoid-all", "css", "legacy"]
                }
            };

            const worker = html2pdf().set(opt).from(element);

            await worker.toPdf();

            const pdf = await worker.get("pdf");
            const pdfBlob = pdf.output("blob");

            const citaId = obtenerCitaIdActual();

            if (!citaId) {
                throw new Error("No se encontró el identificador de la cita.");
            }

            const pdfFile = new File(
                [pdfBlob],
                `Reporte_Cita_${codigo}_${fechaArchivo}.pdf`,
                { type: "application/pdf" }
            );

            const formDataGuardar = new FormData();
            formDataGuardar.append("citaId", citaId);
            formDataGuardar.append("resultadoIzq", resultadoLeft || "");
            formDataGuardar.append("resultadoDer", resultadoRight || "");
            formDataGuardar.append("confianzaIzq", confidenceLeft != null ? confidenceLeft : "");
            formDataGuardar.append("confianzaDer", confidenceRight != null ? confidenceRight : "");
            formDataGuardar.append("recomendacion", document.getElementById("modelRecommendation")?.value || "");
            formDataGuardar.append("tipo", document.getElementById("retinopathyType")?.value || "");
            formDataGuardar.append("grado", document.getElementById("retinopathyGrade")?.value || "");
            formDataGuardar.append("obsIzq", obsLeft);
            formDataGuardar.append("obsDer", obsRight);
            formDataGuardar.append("reportePdfFile", pdfFile);

            if (inputLeft.files.length > 0) {
                formDataGuardar.append("imagenIzquierdaFile", inputLeft.files[0]);
            }

            if (inputRight.files.length > 0) {
                formDataGuardar.append("imagenDerechaFile", inputRight.files[0]);
            }

            const responseGuardar = await fetch(guardarReporteEndpoint, {
                method: "POST",
                body: formDataGuardar,
                credentials: "same-origin",
                headers: {
                    "RequestVerificationToken": antiforgeryToken
                }
            });

            const dataGuardar = await responseGuardar.json();

            if (!responseGuardar.ok || !dataGuardar.success) {
                throw new Error(dataGuardar.message || "No se pudo guardar el reporte.");
            }

            await worker.save();

            reporteGenerado = true;

            document.querySelectorAll(".analyze-btn").forEach(b => b.disabled = true);
            document.querySelectorAll(".file-btn").forEach(b => b.disabled = true);
            document.getElementById("btnReport").disabled = true;

        } catch (error) {
            console.error(error);
            alert("⚠️ Ocurrió un error al generar el reporte.");
        } finally {

            selectsReemplazados.forEach(item => {
                if (item.reemplazo.parentNode) {
                    item.parent.replaceChild(item.original, item.reemplazo);
                }
            });

            textareasReemplazadas.forEach(item => {
                if (item.reemplazo.parentNode) {
                    item.parent.replaceChild(item.original, item.reemplazo);
                }
            });

            elementosOcultos.forEach(item => {
                item.elemento.style.display = item.displayOriginal;
            });

            previewsMarcados.forEach(preview => {
                preview.classList.remove("empty-preview-report");
            });

            element.classList.remove("report-mode");
        }
    });

    actualizarValidacionDinamica();
});