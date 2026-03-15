function previewImage(inputId, previewId) {

    const input = document.getElementById(inputId);

    input.addEventListener("change", function (e) {

        const file = e.target.files[0];

        if (!file) return;

        const extension = file.name.split(".").pop().toLowerCase();

        if (extension !== "jpg" && extension !== "jpeg") {
            document.getElementById("errorModal").style.display = "block";
            input.value = "";
            return;
        }

        const reader = new FileReader();

        reader.onload = function () {

            const preview = document.getElementById(previewId);
            preview.style.backgroundImage = "url(" + reader.result + ")";
            preview.style.backgroundSize = "cover";
            preview.style.backgroundPosition = "center";

        }

        reader.readAsDataURL(file);

    });
}

previewImage("leftImage", "previewLeft");
previewImage("rightImage", "previewRight");


/* MODAL */

function closeModal() {
    document.getElementById("errorModal").style.display = "none";
}


/* ACORDEON */

function toggleSection(id) {

    const section = document.getElementById(id);

    if (section.style.display === "block") {
        section.style.display = "none";
    } else {
        section.style.display = "block";
    }

}


/* LOADING ANALISIS */

document.getElementById("startAnalysis").addEventListener("click", function () {

    document.getElementById("loadingSection").style.display = "block";

    setTimeout(function () {

        document.getElementById("loadingSection").style.display = "none";

        alert("Análisis completado");

    }, 3000);

});