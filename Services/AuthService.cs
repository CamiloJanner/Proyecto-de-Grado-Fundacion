using Microsoft.EntityFrameworkCore;
using ProyectoDeGradoFundacion.Data;
using ProyectoDeGradoFundacion.Models;

namespace ProyectoDeGradoFundacion.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔐 LOGIN REAL
        public async Task<Usuario?> Login(string email, string password)
        {
            var user = await _context.Usuarios
                .Include(u => u.Rol) // 🔥 IMPORTANTE: cargar el rol
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return null;

            // ⚠️ VALIDACIÓN SIMPLE (luego lo mejoras con hash)
            if (user.PasswordHash != password)
                return null;

            return user;
        }
    }
}

