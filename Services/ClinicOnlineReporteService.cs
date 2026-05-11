using MySqlConnector;

namespace ProyectoDeGradoFundacion.Services
{
    public class ClinicOnlineReporteService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ClinicOnlineReporteService> _logger;

        public ClinicOnlineReporteService(
            IConfiguration configuration,
            ILogger<ClinicOnlineReporteService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task RegistrarReporteAsync(
            string codigoCita,
            DateTime fechaHora,
            string archivo,
            CancellationToken cancellationToken = default)
        {
            var connectionString = _configuration.GetConnectionString("ClinicOnlineConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("No se encontró la conexión ClinicOnlineConnection.");
            }

            await using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            /*
             * Esta es la tabla que el cliente indicó para insertar el PDF:
             * asig_cita_pdfunab(codasig_cita, fechahora, archivo)
             */
            var sql = @"
                INSERT INTO asig_cita_pdfunab
                (
                    codasig_cita,
                    fechahora,
                    archivo
                )
                VALUES
                (
                    @codasig_cita,
                    @fechahora,
                    @archivo
                );
            ";

            await using var command = new MySqlCommand(sql, connection);
            command.CommandTimeout = 30;

            command.Parameters.AddWithValue("@codasig_cita", codigoCita);
            command.Parameters.AddWithValue("@fechahora", fechaHora);
            command.Parameters.AddWithValue("@archivo", archivo);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
