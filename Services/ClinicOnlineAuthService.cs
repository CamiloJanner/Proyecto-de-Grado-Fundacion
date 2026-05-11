using MySqlConnector;

namespace ProyectoDeGradoFundacion.Services
{
    public class ClinicOnlineAuthService
    {
        private readonly IConfiguration _configuration;

        public ClinicOnlineAuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ClinicOnlineLoginResult?> ValidarUsuarioAsync(
            string identificador,
            string contrasena)
        {
            var connectionString = _configuration.GetConnectionString("ClinicOnlineConnection");

            await using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            /*
             * SOLO LECTURA SOBRE CLINIC ONLINE:
             * No hace INSERT, UPDATE ni DELETE.
             *
             * Permite iniciar sesión usando:
             * - login
             * - o codempleados
             *
             * La clave se valida igual que en MySQL Workbench:
             * clave = SHA1('contraseña')
             */
            var sql = @"
                SELECT 
                    codempleados,
                    login
                FROM unab1
                WHERE 
                    (login = @identificador OR CAST(codempleados AS CHAR) = @identificador)
                    AND clave = SHA1(@contrasena)
                LIMIT 1;
            ";

            await using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@identificador", identificador.Trim());
            command.Parameters.AddWithValue("@contrasena", contrasena);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new ClinicOnlineLoginResult
                {
                    CodigoEmpleado = reader["codempleados"]?.ToString() ?? "",
                    Login = reader["login"]?.ToString() ?? ""
                };
            }

            return null;
        }
    }

    public class ClinicOnlineLoginResult
    {
        public string CodigoEmpleado { get; set; } = "";
        public string Login { get; set; } = "";
    }
}