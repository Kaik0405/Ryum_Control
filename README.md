# GestionApp — Sistema de Gestión de Distribución

Aplicación de escritorio en **C# + WPF (.NET 8)** orientada a la operación diaria de un negocio de distribución: inventario, combos, entregas, costos y movimientos. El foco está en una arquitectura limpia, reglas de negocio explícitas y una UX consistente.

## Estado del proyecto

**En desarrollo activo.** El sistema está en evolución y se continúan incorporando funcionalidades y refinamientos. Este repositorio refleja trabajo en curso.

## Características clave

- Gestión de inventario con control de stock y movimientos.
- Administración de combos/agrupaciones y su relación con productos.
- Flujo de entregas con prioridades y alertas por fechas.
- Persistencia local con SQLite y EF Core.
- Navegación modular y UI reactiva mediante MVVM.

## Arquitectura y diseño

- **MVVM** con separación estricta entre Vista, ViewModel y Modelo.
- **Inyección de dependencias** para servicios, navegación y ViewModels.
- **Servicios de dominio** para encapsular reglas de negocio.
- **Soft-delete** en entidades sensibles para trazabilidad.

## Requisitos del entorno

- **Windows 10/11** (WPF es exclusivo de Windows).
- **.NET SDK 8.0** o superior.

## Instalación y ejecución

```bash
dotnet restore
dotnet build
dotnet run
```

Si el proceso está bloqueando la compilación:

```bash
taskkill /F /IM GestionApp.exe
dotnet build
```

## Base de datos

SQLite local creada automáticamente al iniciar la aplicación mediante `EnsureCreated()`:

```
%APPDATA%\GestionApp\gestion.db
```

> **Nota técnica:** si cambias modelos o relaciones, elimina `gestion.db` y vuelve a ejecutar. No se utilizan migraciones en este flujo.

## Estructura del repositorio

```
GestionApp/
├── Data/              # DbContext y configuración EF Core
├── Helpers/           # Commands, converters y utilidades MVVM
├── Models/            # Entidades de dominio
├── Services/          # Servicios de negocio y navegación
├── ViewModels/        # Lógica de presentación
├── Views/             # Vistas WPF (XAML)
├── App.xaml           # Recursos y DataTemplates
├── MainWindow.xaml    # Contenedor principal
└── GestionApp.csproj
```

## Paquetes principales

| Paquete | Propósito |
|---|---|
| Microsoft.EntityFrameworkCore.Sqlite | Persistencia SQLite |
| Microsoft.EntityFrameworkCore.Tools | Herramientas EF Core |
| Microsoft.Extensions.DependencyInjection | Contenedor DI |

## Guía rápida de desarrollo

- Mantener la lógica en **servicios** y ViewModels; las vistas deben ser “tontas”.
- Los formularios usan `string` en campos numéricos para evitar fallos por formato decimal.
- Las operaciones críticas registran movimientos para auditoría.

## Documentación

- Detalles técnicos ampliados en `docs/DOCUMENTACION.md`.

## Contribución

Si deseas colaborar, abre un issue describiendo el cambio propuesto. Las PRs deben incluir una descripción técnica clara y pruebas manuales mínimas.

---

*Desarrollado con C# y WPF (.NET 8).* 
