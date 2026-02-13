# GestionApp - Sistema de Gestión

Aplicación de escritorio desarrollada en **C# con WPF** para la gestión de dinero, productos y más.

## 🛠️ Requisitos

- .NET 8.0 SDK o superior
- Visual Studio Code con extensión C# Dev Kit (o Visual Studio 2022)
- Windows 10/11

## 📁 Estructura del Proyecto

```
GestionApp/
├── Data/               # Contexto de base de datos y configuración
├── Helpers/            # Clases auxiliares (RelayCommand, etc.)
├── Models/             # Modelos de datos (Producto, Categoría, Transacción)
├── Services/           # Servicios de lógica de negocio
├── ViewModels/         # ViewModels para el patrón MVVM
├── Views/              # Vistas XAML adicionales
├── App.xaml            # Configuración de la aplicación
├── MainWindow.xaml     # Ventana principal
└── GestionApp.csproj   # Archivo de proyecto
```

## 🚀 Instalación

1. **Restaurar paquetes NuGet:**
   ```bash
   dotnet restore
   ```

2. **Compilar el proyecto:**
   ```bash
   dotnet build
   ```

3. **Ejecutar la aplicación:**
   ```bash
   dotnet run
   ```

## 📦 Paquetes Utilizados

- **Microsoft.EntityFrameworkCore.Sqlite** - Base de datos SQLite
- **Microsoft.EntityFrameworkCore.Tools** - Herramientas para migraciones
- **CommunityToolkit.Mvvm** - Herramientas MVVM de Microsoft

## 💾 Base de Datos

La aplicación utiliza SQLite como base de datos local. El archivo de base de datos se guarda en:
```
%LOCALAPPDATA%\GestionApp\gestion.db
```

### Crear la primera migración (después de restaurar paquetes):
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## 🎯 Funcionalidades (Por implementar)

- [ ] Gestión de productos
- [ ] Gestión de categorías
- [ ] Control de ingresos y egresos
- [ ] Reportes y estadísticas
- [ ] Exportación de datos

## 📝 Notas

Este proyecto utiliza el patrón **MVVM (Model-View-ViewModel)** para una mejor separación de responsabilidades y facilitar las pruebas unitarias.

---

*Desarrollado con ❤️ usando C# y WPF*
