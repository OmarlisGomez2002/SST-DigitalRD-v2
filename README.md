# SST-Digital RD
**Plataforma inteligente de Seguridad y Salud en el Trabajo**
Proyecto de Grado · UNIBE 2025 · Ramón Antonio Gómez Montero

---

## Requisitos previos
- [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- SQL Server 2019+ (o SQL Server Express)
- Visual Studio 2022 o VS Code con extensión C#

---

## Estructura del proyecto

```
SSTDigitalRD/
├── Client/          ← Blazor WebAssembly (Frontend)
│   ├── Pages/       ← Páginas (@page "/ruta")
│   ├── Shared/      ← Componentes reutilizables
│   └── wwwroot/     ← CSS, JS, imágenes
├── Server/          ← ASP.NET Core (Backend / API)
│   ├── Controllers/ ← API Endpoints
│   ├── Data/        ← DbContext + Migrations
│   ├── Models/      ← Entidades EF Core
│   └── Services/    ← Lógica de negocio
├── Shared/          ← DTOs y modelos compartidos Client↔Server
└── SSTDigitalRD.sln
```

---

## Cómo ejecutar

```bash
# 1. Clonar o descomprimir el proyecto
cd SSTDigitalRD

# 2. Restaurar paquetes
dotnet restore

# 3. Ejecutar (desde la raíz de la solución)
dotnet run --project Server
```

La app abre en `https://localhost:5001` por defecto.

---

## Plan de módulos

| # | Módulo              | Estado     |
|---|---------------------|------------|
| 1 | Dashboard           | ✅ Listo   |
| 2 | Inspecciones        | 🔜 Siguiente|
| 3 | Registro de charlas | ⏳ Pendiente|
| 4 | Inventario EPP      | ⏳ Pendiente|
| 5 | Incidentes          | ⏳ Pendiente|
| 6 | Dossier MTRAB       | ⏳ Pendiente|
| 7 | IA Predictiva       | ⏳ Pendiente|
| 8 | Visión en obra      | ⏳ Pendiente|

---

## Tecnologías

- **Frontend**: Blazor WebAssembly .NET 7
- **Backend**: ASP.NET Core 7
- **Base de datos**: SQL Server + Entity Framework Core 7
- **Autenticación**: ASP.NET Core Identity + JWT
- **IA**: ML.NET · YOLOv4-Tiny (detección EPP)
- **Gráficos**: Chart.js (via JS Interop)
- **Iconos**: Tabler Icons
