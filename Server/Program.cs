using Microsoft.EntityFrameworkCore;
using SSTDigitalRD.Server.Data;
using SSTDigitalRD.Server.Models;
using SSTDigitalRD.Server.Services;


var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community; //Configuracion de licencia PDF

// ── Servicios ──────────────────────────────────────────────
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Entity Framework + SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        sql => sql.EnableRetryOnFailure(3)));

// Servicios propios
builder.Services.AddScoped<IHashService, HashService>();
builder.Services.AddScoped<IGeofencingService, GeofencingService>();

builder.Services.AddScoped<IPdfService, PdfService>();

builder.Services.AddScoped<IAuthService, AuthService>();  //Auth


var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(
    Microsoft.AspNetCore.Authentication.JwtBearer
        .JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new Microsoft.IdentityModel.Tokens
                .TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                new Microsoft.IdentityModel.Tokens
                    .SymmetricSecurityKey(
                    System.Text.Encoding.UTF8
                        .GetBytes(jwtKey)),
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = builder.Configuration["Jwt:Audience"],
                ValidateLifetime = true
            };
    });

builder.Services.AddAuthorization();

//builder.Services.AddSingleton<IYoloService, YoloService>();


//SixLabors.ImageSharp.Configuration.Default.MaxDegreeOfParallelism = 1;

// Swagger para pruebas en desarrollo
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ── Middleware ─────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); //Auth
app.UseAuthorization(); //Auth

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

// ── Crear base de datos automáticamente en desarrollo ──────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // Seed obra de pruebas si no existe ninguna
    if (!db.ObrasActivas.Any())
    {
        db.ObrasActivas.Add(new ObraActiva
        {
            Nombre = "Invivienda — Edificio Central",
            Direccion = "Av. Tiradentes esq. Ortega y Gasset, D.N.",
            Latitud = 18.4762,
            Longitud = -69.9312,
            RadioGeofencing = 50000, // 50km para pruebas
            Activa = true
        });
        db.SaveChanges();
    }
}

app.Run();
