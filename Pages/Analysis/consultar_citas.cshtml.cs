using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoDeGradoFundacion.Data;
using ProyectoDeGradoFundacion.Models;
using ProyectoDeGradoFundacion.Services;

namespace ProyectoDeGradoFundacion.Pages.Analysis
{
    public class consultar_citasModel : PageModel
    {
        private readonly ClinicOnlineCitasService _clinicOnlineCitasService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<consultar_citasModel> _logger;

        public consultar_citasModel(
            ClinicOnlineCitasService clinicOnlineCitasService,
            ApplicationDbContext context,
            ILogger<consultar_citasModel> logger)
        {
            _clinicOnlineCitasService = clinicOnlineCitasService;
            _context = context;
            _logger = logger;
        }

        public List<ClinicOnlineCitaDto> Citas { get; set; } = new();

        public string? MensajeInformativo { get; set; }

        public bool ErrorComunicacion { get; set; }

        // ============================
        // FILTROS
        // ============================

        [BindProperty(SupportsGet = true)]
        public string? FilterCodigo { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FilterNombre { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FilterApellido { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FilterTipo { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FilterDocumento { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FilterFecha { get; set; }

        // ============================
        // PAGINACIÓN
        // ============================

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalRegistros { get; set; }

        public int TotalPaginas { get; set; }

        public bool TienePaginaAnterior => PageNumber > 1;

        public bool TienePaginaSiguiente => PageNumber < TotalPaginas;

        public async Task OnGetAsync()
        {
            try
            {
                if (PageNumber < 1)
                {
                    PageNumber = 1;
                }

                var filtro = new ClinicOnlineCitaFiltro
                {
                    Codigo = FilterCodigo,
                    Nombre = FilterNombre,
                    Apellido = FilterApellido,
                    TipoDocumento = FilterTipo,
                    Documento = FilterDocumento,
                    Fecha = FilterFecha
                };

                /*
                 * 1. Traer citas desde Clinic Online.
                 * Esto NO modifica la base de datos de Clinic Online.
                 */
                var citasClinicOnline = await _clinicOnlineCitasService.ObtenerCitasAsync(filtro);

                /*
                 * 2. Buscar en Aiven las citas que ya fueron finalizadas.
                 * Esas ya tienen reporte generado y no deben volver a salir
                 * en "Consultar citas".
                 */
                var codigosCitasFinalizadas = await _context.Citas
                    .Where(c =>
                        c.EstadoCita == "finalizado" &&
                        c.CitaExternaId != null)
                    .Select(c => c.CitaExternaId!)
                    .ToListAsync();

                /*
                 * 3. Ocultar de la lista las citas de Clinic Online
                 * que ya estén finalizadas en Aiven.
                 */
                var citasFiltradas = citasClinicOnline
                    .Where(c => !codigosCitasFinalizadas.Contains(c.CodigoCita))
                    .ToList();

                TotalRegistros = citasFiltradas.Count;

                TotalPaginas = (int)Math.Ceiling(TotalRegistros / (double)PageSize);

                if (TotalPaginas == 0)
                {
                    TotalPaginas = 1;
                }

                if (PageNumber > TotalPaginas)
                {
                    PageNumber = TotalPaginas;
                }

                Citas = citasFiltradas
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                if (!citasFiltradas.Any())
                {
                    MensajeInformativo = "No hay citas disponibles para análisis de IA con los criterios consultados.";
                }
            }
            catch (Exception ex)
            {
                ErrorComunicacion = true;
                MensajeInformativo = "Ocurrió un error de comunicación con Clinic Online. Intente nuevamente.";

                _logger.LogError(ex, "Error consultando citas desde Clinic Online o filtrando citas finalizadas en Aiven.");
            }
        }
    }
}