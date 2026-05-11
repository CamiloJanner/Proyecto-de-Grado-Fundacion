using MySqlConnector;
using ProyectoDeGradoFundacion.Models;
using System.Runtime.InteropServices;

namespace ProyectoDeGradoFundacion.Services
{
    public class ClinicOnlineCitasService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ClinicOnlineCitasService> _logger;

        public ClinicOnlineCitasService(
            IConfiguration configuration,
            ILogger<ClinicOnlineCitasService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<ClinicOnlineCitaDto>> ObtenerCitasAsync(
            ClinicOnlineCitaFiltro filtro,
            CancellationToken cancellationToken = default)
        {
            var connectionString = _configuration.GetConnectionString("ClinicOnlineConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("No se encontró la conexión ClinicOnlineConnection.");
            }

            var citas = new List<ClinicOnlineCitaDto>();

            var fechaActual = ObtenerFechaColombia().Date;
            var fechaLimite = fechaActual.AddDays(3);

            await using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            var sql = @"
    SELECT
        codasig_cita,
        nombre1,
        nombre2,
        apellido1,
        apellido2,
        fecha_cita,
        nrodoc,
        codtipo_doc
    FROM unab2
    WHERE fecha_cita >= @fechaActual
      AND fecha_cita < @fechaLimite
";

            if (!string.IsNullOrWhiteSpace(filtro.Codigo))
            {
                sql += " AND codasig_cita LIKE @codigo ";
            }

            if (!string.IsNullOrWhiteSpace(filtro.Nombre))
            {
                sql += @" AND (
                            nombre1 LIKE @nombre
                            OR nombre2 LIKE @nombre
                         ) ";
            }

            if (!string.IsNullOrWhiteSpace(filtro.Apellido))
            {
                sql += @" AND (
                            apellido1 LIKE @apellido
                            OR apellido2 LIKE @apellido
                         ) ";
            }

            if (!string.IsNullOrWhiteSpace(filtro.TipoDocumento))
            {
                sql += " AND codtipo_doc LIKE @tipoDocumento ";
            }

            if (!string.IsNullOrWhiteSpace(filtro.Documento))
            {
                sql += " AND nrodoc LIKE @documento ";
            }

            if (filtro.Fecha.HasValue)
            {
                sql += " AND fecha_cita = @fechaFiltro ";
            }

            sql += " ORDER BY fecha_cita ASC, apellido1 ASC, nombre1 ASC;";

            await using var command = new MySqlCommand(sql, connection);
            command.CommandTimeout = 15;

            command.Parameters.AddWithValue("@fechaActual", fechaActual);
            command.Parameters.AddWithValue("@fechaLimite", fechaLimite);

            if (!string.IsNullOrWhiteSpace(filtro.Codigo))
            {
                command.Parameters.AddWithValue("@codigo", $"%{filtro.Codigo}%");
            }

            if (!string.IsNullOrWhiteSpace(filtro.Nombre))
            {
                command.Parameters.AddWithValue("@nombre", $"%{filtro.Nombre}%");
            }

            if (!string.IsNullOrWhiteSpace(filtro.Apellido))
            {
                command.Parameters.AddWithValue("@apellido", $"%{filtro.Apellido}%");
            }

            if (!string.IsNullOrWhiteSpace(filtro.TipoDocumento))
            {
                command.Parameters.AddWithValue("@tipoDocumento", $"%{filtro.TipoDocumento}%");
            }

            if (!string.IsNullOrWhiteSpace(filtro.Documento))
            {
                command.Parameters.AddWithValue("@documento", $"%{filtro.Documento}%");
            }

            if (filtro.Fecha.HasValue)
            {
                command.Parameters.AddWithValue("@fechaFiltro", filtro.Fecha.Value.Date);
            }

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var cita = new ClinicOnlineCitaDto
                {
                    CodigoCita = reader["codasig_cita"]?.ToString() ?? string.Empty,
                    Nombre1 = reader["nombre1"]?.ToString() ?? string.Empty,
                    Nombre2 = reader["nombre2"]?.ToString() ?? string.Empty,
                    Apellido1 = reader["apellido1"]?.ToString() ?? string.Empty,
                    Apellido2 = reader["apellido2"]?.ToString() ?? string.Empty,
                    FechaCita = reader["fecha_cita"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["fecha_cita"]),
                    Documento = reader["nrodoc"]?.ToString() ?? string.Empty,
                    TipoDocumento = reader["codtipo_doc"]?.ToString() ?? string.Empty
                };

                citas.Add(cita);
            }

            return citas;
        }

        private static DateTime ObtenerFechaColombia()
        {
            var zonaHoraria = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "SA Pacific Standard Time"
                : "America/Bogota";

            var zonaColombia = TimeZoneInfo.FindSystemTimeZoneById(zonaHoraria);

            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaColombia);
        }
    }
}
