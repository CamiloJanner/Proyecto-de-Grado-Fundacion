document.addEventListener("DOMContentLoaded", function () {

    const tituloModulo = document.getElementById("tituloModulo");

    const secciones = {
        usuarios: document.getElementById("sectionUsuarios"),
        perfiles: document.getElementById("sectionPerfiles"),
        reportes: document.getElementById("sectionReportes"),
        observaciones: document.getElementById("sectionObservaciones"),
        logs: document.getElementById("sectionLogs")
    };

    const titulos = {
        usuarios: "Usuarios Clinic Online",
        perfiles: "Perfiles de usuario",
        reportes: "Reportes finalizados",
        observaciones: "Observaciones clínicas",
        logs: "Logs de auditoría"
    };

    function ocultarSecciones() {
        Object.values(secciones).forEach(section => {
            if (section) {
                section.classList.remove("active-section");
            }
        });
    }

    function quitarActivoMenu() {
        document.querySelectorAll(".admin-menu-link").forEach(btn => {
            btn.classList.remove("active");
        });
    }

    function mostrarSeccion(nombreSeccion) {
        ocultarSecciones();
        quitarActivoMenu();

        if (secciones[nombreSeccion]) {
            secciones[nombreSeccion].classList.add("active-section");
        }

        if (tituloModulo && titulos[nombreSeccion]) {
            tituloModulo.innerText = titulos[nombreSeccion];
        }

        const boton = document.querySelector(`.admin-menu-link[data-section="${nombreSeccion}"]`);

        if (boton) {
            boton.classList.add("active");
        }

        sessionStorage.setItem("adminSection", nombreSeccion);
    }

    document.querySelectorAll(".admin-menu-link").forEach(btn => {
        btn.addEventListener("click", function () {
            const section = this.getAttribute("data-section");
            mostrarSeccion(section);
        });
    });

    const seccionGuardada = sessionStorage.getItem("adminSection");

    if (seccionGuardada && secciones[seccionGuardada]) {
        mostrarSeccion(seccionGuardada);
    } else {
        mostrarSeccion("usuarios");
    }

    document.querySelectorAll(".btn-asignar-rol").forEach(btn => {
        btn.addEventListener("click", function () {
            const fila = this.closest("tr");
            const editor = fila.querySelector(".role-editor");

            if (!editor) return;

            editor.classList.add("show");
            this.style.display = "none";
        });
    });

    document.querySelectorAll(".btn-quitar-activo").forEach(btn => {
        btn.addEventListener("click", function () {
            const fila = this.closest("tr");
            const checkbox = fila.querySelector('input[type="checkbox"][name$=".Estado"]');

            if (checkbox) {
                checkbox.checked = false;
            }
        });
    });

    const btnAgregarPerfilVisual = document.getElementById("btnAgregarPerfilVisual");
    const tablaPerfiles = document.getElementById("tablaPerfiles");

    function mostrarMensajeVisual(texto) {
        let mensaje = document.getElementById("mensajeVisualPerfiles");

        if (!mensaje) {
            mensaje = document.createElement("div");
            mensaje.id = "mensajeVisualPerfiles";
            mensaje.className = "error-box success-box";
            mensaje.style.display = "block";
            mensaje.style.marginBottom = "16px";

            const sectionPerfiles = document.getElementById("sectionPerfiles");

            if (sectionPerfiles) {
                sectionPerfiles.insertBefore(mensaje, sectionPerfiles.firstChild);
            }
        }

        mensaje.innerText = texto;
        mensaje.style.display = "block";

        setTimeout(() => {
            mensaje.style.display = "none";
        }, 3000);
    }

    document.addEventListener("click", function (e) {

        if (e.target.classList.contains("btn-editar-perfil")) {
            const fila = e.target.closest("tr");

            if (!fila) return;

            const nombre = fila.querySelector(".profile-name-input");
            const descripcion = fila.querySelector(".profile-description-input");
            const btnGuardar = fila.querySelector(".btn-guardar-perfil");

            if (nombre) {
                nombre.removeAttribute("readonly");
                nombre.classList.add("profile-editing");
            }

            if (descripcion) {
                descripcion.removeAttribute("readonly");
                descripcion.classList.add("profile-editing");
            }

            e.target.style.display = "none";

            if (btnGuardar) {
                btnGuardar.style.display = "inline-block";
            }
        }

        if (e.target.classList.contains("btn-guardar-perfil")) {
            const fila = e.target.closest("tr");

            if (!fila) return;

            const nombre = fila.querySelector(".profile-name-input");
            const descripcion = fila.querySelector(".profile-description-input");
            const btnEditar = fila.querySelector(".btn-editar-perfil");

            if (nombre) {
                nombre.setAttribute("readonly", "readonly");
                nombre.classList.remove("profile-editing");
            }

            if (descripcion) {
                descripcion.setAttribute("readonly", "readonly");
                descripcion.classList.remove("profile-editing");
            }

            e.target.style.display = "none";

            if (btnEditar) {
                btnEditar.style.display = "inline-block";
            }

            mostrarMensajeVisual("Cambios guardados visualmente. No se modificó ningún rol en la base de datos.");
        }
    });

    if (btnAgregarPerfilVisual && tablaPerfiles) {
        btnAgregarPerfilVisual.addEventListener("click", function () {
            const noProfiles = document.getElementById("noProfiles");

            if (noProfiles) {
                noProfiles.remove();
            }

            const filas = document.querySelectorAll("#tablaPerfiles .perfil-row");
            const nuevoNumero = filas.length + 1;

            const fila = document.createElement("tr");
            fila.classList.add("perfil-row");

            fila.innerHTML = `
            <td>Visual ${nuevoNumero}</td>

            <td>
                <input type="text"
                       class="profile-input profile-name-input profile-editing"
                       placeholder="Nombre del perfil visual" />
            </td>

            <td>
                <textarea class="profile-textarea profile-description-input profile-editing"
                          placeholder="Descripción visual del acceso"></textarea>
            </td>

            <td>
                <button type="button" class="view-btn btn-editar-perfil" style="display:none;">
                    Editar
                </button>

                <button type="button" class="view-btn btn-guardar-perfil">
                    Guardar
                </button>
            </td>
        `;

            tablaPerfiles.appendChild(fila);
        });
    }
});