using System.Linq;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoDeGradoFundacion.Data;
using ProyectoDeGradoFundacion.Models;
using ProyectoDeGradoFundacion.Services;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace ProyectoDeGradoFundacion.Pages.Analysis
{
    public class AnalysisModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ClinicOnlineReporteService _clinicOnlineReporteService;

        public AnalysisModel(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            ClinicOnlineReporteService clinicOnlineReporteService)
        {
            _context = context;
            _environment = environment;
            _clinicOnlineReporteService = clinicOnlineReporteService;
        }

        private string? BuscarArchivoRecursivo(string[] raices, string[] nombresArchivo)
        {
            foreach (var raiz in raices)
            {
                if (string.IsNullOrWhiteSpace(raiz) || !Directory.Exists(raiz))
                    continue;

                foreach (var nombre in nombresArchivo)
                {
                    try
                    {
                        var encontrado = Directory
                            .EnumerateFiles(raiz, nombre, SearchOption.AllDirectories)
                            .FirstOrDefault();

                        if (!string.IsNullOrWhiteSpace(encontrado))
                            return encontrado;
                    }
                    catch
                    {
                    }
                }
            }

            return null;
        }

        private string ConstruirDetalleBusqueda(string[] raices, string[] nombresArchivo)
        {
            var detalle = new StringBuilder();

            detalle.AppendLine("Raíces buscadas:");
            foreach (var raiz in raices)
            {
                detalle.AppendLine($"- {raiz}");
            }

            detalle.AppendLine("Nombres buscados:");
            foreach (var nombre in nombresArchivo)
            {
                detalle.AppendLine($"- {nombre}");
            }

            return detalle.ToString();
        }

        private async Task<ModelPredictionResponse> EjecutarInferenciaModeloAsync(
    string tempFilePath,
    string[] raicesBusqueda,
    string[] nombresScript,
    string[] nombresModelo,
    string nombreModeloHumano)
        {
            var scriptPath = BuscarArchivoRecursivo(raicesBusqueda, nombresScript);
            var modelPath = BuscarArchivoRecursivo(raicesBusqueda, nombresModelo);

            if (string.IsNullOrWhiteSpace(scriptPath))
                throw new Exception(
                    $"No se encontró el script de inferencia para {nombreModeloHumano}.\n" +
                    ConstruirDetalleBusqueda(raicesBusqueda, nombresScript));

            if (string.IsNullOrWhiteSpace(modelPath))
                throw new Exception(
                    $"No se encontró el modelo para {nombreModeloHumano}.\n" +
                    ConstruirDetalleBusqueda(raicesBusqueda, nombresModelo));

            var pythonExecutable = "python";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = pythonExecutable,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            processStartInfo.Environment["PYTHONIOENCODING"] = "utf-8";
            processStartInfo.ArgumentList.Add(scriptPath);
            processStartInfo.ArgumentList.Add("--model");
            processStartInfo.ArgumentList.Add(modelPath);
            processStartInfo.ArgumentList.Add("--image");
            processStartInfo.ArgumentList.Add(tempFilePath);

            using var process = new Process
            {
                StartInfo = processStartInfo
            };

            process.Start();

            var standardOutput = await process.StandardOutput.ReadToEndAsync();
            var standardError = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new Exception(
                    $"Error ejecutando {nombreModeloHumano}: " +
                    (string.IsNullOrWhiteSpace(standardError) ? standardOutput : standardError));
            }

            if (string.IsNullOrWhiteSpace(standardOutput))
            {
                throw new Exception($"El modelo {nombreModeloHumano} no devolvió ninguna respuesta.");
            }

            var prediction = JsonSerializer.Deserialize<ModelPredictionResponse>(
                standardOutput,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (prediction == null)
                throw new Exception($"No se pudo interpretar la respuesta del modelo {nombreModeloHumano}.");

            if (!prediction.Success)
                throw new Exception(prediction.Message ?? $"El modelo {nombreModeloHumano} devolvió un error.");

            return prediction;
        }

        private bool EsPositivo(string? label, string positivoEsperado)
        {
            if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(positivoEsperado))
                return false;

            return label.Trim().StartsWith(positivoEsperado.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        public Cita? CitaActual { get; set; }

        public string CodigoCita { get; set; } = "";
        public string NombrePaciente { get; set; } = "";
        public string Fecha { get; set; } = "";
        public string MedicoResponsable { get; set; } = "Médico";

        // Ajusta estos valores si en tu tabla roles o estados tienen otros IDs.
        private const int ROL_PACIENTE_ID = 4;
        private const int ESTADO_USUARIO_ACTIVO_ID = 1;

        public async Task<IActionResult> OnGetAsync(
            int? cita,
            string? codigoCita,
            string? documento,
            string? tipoDocumento,
            string? nombrePaciente,
            DateTime? fechaCita)
        {
            /*
             * CASO 1:
             * La página viene desde consultar_citas con datos de Clinic Online.
             * Entonces se sincroniza la cita en la base local de Aiven.
             */
            if (cita == null && !string.IsNullOrWhiteSpace(codigoCita))
            {
                var citaLocal = await ObtenerOCrearCitaDesdeClinicOnlineAsync(
                    codigoCita,
                    documento,
                    tipoDocumento,
                    nombrePaciente,
                    fechaCita
                );

                return RedirectToPage("/Analysis/Index", new { cita = citaLocal.CitaId });
            }

            /*
             * CASO 2:
             * La página ya viene con el ID local de la cita.
             */
            if (cita == null)
                return RedirectToPage("/Analysis/consultar_citas");

            CitaActual = await _context.Citas
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.CitaId == cita.Value);

            if (CitaActual == null)
                return RedirectToPage("/Analysis/consultar_citas");

            CodigoCita = CitaActual.CitaExternaId ?? $"CITA-{CitaActual.CitaId}";
            NombrePaciente = $"{CitaActual.Usuario.NombreUsuario} {CitaActual.Usuario.ApellidoUsuario}";
            Fecha = CitaActual.FechaCreado.ToString("yyyy/MM/dd");

            MedicoResponsable =
    User.FindFirstValue("LoginClinicOnline") ??
    User.FindFirstValue(ClaimTypes.Name) ??
    "Médico";

            return Page();
        }

        private async Task<Cita> ObtenerOCrearCitaDesdeClinicOnlineAsync(
            string codigoCita,
            string? documento,
            string? tipoDocumento,
            string? nombrePaciente,
            DateTime? fechaCita)
        {
            var codigoLimpio = codigoCita.Trim();
            var documentoLimpio = string.IsNullOrWhiteSpace(documento)
                ? $"SIN_DOC_{codigoLimpio}"
                : documento.Trim();

            /*
             * 1. Si la cita ya existe en Aiven, se reutiliza.
             * Esto evita duplicar citas si el médico entra varias veces.
             */
            var citaExistente = await _context.Citas
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.CitaExternaId == codigoLimpio);

            if (citaExistente != null)
            {
                return citaExistente;
            }

            /*
             * 2. Buscar o crear el usuario/paciente local.
             * Clinic Online entrega el paciente, pero tu sistema necesita usuario_id
             * porque la tabla citas tiene relación obligatoria con usuarios.
             */
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NumeroIdentificacion == documentoLimpio);

            if (usuario == null)
            {
                var nombresSeparados = SepararNombrePaciente(nombrePaciente);

                usuario = new Usuario
                {
                    RolId = ROL_PACIENTE_ID,
                    NombreUsuario = nombresSeparados.Nombres,
                    ApellidoUsuario = nombresSeparados.Apellidos,
                    Email = $"paciente_{documentoLimpio}_{codigoLimpio}@clinic-online.local",
                    Telefono = null,
                    TipoIdentificacion = string.IsNullOrWhiteSpace(tipoDocumento)
                        ? "CC"
                        : tipoDocumento.Trim(),
                    NumeroIdentificacion = documentoLimpio,
                    PasswordHash = "PACIENTE_EXTERNO_CLINIC_ONLINE",
                    EstadoUsuarioId = ESTADO_USUARIO_ACTIVO_ID,
                    FechaCreado = DateTime.UtcNow
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
            }

            /*
             * 3. Crear la cita local en Aiven.
             * Esta cita local será la que luego use el análisis,
             * la carga de imágenes, los resultados IA, observaciones y PDF.
             */
            var nuevaCita = new Cita
            {
                UsuarioId = usuario.UsuarioId,
                PacienteExternoId = documentoLimpio,
                CitaExternaId = codigoLimpio,
                EstadoCita = "pendiente",
                FechaCreado = fechaCita ?? DateTime.UtcNow
            };

            _context.Citas.Add(nuevaCita);
            await _context.SaveChangesAsync();

            return nuevaCita;
        }

        private static (string Nombres, string Apellidos) SepararNombrePaciente(string? nombrePaciente)
        {
            if (string.IsNullOrWhiteSpace(nombrePaciente))
            {
                return ("Paciente", "Clinic Online");
            }

            var partes = nombrePaciente
                .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            if (partes.Count == 1)
            {
                return (partes[0], "Clinic Online");
            }

            if (partes.Count == 2)
            {
                return (partes[0], partes[1]);
            }

            var mitad = partes.Count / 2;

            var nombres = string.Join(" ", partes.Take(mitad));
            var apellidos = string.Join(" ", partes.Skip(mitad));

            return (nombres, apellidos);
        }

        public async Task<IActionResult> OnPostAnalyzeAsync(IFormFile image, string lado, int citaId)
        {
            const long TAMANO_MAXIMO_BYTES = 10 * 1024 * 1024; // 10 MB

            if (image == null || image.Length == 0)
                return new JsonResult(new
                {
                    success = false,
                    message = "No se recibió ninguna imagen."
                });

            if (image.Length > TAMANO_MAXIMO_BYTES)
                return new JsonResult(new
                {
                    success = false,
                    message = "La imagen supera el tamaño máximo permitido de 10 MB."
                });

            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

            if (extension != ".jpg" && extension != ".jpeg")
                return new JsonResult(new
                {
                    success = false,
                    message = "Solo se permiten imágenes JPG/JPEG."
                });

            var cita = await _context.Citas
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.CitaId == citaId);

            if (cita == null)
                return new JsonResult(new
                {
                    success = false,
                    message = "No se encontró la cita asociada al análisis."
                });

            if (cita.EstadoCita == "finalizado")
                return new JsonResult(new
                {
                    success = false,
                    message = "Esta cita ya tiene un reporte finalizado y no puede volver a procesarse."
                });

            var tempDirectory = Path.Combine(_environment.ContentRootPath, "TempAnalysis");

            if (!Directory.Exists(tempDirectory))
                Directory.CreateDirectory(tempDirectory);

            var tempFileName = $"{Guid.NewGuid()}{extension}";
            var tempFilePath = Path.Combine(tempDirectory, tempFileName);

            try
            {
                /*
                 * IMPORTANTE:
                 * Aquí NO se guarda la imagen en wwwroot/uploads/analisis
                 * y NO se guarda la ruta en la base de datos.
                 *
                 * En este punto la imagen solo se copia temporalmente
                 * para que el modelo de IA pueda analizarla.
                 *
                 * La imagen definitiva se guardará después,
                 * cuando el usuario dé clic en "Visualizar Reporte".
                 */
                await using (var inputStream = image.OpenReadStream())
                {
                    await using (var streamTemporal = new FileStream(tempFilePath, FileMode.Create))
                    {
                        await inputStream.CopyToAsync(streamTemporal);
                    }
                }

                var projectRoot = _environment.ContentRootPath;
                var parent1 = Directory.GetParent(projectRoot)?.FullName;
                var parent2 = parent1 != null ? Directory.GetParent(parent1)?.FullName : null;

                var raicesBusqueda = new[]
                {
            projectRoot,
            parent1 ?? "",
            parent2 ?? ""
        };

                var prediccionDiabetica = await EjecutarInferenciaModeloAsync(
                    tempFilePath,
                    raicesBusqueda,
                    new[]
                    {
                "retinopathy_D_inference.py",
                "retinopathy_inference.py"
                    },
                    new[]
                    {
                "mejor_modelo.pth",
                "mejor_modelo.pth.zip",
                "modelo_d.pth",
                "modelo_d.pth.zip"
                    },
                    "retinopatía diabética"
                );

                var prediccionHipertensiva = await EjecutarInferenciaModeloAsync(
                    tempFilePath,
                    raicesBusqueda,
                    new[]
                    {
                "retinopathy_H_inference.py"
                    },
                    new[]
                    {
                "modelo_h.pth",
                "modelo_h.pth.zip",
                "mejor_modelo_h.pth",
                "mejor_modelo_h.pth.zip"
                    },
                    "retinopatía hipertensiva"
                );

                string etiquetaFinal;
                double confianzaFinal;

                if (EsPositivo(prediccionDiabetica.Label, "Retinopatía Diabética"))
                {
                    etiquetaFinal = prediccionDiabetica.Label ?? "Retinopatía Diabética";
                    confianzaFinal = prediccionDiabetica.Confidence;
                }
                else if (EsPositivo(prediccionHipertensiva.Label, "Retinopatía Hipertensiva"))
                {
                    etiquetaFinal = prediccionHipertensiva.Label ?? "Retinopatía Hipertensiva";
                    confianzaFinal = prediccionHipertensiva.Confidence;
                }
                else
                {
                    etiquetaFinal = "Sin retinopatía diabética e hipertensiva";
                    confianzaFinal = Math.Max(
                        prediccionDiabetica.Confidence,
                        prediccionHipertensiva.Confidence
                    );
                }

                return new JsonResult(new
                {
                    success = true,
                    lado = lado,
                    label = etiquetaFinal,
                    confidence = Math.Round(confianzaFinal, 2),

                    diabetic = new
                    {
                        label = prediccionDiabetica.Label,
                        confidence = prediccionDiabetica.Confidence,
                        probabilityPositive = prediccionDiabetica.Probability_Positive,
                        probabilityNegative = prediccionDiabetica.Probability_Negative,
                        logit = prediccionDiabetica.Logit
                    },

                    hypertensive = new
                    {
                        label = prediccionHipertensiva.Label,
                        confidence = prediccionHipertensiva.Confidence,
                        probabilityPositive = prediccionHipertensiva.Probability_Positive,
                        probabilityNegative = prediccionHipertensiva.Probability_Negative,
                        logit = prediccionHipertensiva.Logit
                    }
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = $"Ocurrió un error al analizar la imagen: {ex.Message}"
                });
            }
            finally
            {
                if (System.IO.File.Exists(tempFilePath))
                {
                    try
                    {
                        System.IO.File.Delete(tempFilePath);
                    }
                    catch
                    {
                    }
                }
            }
        }

        public async Task<IActionResult> OnPostUploadAsync(int citaId, string lado)
        {
            var file = Request.Form.Files.FirstOrDefault();

            if (file == null)
                return new JsonResult(new { success = false });

            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var path = Path.Combine(uploadsPath, fileName);

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            var cita = await _context.Citas.FindAsync(citaId);

            if (cita == null)
                return new JsonResult(new { success = false });

            if (lado == "izquierdo")
                cita.ImagenOjoIzquierdo = fileName;
            else
                cita.ImagenOjoDerecho = fileName;

            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true, file = fileName });
        }

        public async Task<IActionResult> OnPostGuardarAsync(
            int citaId,
            string resultadoIzq,
            string resultadoDer,
            float? confianzaIzq,
            float? confianzaDer,
            string recomendacion,
            string tipo,
            string grado,
            string obsIzq,
            string obsDer)
        {
            var cita = await _context.Citas.FindAsync(citaId);

            if (cita == null)
                return new JsonResult(new { success = false });

            cita.ResultadoOjoIzquierdo = resultadoIzq;
            cita.ResultadoOjoDerecho = resultadoDer;
            cita.ConfianzaOjoIzquierdo = confianzaIzq;
            cita.ConfianzaOjoDerecho = confianzaDer;

            cita.RecomendacionModelo = recomendacion;
            cita.TipoRetinopatia = tipo;
            cita.GradoRetinopatia = grado;

            cita.ObservacionesOjoIzquierdo = obsIzq;
            cita.ObservacionesOjoDerecho = obsDer;

            cita.EstadoCita = "analizado";
            cita.FechaAnalisis = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostGuardarReporteAsync(
    int citaId,
    string resultadoIzq,
    string resultadoDer,
    float? confianzaIzq,
    float? confianzaDer,
    string recomendacion,
    string tipo,
    string grado,
    string obsIzq,
    string obsDer,
    IFormFile? reportePdfFile,
    IFormFile? imagenIzquierdaFile,
    IFormFile? imagenDerechaFile)
        {
            var cita = await _context.Citas
    .Include(c => c.Usuario)
    .FirstOrDefaultAsync(c => c.CitaId == citaId);

            if (cita == null)
                return new JsonResult(new
                {
                    success = false,
                    message = "No se encontró la cita."
                });

            if (cita.EstadoCita == "finalizado")
                return new JsonResult(new
                {
                    success = false,
                    message = "Esta cita ya tiene un reporte finalizado."
                });

            string? rutaReporteGuardado = cita.ReportePdf;
            string? nombreReporteGenerado = null;

            // ============================
            // 1. GUARDAR PDF DEL REPORTE
            // ============================

            if (reportePdfFile != null && reportePdfFile.Length > 0)
            {
                var reportesPath = Path.Combine(_environment.WebRootPath, "reportes");

                if (!Directory.Exists(reportesPath))
                    Directory.CreateDirectory(reportesPath);

                var codigoSeguro = string.IsNullOrWhiteSpace(cita.CitaExternaId)
                    ? $"CITA_{cita.CitaId}"
                    : cita.CitaExternaId
                        .Replace("/", "_")
                        .Replace("\\", "_")
                        .Replace(" ", "_")
                        .Replace(":", "_");

                var nombreReporte = $"Reporte_Cita_{codigoSeguro}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                nombreReporteGenerado = nombreReporte;

                var rutaCompleta = Path.Combine(reportesPath, nombreReporte);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await reportePdfFile.CopyToAsync(stream);
                }

                rutaReporteGuardado = $"/reportes/{nombreReporte}";
            }

            // ============================
            // 2. GUARDAR IMÁGENES SOLO AL GENERAR REPORTE
            // ============================

            var uploadsAnalisisPath = Path.Combine(_environment.WebRootPath, "uploads", "analisis");

            if (!Directory.Exists(uploadsAnalisisPath))
                Directory.CreateDirectory(uploadsAnalisisPath);

            var codigoSeguroImagen = string.IsNullOrWhiteSpace(cita.CitaExternaId)
                ? $"CITA_{cita.CitaId}"
                : cita.CitaExternaId
                    .Replace("/", "_")
                    .Replace("\\", "_")
                    .Replace(" ", "_")
                    .Replace(":", "_");

            if (imagenIzquierdaFile != null && imagenIzquierdaFile.Length > 0)
            {
                var extensionIzq = Path.GetExtension(imagenIzquierdaFile.FileName).ToLowerInvariant();

                if (extensionIzq != ".jpg" && extensionIzq != ".jpeg")
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "La imagen del ojo izquierdo debe estar en formato JPG/JPEG."
                    });
                }

                var nombreImagenIzq = $"{codigoSeguroImagen}_ojo_izquierdo_{DateTime.Now:yyyyMMdd_HHmmss}{extensionIzq}";
                var rutaImagenIzq = Path.Combine(uploadsAnalisisPath, nombreImagenIzq);

                using (var stream = new FileStream(rutaImagenIzq, FileMode.Create))
                {
                    await imagenIzquierdaFile.CopyToAsync(stream);
                }

                cita.ImagenOjoIzquierdo = $"/uploads/analisis/{nombreImagenIzq}";
                cita.FechaCargaOjoIzquierdo = DateTime.UtcNow;
            }
            else
            {
                cita.ImagenOjoIzquierdo = null;
                cita.FechaCargaOjoIzquierdo = null;
            }

            if (imagenDerechaFile != null && imagenDerechaFile.Length > 0)
            {
                var extensionDer = Path.GetExtension(imagenDerechaFile.FileName).ToLowerInvariant();

                if (extensionDer != ".jpg" && extensionDer != ".jpeg")
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "La imagen del ojo derecho debe estar en formato JPG/JPEG."
                    });
                }

                var nombreImagenDer = $"{codigoSeguroImagen}_ojo_derecho_{DateTime.Now:yyyyMMdd_HHmmss}{extensionDer}";
                var rutaImagenDer = Path.Combine(uploadsAnalisisPath, nombreImagenDer);

                using (var stream = new FileStream(rutaImagenDer, FileMode.Create))
                {
                    await imagenDerechaFile.CopyToAsync(stream);
                }

                cita.ImagenOjoDerecho = $"/uploads/analisis/{nombreImagenDer}";
                cita.FechaCargaOjoDerecho = DateTime.UtcNow;
            }
            else
            {
                cita.ImagenOjoDerecho = null;
                cita.FechaCargaOjoDerecho = null;
            }

            // ============================
            // 3. GUARDAR RESULTADOS DEL ANÁLISIS
            // ============================

            cita.ResultadoOjoIzquierdo = resultadoIzq;
            cita.ResultadoOjoDerecho = resultadoDer;
            cita.ConfianzaOjoIzquierdo = confianzaIzq;
            cita.ConfianzaOjoDerecho = confianzaDer;

            cita.RecomendacionModelo = recomendacion;
            cita.TipoRetinopatia = tipo;
            cita.GradoRetinopatia = grado;

            cita.ObservacionesOjoIzquierdo = obsIzq;
            cita.ObservacionesOjoDerecho = obsDer;

            var medicoLogin =
    User.FindFirstValue("LoginClinicOnline") ??
    User.FindFirstValue(ClaimTypes.Name) ??
    "Médico";

            cita.ReportePdf = rutaReporteGuardado;
            cita.EstadoCita = "finalizado";
            cita.FechaAnalisis = DateTime.UtcNow;
            cita.MedicoResponsable = medicoLogin;

            var pacienteNombre = cita.Usuario != null
                ? $"{cita.Usuario.NombreUsuario} {cita.Usuario.ApellidoUsuario}"
                : cita.PacienteExternoId;

            _context.AuditoriaLogs.Add(new AuditoriaLog
            {
                TipoRegistro = "analisis",
                UsuarioId = cita.UsuarioId,
                Usuario = pacienteNombre,
                Rol = "Paciente",
                CitaExternaId = cita.CitaExternaId,
                Paciente = pacienteNombre,
                MedicoResponsable = cita.MedicoResponsable,
                FechaAnalisis = cita.FechaAnalisis,
                Descripcion = "Resultado de análisis generado y reporte PDF finalizado.",
                FechaCreado = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            // ============================
            // 4. REGISTRO OPCIONAL EN CLINIC ONLINE
            // ============================
            // Si en appsettings.json tienes:
            // "EnviarReporteAClinicOnline": false
            // NO se envía nada a Clinic Online.

            var enviarAClinicOnline = HttpContext.RequestServices
                .GetRequiredService<IConfiguration>()
                .GetValue<bool>("ClinicOnline:EnviarReporteAClinicOnline");

            if (enviarAClinicOnline &&
                !string.IsNullOrWhiteSpace(cita.CitaExternaId) &&
                !string.IsNullOrWhiteSpace(nombreReporteGenerado))
            {
                await _clinicOnlineReporteService.RegistrarReporteAsync(
                    cita.CitaExternaId,
                    cita.FechaAnalisis ?? DateTime.UtcNow,
                    nombreReporteGenerado
                );
            }

            return new JsonResult(new
            {
                success = true,
                reportePdf = rutaReporteGuardado
            });
        }

        private sealed class ModelPredictionResponse
        {
            public bool Success { get; set; }
            public string? Label { get; set; }
            public double Confidence { get; set; }
            public double Probability_Positive { get; set; }
            public double Probability_Negative { get; set; }
            public double Logit { get; set; }
            public string? Message { get; set; }
        }
    }
}