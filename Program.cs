using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using ProyectoDeGradoFundacion.Data;
using ProyectoDeGradoFundacion.Services;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Conexión a BD
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 🔹 Razor Pages
builder.Services.AddRazorPages();

// 🔹 DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)))
);

// 🔥 AUTENTICACIÓN
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/AccesoDenegado";
    });

// 🔥 SERVICIOS
builder.Services.AddScoped<AuthService>();

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
