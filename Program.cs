using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using ProyectoDeGradoFundacion.Data;
using ProyectoDeGradoFundacion.Services;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Conexión a BD AIVEN
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 🔹 Razor Pages
builder.Services.AddRazorPages();

// 🔹 DbContext AIVEN
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)))
);

// 🔥 AUTENTICACIÓN
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Tu login real está en Pages/Login/inicio.cshtml
        options.LoginPath = "/Login/inicio";
        options.AccessDeniedPath = "/AccesoDenegado";

        // 15 minutos de sesión según el requerimiento de inactividad
        options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
        options.SlidingExpiration = true;
    });

// 🔥 SERVICIOS
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ClinicOnlineCitasService>();
builder.Services.AddScoped<ClinicOnlineReporteService>();
builder.Services.AddScoped<ClinicOnlineUsuariosService>();
builder.Services.AddScoped<ClinicOnlineAuthService>();

var app = builder.Build();

// 🔹 Manejo de errores
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🔥 ORDEN CLAVE
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
