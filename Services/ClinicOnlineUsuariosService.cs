using MySqlConnector;

namespace ProyectoDeGradoFundacion.Services
{
    public class ClinicOnlineUsuariosService
    {
        private readonly IConfiguration _configuration;

        public ClinicOnlineUsuariosService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<ClinicOnlineUsuarioDto>> ObtenerUsuariosClinicAsync()
        {
            var usuarios = new List<ClinicOnlineUsuarioDto>();

            var connectionString = _configuration.GetConnectionString("ClinicOnlineConnection");

            await using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            var sql = @"
                SELECT 
                    codempleados,
                    login
                FROM unab1
                ORDER BY codempleados;
            ";

            await using var command = new MySqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                usuarios.Add(new ClinicOnlineUsuarioDto
                {
                    CodigoEmpleado = reader["codempleados"]?.ToString() ?? "",
                    Login = reader["login"]?.ToString() ?? ""
                });
            }

            return usuarios;
        }
    }

    public class ClinicOnlineUsuarioDto
    {
        public string CodigoEmpleado { get; set; } = "";
        public string Login { get; set; } = "";
    }
}