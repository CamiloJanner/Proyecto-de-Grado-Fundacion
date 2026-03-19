const input = document.getElementById("imageInput");

input.addEventListener("change", function (e) {

    const file = e.target.files[0];

    if (!file) return;

    const extension = file.name.split(".").pop().toLowerCase();

    if (extension !== "jpg" && extension !== "jpeg") {

        alert("Solo se permiten imágenes JPG");
        input.value = "";
        return;
    }

    const reader = new FileReader();

    reader.onload = function () {

        document.getElementById("previewRight").style.backgroundImage =
            "url(" + reader.result + ")";

        document.getElementById("imageStatus").innerHTML =
            "Imagen cargada: " + file.name;

    }

    reader.readAsDataURL(file);

});



document.getElementById("startAnalysis").addEventListener("click", function () {

    document.getElementById("loadingSection").style.display = "block";

    document.getElementById("imageStatus").innerHTML +=
        "<br>Estado: Procesando...";



    setTimeout(function () {

        document.getElementById("loadingSection").style.display = "none";

        document.getElementById("analysisResult").innerHTML =
            "Retinopatía Diabética no Proliferativa con un 92%";

        document.getElementById("resultButtons").style.display = "block";

        document.getElementById("rightSummary").value =
            "Retinopatía Diabética no Proliferativa\nConfianza 92%\n\nHallazgos detectados:\n- Hemorragias\n- Exudados\n- Microaneurismas";

    }, 3000);

});