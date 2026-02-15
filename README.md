# GestionApp - Sistema de Gestión de Distribución

Aplicación de escritorio desarrollada en **C# con WPF (.NET 8)** para la gestión integral de un negocio de distribución: inventario, combos, entregas con alertas de urgencia, fichas de costo, movimientos financieros y reportes.

## 🛠️ Requisitos

- .NET SDK 8.0 o superior
- Windows 10/11 (WPF es exclusivo de Windows)

## 📁 Estructura del Proyecto

```
GestionApp/
├── Data/
│   └── AppDbContext.cs              # EF Core — SQLite, relaciones, índices
├── Helpers/
│   ├── RelayCommand.cs              # Implementación de ICommand
│   ├── BaseViewModel.cs             # Clase base MVVM (SetProperty, navegación)
│   └── EntregaEstadoConverter.cs    # Converters de color/texto por estado de entrega
├── Models/
│   ├── Producto.cs                  # Producto de inventario + variantes
│   ├── Combo.cs                     # Combo/Agrego/Festejo + ComboProducto
│   ├── ModeloConformidad.cs         # Entrega + EntregaProducto (sistema de entregas)
│   ├── FichaCosto.cs                # Ficha de costo de envío
│   ├── Movimiento.cs                # Movimiento financiero (ingreso/egreso)
│   └── PeriodoInventario.cs         # Control mensual + snapshots
├── Services/
│   ├── Interfaces.cs                # Todas las interfaces de servicio
│   ├── ProductoService.cs           # CRUD productos + stock + soft-delete
│   ├── ComboService.cs              # CRUD combos + duplicar + productos
│   ├── EntregaService.cs            # CRUD entregas + urgencias + orden automática
│   ├── FichaCostoService.cs         # CRUD fichas + numeración automática
│   ├── MovimientoService.cs         # Movimientos + resúmenes + balance
│   ├── PeriodoInventarioService.cs  # Períodos mensuales + cierre
│   ├── ConfiguracionService.cs      # Configuración + Clientes + Agencias
│   └── NavigationService.cs         # Navegación entre ViewModels
├── ViewModels/
│   ├── MainViewModel.cs             # VM principal — sidebar y navegación
│   ├── DashboardViewModel.cs        # Dashboard (esqueleto)
│   ├── InventarioViewModel.cs       # ✅ CRUD completo, ajuste stock, historial (743 lín.)
│   ├── CombosViewModel.cs           # ✅ CRUD completo, tarjetas, duplicar (512 lín.)
│   ├── EntregasViewModel.cs         # ✅ Entregas con urgencia, filtros, CRUD (515 lín.)
│   ├── FichasCostoViewModel.cs      # Esqueleto
│   ├── MovimientosViewModel.cs      # Esqueleto
│   └── ReportesViewModel.cs         # Esqueleto
├── Views/
│   ├── InventarioView.xaml          # ✅ DataGrid + formulario + ajuste + historial
│   ├── CombosView.xaml              # ✅ Tarjetas con colores por tipo
│   ├── EntregasView.xaml            # ✅ DataGrid + urgencias + formulario + detalle
│   ├── DashboardView.xaml           # Placeholder
│   ├── FichasCostoView.xaml         # Placeholder
│   ├── MovimientosView.xaml         # Placeholder
│   └── ReportesView.xaml            # Placeholder
├── App.xaml                         # DataTemplates + estilos globales
├── App.xaml.cs                      # DI: servicios, ViewModels, DbContext
├── MainWindow.xaml                  # Sidebar + ContentControl
└── GestionApp.csproj                # Proyecto .NET 8
```

## 🚀 Instalación y Ejecución

```bash
# Restaurar paquetes
dotnet restore

# Compilar
dotnet build

# Ejecutar
dotnet run
```

Si la app está corriendo y no permite compilar:
```bash
taskkill /F /IM GestionApp.exe
dotnet build
```

## 📦 Paquetes Utilizados

| Paquete | Uso |
|---------|-----|
| Microsoft.EntityFrameworkCore.Sqlite | Base de datos SQLite |
| Microsoft.EntityFrameworkCore.Tools | Herramientas EF Core |
| Microsoft.Extensions.DependencyInjection | Inyección de dependencias |

## 💾 Base de Datos

SQLite local. Se crea automáticamente al iniciar la app (`EnsureCreated()`):
```
%APPDATA%\GestionApp\gestion.db
```

> **Importante:** Si cambias modelos o relaciones, debes **borrar `gestion.db`** y re-ejecutar. No se usan migraciones.

## 🎯 Módulos y Estado

| Módulo | Estado | Descripción |
|--------|--------|-------------|
| **Inventario** | ✅ Completo | CRUD productos, ajuste stock ±, historial movimientos, búsqueda, soft-delete |
| **Combos** | ✅ Completo | CRUD combos (Combo/Agrego/Festejo), tarjetas con colores, duplicar, numeración |
| **Entregas** | ✅ Completo | CRUD entregas, orden automática (ENT-00001), alertas urgencia 5 días, colores semáforo, crear desde combo |
| **Dashboard** | ⬜ Pendiente | Resumen general del negocio |
| **Fichas de Costo** | ⬜ Pendiente | Documentos de envío con desglose de costos y ganancia |
| **Movimientos** | ⬜ Pendiente | Ingresos/egresos con categorías y balance |
| **Reportes** | ⬜ Pendiente | Períodos mensuales, cierre, snapshots de inventario |

## 🏗️ Arquitectura

- **Patrón MVVM** — Separación estricta Vista ↔ ViewModel ↔ Modelo
- **Inyección de Dependencias** — Singleton (navegación), Scoped (servicios), Transient (ViewModels)
- **Navegación** — `NavigationService` + DataTemplates en App.xaml
- **Binding + Commands** — Toda la interacción UI pasa por propiedades bindables y `RelayCommand`
- **Soft-delete** — Productos no se eliminan, se desactivan

## 📝 Notas para Desarrollo

- Los campos numéricos del formulario son `string` para evitar crash con el punto decimal
- Al ajustar stock se registra automáticamente un movimiento financiero
- Los combos usan tarjetas con colores: 🟣 Combo, 🔵 Agrego, 🟠 Festejo
- Las entregas usan semáforo: 🔴 Vencida, 🟠 Urgente (≤2 días), 🟢 En tiempo, ⚫ Entregada
- Documentación técnica completa en `docs/DOCUMENTACION.md`

---

*Desarrollado con C# y WPF (.NET 8)*
