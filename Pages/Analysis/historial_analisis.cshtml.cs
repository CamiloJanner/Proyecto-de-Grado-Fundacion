using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoDeGradoFundacion.Data;
using ProyectoDeGradoFundacion.Models;

namespace ProyectoDeGradoFundacion.Pages.Analysis
{
    public class historial_analisisModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public historial_analisisModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Cita> Historial { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? FilterNombre { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FilterFecha { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FilterResultado { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FilterMedico { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Citas
                .Include(c => c.Usuario)
                .Where(c =>
                    c.EstadoCita == "finalizado" &&
                    !string.IsNullOrEmpty(c.ReportePdf))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(FilterNombre))
            {
                query = query.Where(c =>
                    (c.Usuario.NombreUsuario + " " + c.Usuario.ApellidoUsuario)
                    .Contains(FilterNombre));
            }

            if (FilterFecha.HasValue)
            {
                var inicio = FilterFecha.Value.Date;
                var fin = inicio.AddDays(1);

                query = query.Where(c =>
                    c.FechaAnalisis >= inicio &&
                    c.FechaAnalisis < fin);
            }

            if (!string.IsNullOrWhiteSpace(FilterResultado))
            {
                query = query.Where(c =>
                    (c.ResultadoOjoIzquierdo != null && c.ResultadoOjoIzquierdo.Contains(FilterResultado)) ||
                    (c.ResultadoOjoDerecho != null && c.ResultadoOjoDerecho.Contains(FilterResultado)));
            }

            if (!string.IsNullOrWhiteSpace(FilterMedico))
            {
                query = query.Where(c =>
                    c.MedicoResponsable != null &&
                    c.MedicoResponsable.Contains(FilterMedico));
            }

            Historial = await query
                .OrderByDescending(c => c.FechaAnalisis)
                .ToListAsync();
        }
    }
}