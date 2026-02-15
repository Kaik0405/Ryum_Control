# Documentación del Sistema de Gestión - GestionApp

> **Proyecto:** Aplicación de escritorio para gestión de negocio de distribución  
> **Tecnología:** C# / WPF / .NET 8.0 / SQLite  
> **Patrón:** MVVM (Model-View-ViewModel)  
> **Última actualización:** Febrero 2026

---

## Tabla de Contenidos

1. [Visión General](#1-visión-general)
2. [Estructura de Carpetas](#2-estructura-de-carpetas)
3. [Cómo Funciona la App (Flujo General)](#3-cómo-funciona-la-app)
4. [Modelos (Models/)](#4-modelos)
5. [Base de Datos (Data/)](#5-base-de-datos)
6. [Servicios (Services/)](#6-servicios)
7. [ViewModels (ViewModels/)](#7-viewmodels)
8. [Vistas (Views/)](#8-vistas)
9. [Navegación](#9-navegación)
10. [Inyección de Dependencias](#10-inyección-de-dependencias)
11. [Guía: "Quiero cambiar X"](#11-guía-quiero-cambiar-x)
12. [Módulos y Estado Actual](#12-módulos-y-estado-actual)
13. [Cómo Compilar y Ejecutar](#13-cómo-compilar-y-ejecutar)
14. [Problemas Conocidos y Soluciones](#14-problemas-conocidos-y-soluciones)

---

## 1. Visión General

GestionApp es una aplicación de escritorio que gestiona:

- **Inventario de productos** (con costos en CUP)
- **Combos** (agrupaciones de productos para envío, con precio en USD y numeración)
- **Entregas** (órdenes de entrega con alertas de urgencia a 5 días, colores semáforo)
- **Fichas de Costo** (documentos de venta/envío)
- **Movimientos financieros** (ingresos y egresos)
- **Períodos de inventario** (control mensual)
- **Clientes y Agencias** de envío

### Arquitectura en una frase:
> La **Vista** (XAML) muestra datos y captura clics → el **ViewModel** (C#) procesa la lógica y pide datos → el **Servicio** (C#) habla con la **Base de Datos** (SQLite) → los datos vuelven y la Vista se actualiza automáticamente.

---

## 2. Estructura de Carpetas

```
📁 Proyecto de Papi/
│
├── 📄 App.xaml                  ← Estilos globales + DataTemplates (conecta ViewModel → View)
├── 📄 App.xaml.cs               ← ARRANQUE: configura DI, crea BD, abre ventana
├── 📄 MainWindow.xaml           ← Ventana principal: sidebar + área de contenido
├── 📄 MainWindow.xaml.cs        ← Code-behind (vacío, todo va por MVVM)
├── 📄 GestionApp.csproj         ← Paquetes NuGet y configuración del proyecto
├── 📄 AssemblyInfo.cs           ← Metadatos del ensamblado
│
├── 📁 Models/                   ← DATOS: Las clases que representan las tablas de la BD
│   ├── Producto.cs              ← Producto del inventario
│   ├── Combo.cs                 ← Combo/Agrego/Festejo (con campo Numero)
│   ├── ComboProducto.cs         ← Producto dentro de un combo (tabla intermedia)
│   ├── CompraProducto.cs        ← Registro de compra para inventario
│   ├── Movimiento.cs            ← Movimiento financiero (ingreso/egreso)
│   ├── FichaCosto.cs            ← Ficha de costo de envío + FichaCostoProducto
│   ├── PeriodoInventario.cs     ← Período mensual + InventarioSnapshot
│   ├── Cliente.cs               ← Cliente/receptor
│   ├── Agencia.cs               ← Agencia de envío
│   ├── ModeloConformidad.cs     ← Entrega + EntregaProducto (sistema de entregas con urgencia)
│   └── ConfiguracionDistribuidor.cs ← Configuración global del negocio
│
├── 📁 Data/                     ← BASE DE DATOS
│   └── AppDbContext.cs          ← Contexto EF Core: tablas, relaciones, configuración SQLite
│
├── 📁 Services/                 ← LÓGICA DE NEGOCIO: operaciones CRUD y más
│   ├── Interfaces.cs            ← TODAS las interfaces (contratos) de servicios
│   ├── INavigationService.cs    ← Interfaz del servicio de navegación
│   ├── NavigationService.cs     ← Implementación de la navegación entre vistas
│   ├── ProductoService.cs       ← CRUD de productos
│   ├── ComboService.cs          ← CRUD de combos (con duplicar)
│   ├── EntregaService.cs        ← CRUD entregas + urgencia + crear desde combo + orden auto
│   ├── MovimientoService.cs     ← CRUD de movimientos + registrar venta/compra automática
│   ├── FichaCostoService.cs     ← CRUD de fichas + generar número de ficha
│   ├── PeriodoInventarioService.cs ← Gestión de períodos mensuales
│   └── ConfiguracionService.cs  ← Configuración + ClienteService + AgenciaService
│
├── 📁 ViewModels/               ← CEREBRO: lógica de cada pantalla
│   ├── BaseViewModel.cs         ← Clase base (INotifyPropertyChanged)
│   ├── MainViewModel.cs         ← ViewModel principal (navegación del sidebar)
│   ├── InventarioViewModel.cs   ← ✅ Completo: CRUD productos, stock, historial
│   ├── CombosViewModel.cs       ← ✅ Completo: CRUD combos con productos + Numero
│   ├── EntregasViewModel.cs     ← ✅ Completo: entregas, urgencia, filtros, CRUD (515 lín.)
│   ├── DashboardViewModel.cs    ← ⬜ Esqueleto (sin implementar)
│   ├── FichasCostoViewModel.cs  ← ⬜ Esqueleto
│   ├── MovimientosViewModel.cs  ← ⬜ Esqueleto
│   └── ReportesViewModel.cs     ← ⬜ Esqueleto
│
├── 📁 Views/                    ← INTERFAZ VISUAL: lo que ve el usuario
│   ├── InventarioView.xaml      ← ✅ DataGrid con productos, paneles laterales
│   ├── CombosView.xaml          ← ✅ Tarjetas de combos con Numero, formulario, detalle
│   ├── EntregasView.xaml        ← ✅ DataGrid + urgencias + formulario + detalle (594 lín.)
│   ├── DashboardView.xaml       ← ⬜ Placeholder
│   ├── FichasCostoView.xaml     ← ⬜ Placeholder
│   ├── MovimientosView.xaml     ← ⬜ Placeholder
│   └── ReportesView.xaml        ← ⬜ Placeholder
│   └── (cada .xaml tiene su .xaml.cs vacío)
│
├── 📁 Helpers/                  ← UTILIDADES
│   ├── RelayCommand.cs          ← Implementación de ICommand para botones
│   └── EntregaEstadoConverter.cs ← Converters: color/texto por estado de entrega
│
└── 📁 docs/                     ← DOCUMENTACIÓN
    └── DOCUMENTACION.md         ← Este archivo
```

---

## 3. Cómo Funciona la App

### El ciclo de vida al abrir la app:

```
1. Windows ejecuta GestionApp.exe
2. App.xaml.cs → constructor App()
   └── ConfigureServices() → registra todos los servicios y ViewModels en el contenedor DI
3. App.xaml.cs → OnStartup()
   ├── Crea la BD SQLite si no existe (Database.EnsureCreated)
   ├── Crea MainWindow
   ├── Le asigna MainViewModel como DataContext
   └── Muestra la ventana
4. MainViewModel → constructor
   └── Navega automáticamente a DashboardViewModel
5. WPF busca en App.xaml los DataTemplates
   └── Encuentra: DashboardViewModel → muestra DashboardView
6. El usuario ve el Dashboard en pantalla
```

### Cuando el usuario hace clic en "Inventario" del sidebar:

```
1. Clic en botón "Inventario"
2. WPF ejecuta NavigateToInventarioCommand (definido en MainViewModel)
3. MainViewModel → NavigateTo<InventarioViewModel>("Inventario")
4. NavigationService → crea nuevo InventarioViewModel vía DI
   └── DI inyecta: IProductoService, IMovimientoService, IPeriodoInventarioService
5. NavigationService → llama OnNavigatedTo() en InventarioViewModel
   └── CargarDatosAsync() → ProductoService.ObtenerTodosAsync() → BD → lista de productos
6. MainViewModel.CurrentViewModel cambia → la UI detecta el cambio
7. WPF busca DataTemplate para InventarioViewModel → muestra InventarioView
8. InventarioView se conecta al ViewModel vía Bindings
9. El DataGrid muestra ProductosFiltrados
```

### Cuando el usuario guarda un producto:

```
1. Clic en "Guardar" → ejecuta GuardarProductoCommand
2. InventarioViewModel.GuardarProducto()
   ├── Valida campos (nombre no vacío, número válido)
   ├── Parsea strings a decimal (CostoCompra, CantidadStock)
   ├── Si es nuevo: _productoService.CrearAsync(producto)
   │   └── ProductoService → _context.Productos.Add() → SaveChanges → BD
   │   └── Si tiene stock: registra Movimiento de egreso (gasto por compra)
   ├── Si es edición: _productoService.ActualizarAsync(producto)
   └── Recarga la lista: CargarDatosAsync()
3. La UI se actualiza automáticamente (ObservableCollection + PropertyChanged)
```

---

## 4. Modelos

Los modelos son las clases que representan las **tablas en la base de datos**. Están en la carpeta `Models/`.

### Producto (`Models/Producto.cs`)

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `Id` | int | Clave primaria (auto-generada) |
| `Nombre` | string | Nombre del producto (requerido, máx 200) |
| `Descripcion` | string? | Descripción opcional (máx 500) |
| `Unidad` | UnidadMedida | Enum: `Unidad`, `Libra`, `Kilogramo`, `Paquete` |
| `CostoCompra` | decimal | Costo de compra unitario en CUP |
| `CantidadStock` | decimal | Cantidad disponible |
| `EnStock` | bool | Si está disponible (default: true) |
| `FechaIngreso` | DateTime | Fecha de ingreso al inventario |
| `CostoTotal` | decimal | **Calculada**: CostoCompra × CantidadStock (no se guarda en BD) |
| `StockConUnidad` | string | **Calculada**: "5 Unidad", "2.5 Libra" |
| `Variantes` | ICollection | Variantes de precio del producto |
| `Compras` | ICollection | Historial de compras |
| `FechaCreacion` | DateTime | Fecha de creación del registro |
| `FechaModificacion` | DateTime? | Última modificación |
| `Activo` | bool | Soft-delete: false = eliminado (default: true) |

**Para modificar:**
- Agregar un campo → agregar propiedad en esta clase → agregar columna en la vista XAML → **borrar el archivo .db** para recrear la BD

### Combo (`Models/Combo.cs`)

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `Id` | int | Clave primaria |
| `Nombre` | string | Nombre del combo |
| `Descripcion` | string? | Descripción opcional |
| `Tipo` | TipoCombo | Enum: `Combo`, `Agrego`, `Festejo` |
| `PrecioVenta` | decimal | Precio de venta en USD |
| `Productos` | ICollection\<ComboProducto\> | Productos que componen el combo |
| `CostoTotal` | decimal | **Calculada**: suma de totales de productos |
| `Activo` | bool | Soft-delete |

### ComboProducto (`Models/ComboProducto.cs`)
Tabla intermedia que une un Combo con sus Productos.

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `ComboId` | int | FK al Combo |
| `ProductoId` | int | FK al Producto |
| `Cantidad` | decimal | Cuánto de ese producto lleva el combo |
| `Unidad` | UnidadMedida | Unidad para este producto en el combo |
| `CostoUnitario` | decimal | Costo al momento de crear el combo |
| `Total` | decimal | **Calculada**: Cantidad × CostoUnitario |

### Movimiento (`Models/Movimiento.cs`)
Registro de dinero que entra o sale.

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `Concepto` | string | Descripción corta (máx 300) |
| `Monto` | decimal | Cantidad de dinero (siempre positivo) |
| `Tipo` | TipoMovimiento | Enum: `Ingreso`, `Egreso` |
| `Categoria` | CategoriaMovimiento | Enum: `Venta`, `CompraProducto`, `Transporte`, `Remesa`, `FondoInicial`, `Otro` |
| `FichaCostoId` | int? | FK opcional a ficha de costo |
| `ProductoId` | int? | FK opcional a producto |
| `PeriodoInventarioId` | int? | FK opcional a período |
| `Fecha` | DateTime | Fecha del movimiento |

### FichaCosto (`Models/FichaCosto.cs`)
Documento completo de un envío/venta.

| Sección | Propiedades clave |
|---------|-------------------|
| **Identificación** | `NumeroFicha`, `TipoPedido`, `ComboId` |
| **Personas** | `Distribuidor`, `Remitente`, `ClienteId`, `NombreReceptor`, `DireccionReceptor`, `TelefonoReceptor` |
| **Envío** | `AgenciaId`, `NombreAgencia`, `FechaEnvio`, `FechaEntrega` |
| **Productos** | `Productos` (ICollection\<FichaCostoProducto\>), `PrecioVentaUSD`, `CostoTotal` (calc.), `Ganancia` (calc.) |

### PeriodoInventario (`Models/PeriodoInventario.cs`)
Control mensual de finanzas.

| Propiedad | Descripción |
|-----------|-------------|
| `Mes`, `Año` | Identifican el mes |
| `FondosIniciales`, `FondosFinales` | Dinero al inicio y cierre |
| `TotalIngresos`, `TotalEgresos` | Sumatorias del mes |
| `BalanceCalculado` | FondosIniciales + Ingresos - Egresos |
| `Cerrado` | Si ya se cerró el mes |
| `Movimientos`, `FichasCosto`, `Compras` | Colecciones relacionadas |

### Otros modelos:
- **Cliente** → `NombreCompleto`, `Direccion`, `Telefono`, `Email`, `Notas`
- **Agencia** → `Nombre`, `Direccion`, `Telefono`, `Notas`
- **ConfiguracionDistribuidor** → `Nombre`, `NombreNegocio`, `PrefijoFicha` ("FC-"), `UltimoNumeroFicha`, etc.
- **ModeloConformidad** → Ahora es **Entrega** (ver sección Entrega abajo)
- **CompraProducto** → Registro individual de compra con `Cantidad`, `CostoUnitario`, `Proveedor`

### Entrega (`Models/ModeloConformidad.cs`)

> **Nota:** El archivo se sigue llamando `ModeloConformidad.cs` pero contiene las clases `Entrega` y `EntregaProducto` (rediseño del sistema de conformidad → entregas).

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `Id` | int | Clave primaria |
| `NumeroOrden` | string | Número automático: "ENT-00001", "ENT-00002"... |
| `ComboId` | int | FK al combo que se envía |
| `NombreReceptor` | string | Nombre de quien recibe |
| `DireccionReceptor` | string | Dirección de entrega |
| `TelefonoMovil` | string? | Teléfono móvil del receptor |
| `TelefonoFijo` | string? | Teléfono fijo del receptor |
| `NombreRemitente` | string | Nombre de quien envía |
| `Agencia` | string? | Agencia de envío utilizada |
| `FechaOrden` | DateTime | Fecha en que se creó la orden (auto) |
| `FechaEntregada` | DateTime? | Fecha en que se entregó (null = pendiente) |
| `PlazoDias` | int | Días de plazo para entregar (default: 5) |
| `Notas` | string? | Observaciones opcionales |
| **Calculadas** | | |
| `FechaLimite` | DateTime | FechaOrden + PlazoDias |
| `DiasRestantes` | int | Días que faltan para vencer |
| `Vencida` | bool | true si pasó FechaLimite y no entregada |
| `Urgente` | bool | true si DiasRestantes ≤ 2 y no entregada |
| `Entregada` | bool | true si FechaEntregada tiene valor |
| `Resumen` | string | "ENT-00001 → Receptor (Combo)" |

### EntregaProducto (dentro de `Models/ModeloConformidad.cs`)

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `Id` | int | Clave primaria |
| `EntregaId` | int | FK a Entrega |
| `NombreProducto` | string | Nombre del producto copiado |
| `Unidad` | string | Unidad de medida copiada |
| `Cantidad` | decimal | Cantidad del producto |
| `Descripcion` | string | **Calculada**: "2 Unidad de Arroz" |

### Combo.Numero (campo nuevo en `Models/Combo.cs`)

Se agregó `Numero` (int) al modelo Combo para numerar los combos. Tiene un índice único en la BD.

---

## 5. Base de Datos

### Archivo: `Data/AppDbContext.cs`

Es el puente entre C# y SQLite usando Entity Framework Core.

**Ubicación de la BD:** `%APPDATA%/GestionApp/gestion.db`

### Tablas (DbSets):
```csharp
Productos              // Productos del inventario
ProductoVariantes      // Variantes de precio
ComprasProductos       // Historial de compras
Combos                 // Combos/Agregos/Festejos (con campo Numero único)
ComboProductos         // Productos dentro de combos
FichasCosto            // Fichas de costo
FichaCostoProductos    // Productos dentro de fichas
Entregas               // Entregas con alertas de urgencia (antes ModelosConformidad)
EntregaProductos       // Productos en entregas (antes ConformidadProductos)
Movimientos            // Movimientos financieros
PeriodosInventario     // Períodos mensuales
InventarioSnapshots    // Fotos del inventario al inicio/fin de mes
Clientes               // Clientes
Agencias               // Agencias de envío
Configuracion          // Configuración del distribuidor
```

### Relaciones clave (OnModelCreating):
```
Producto 1──N ProductoVariante      (Cascade: borrar producto = borra variantes)
Producto 1──N CompraProducto        (Restrict: no se puede borrar producto con compras)
Combo 1──N ComboProducto            (Cascade: borrar combo = borra sus productos)
ComboProducto N──1 Producto         (Restrict: no se borra producto en combo)
Combo.Numero                        (Índice único)
Entrega N──1 Combo                  (Restrict: no se borra combo con entregas)
EntregaProducto N──1 Entrega        (Cascade: borrar entrega = borra sus productos)
FichaCosto N──1 Entrega             (SetNull: si se borra entrega, ficha queda sin entrega)
FichaCosto N──1 Combo               (SetNull: si se borra combo, la ficha queda sin combo)
FichaCosto N──1 Cliente             (SetNull)
FichaCosto N──1 Agencia             (SetNull)
FichaCosto 1──N FichaCostoProducto  (Cascade)
Movimiento N──1 Producto            (SetNull)
Movimiento N──1 FichaCosto          (SetNull)
PeriodoInventario 1──N Movimiento   (SetNull)
PeriodoInventario + Mes + Año       (Índice único)
```

### ¿Qué tocar aquí?
- **Agregar nueva tabla:** Crear un `DbSet<MiModelo>` y configurar relaciones en `OnModelCreating`
- **Cambiar relaciones:** Modificar la sección correspondiente en `OnModelCreating`
- **IMPORTANTE:** Después de cambiar modelos o relaciones, **borrar el archivo `gestion.db`** para que se recree. Ruta: `%APPDATA%/GestionApp/gestion.db`

---

## 6. Servicios

Los servicios están en `Services/` y son la **capa de acceso a datos**. Cada servicio habla con la BD a través de `AppDbContext`.

### Patrón de Interfaces

Todas las interfaces están en **`Services/Interfaces.cs`** (un solo archivo).

**¿Por qué interfaces?** Para poder cambiar la implementación sin tocar el resto del código. El ViewModel pide `IProductoService`, no `ProductoService`. Esto permite:
- Testear con datos falsos (mock)
- Cambiar de SQLite a SQL Server sin tocar ViewModels
- Respetar el principio de inversión de dependencias

### Interface base genérica:
```csharp
IBaseService<T>
├── ObtenerTodosAsync()        → Lista completa
├── ObtenerPorIdAsync(int id)  → Un registro por ID
├── CrearAsync(T entidad)      → Crear nuevo
├── ActualizarAsync(T entidad) → Actualizar existente
└── EliminarAsync(int id)      → Eliminar (soft-delete en la mayoría)
```

### Interfaces específicas y sus métodos extra:

| Interfaz | Implementación | Métodos extra |
|----------|---------------|---------------|
| `IProductoService` | `ProductoService.cs` | `ObtenerEnStockAsync`, `BuscarPorNombreAsync`, `ActualizarStockAsync` |
| `IComboService` | `ComboService.cs` | `ObtenerPorTipoAsync`, `ObtenerConProductosAsync`, `DuplicarComboAsync` |
| `IEntregaService` | `EntregaService.cs` | `CrearDesdeComboAsync`, `ObtenerPendientesAsync`, `ObtenerUrgentesAsync`, `GenerarNumeroOrdenAsync`, `MarcarEntregadaAsync` |
| `IMovimientoService` | `MovimientoService.cs` | `ObtenerPorPeriodoAsync`, `ObtenerPorTipoAsync`, `ObtenerPorProductoAsync`, `ObtenerResumenAsync`, `ObtenerBalanceActualAsync`, `RegistrarVentaAsync`, `RegistrarCompraProductoAsync` |
| `IFichaCostoService` | `FichaCostoService.cs` | `ObtenerPorPeriodoAsync`, `ObtenerPorRangoFechaAsync`, `ObtenerConDetallesAsync`, `GenerarNumeroFichaAsync`, `ObtenerTotalVentasAsync` |
| `IPeriodoInventarioService` | `PeriodoInventarioService.cs` | `ObtenerPeriodoActualAsync`, `IniciarNuevoPeriodoAsync`, `CerrarPeriodoAsync`, `ObtenerPorMesAsync`, `GenerarSnapshotInventarioAsync` |
| `IConfiguracionService` | `ConfiguracionService.cs` | `ObtenerConfiguracionAsync`, `ActualizarConfiguracionAsync`, `ObtenerSiguienteNumeroFichaAsync` |
| `IClienteService` | `ConfiguracionService.cs` (mismo archivo) | `BuscarPorNombreAsync` |
| `IAgenciaService` | `ConfiguracionService.cs` (mismo archivo) | (solo CRUD base) |
| `INavigationService` | `NavigationService.cs` | `NavigateTo<T>`, `GoBack`, `CanGoBack` |

### Comportamientos automáticos importantes:

1. **ProductoService.EliminarAsync** → NO borra de la BD, marca `Activo = false` (soft-delete)
2. **ComboService.ActualizarAsync** → Borra todos los `ComboProducto` viejos y guarda los nuevos
3. **ComboService.DuplicarComboAsync** → Crea copia completa del combo con todos sus productos
4. **FichaCostoService.CrearAsync** → Genera número de ficha automático Y registra movimiento de ingreso
5. **MovimientoService.RegistrarVentaAsync** → Crea movimiento tipo Ingreso desde una ficha
6. **MovimientoService.RegistrarCompraProductoAsync** → Crea movimiento tipo Egreso desde una compra

### ¿Qué tocar aquí?

- **Agregar un nuevo método a un servicio existente:**
  1. Agregar la firma del método en la interfaz (`Interfaces.cs`)
  2. Implementar el método en el servicio correspondiente
  3. Llamarlo desde el ViewModel

- **Crear un nuevo servicio:**
  1. Crear la interfaz en `Interfaces.cs`
  2. Crear la clase de implementación en un nuevo archivo `.cs`
  3. Registrarlo en `App.xaml.cs` → `ConfigureServices()` con `services.AddScoped<IXxx, Xxx>()`

---

## 7. ViewModels

Los ViewModels son el **cerebro** de cada pantalla. Están en `ViewModels/`.

### BaseViewModel (`ViewModels/BaseViewModel.cs`)

Clase base que todos los ViewModels heredan. Proporciona:

```csharp
// 1. Notificación de cambios → la UI se actualiza automáticamente
OnPropertyChanged("NombrePropiedad")

// 2. Método helper para propiedades
SetProperty(ref _campo, valor)  // Cambia el valor Y notifica a la UI

// 3. Ciclo de vida
OnNavigatedTo(parameter)   // Se llama al entrar a la vista
OnNavigatedFrom()           // Se llama al salir de la vista
```

### MainViewModel (`ViewModels/MainViewModel.cs`)

ViewModel de la ventana principal. Controla:
- **Navegación del sidebar:** Un `ICommand` por cada botón del menú
- **CurrentViewModel:** El ViewModel activo (lo que se muestra en el área principal)
- **Titulo, StatusMessage, MenuSeleccionado:** Datos de la barra de estado

Flujo de navegación:
```csharp
// Cada botón del sidebar ejecuta:
NavigateTo<InventarioViewModel>("Inventario")
// Esto llama a NavigationService que:
//   1. Guarda el ViewModel actual en el historial
//   2. Crea uno nuevo vía DI
//   3. Llama OnNavigatedTo() en el nuevo
//   4. Actualiza CurrentViewModel → la UI cambia
```

### InventarioViewModel (`ViewModels/InventarioViewModel.cs`) — ✅ **COMPLETO** (743 líneas)

El ViewModel más completo del proyecto. Estructura:

```
InventarioViewModel
│
├── SERVICIOS inyectados:
│   ├── IProductoService        → CRUD de productos
│   ├── IMovimientoService      → Registrar gastos al crear/ajustar
│   └── IPeriodoInventarioService → (referencia futura)
│
├── PROPIEDADES DE DATOS:
│   ├── Productos               → Lista completa de la BD
│   ├── ProductosFiltrados      → Lista filtrada (la que ve el usuario)
│   ├── ProductoSeleccionado    → Fila seleccionada en el DataGrid
│   ├── Filtro                  → Texto del buscador
│   ├── MostrarSoloEnStock      → CheckBox filtro
│   └── CostoTotalInventario    → Suma total en el footer
│
├── PROPIEDADES DEL FORMULARIO:
│   ├── FormNombre, FormDescripcion
│   ├── FormCostoCompra (string) → Se parsea a decimal al guardar
│   ├── FormCantidadStock (string) → Se parsea a decimal al guardar
│   ├── FormUnidad              → Enum ComboBox
│   ├── MostrarFormulario       → Muestra/oculta panel lateral
│   └── EsEdicion               → Nuevo vs Editar
│
├── PROPS AJUSTE DE STOCK:
│   ├── ModoAjusteStock, AjusteCantidad, AjusteMotivo
│   └── AjusteEsIngreso (true = +, false = −)
│
├── PROPS HISTORIAL:
│   ├── ModoHistorial
│   ├── HistorialProducto       → Lista de movimientos del producto
│   └── ProductoHistorial       → Producto cuyo historial se muestra
│
└── COMANDOS (ICommand):
    ├── NuevoProductoCommand    → Abre formulario vacío
    ├── EditarProductoCommand   → Abre formulario con datos del producto
    ├── EliminarProductoCommand → Confirma y hace soft-delete
    ├── GuardarProductoCommand  → Valida, parsea, guarda en BD
    ├── CancelarCommand         → Cierra formulario
    ├── RefrescarCommand        → Recarga datos de la BD
    ├── IngresarStockCommand    → Abre panel de ajuste (+)
    ├── SacarStockCommand       → Abre panel de ajuste (−)
    ├── ConfirmarAjusteCommand  → Aplica ajuste de stock + registra movimiento
    ├── CancelarAjusteCommand   → Cierra panel ajuste
    ├── VerHistorialCommand     → Carga movimientos del producto
    └── CerrarHistorialCommand  → Cierra panel historial
```

**Detalles importantes:**
- Los campos numéricos son `string` en el formulario para evitar crash al escribir el punto decimal
- Al crear un producto con stock > 0, se registra automáticamente un movimiento de egreso (gasto)
- Al ajustar stock (−), valida que no se saque más de lo disponible
- El `CostoTotalInventario` se recalcula cada vez que cambia `ProductosFiltrados`

### CombosViewModel (`ViewModels/CombosViewModel.cs`) — ✅ **COMPLETO** (512 líneas)

```
CombosViewModel
│
├── SERVICIOS: IComboService, IProductoService
│
├── DATOS:
│   ├── Combos / CombosFiltrados  → Lista de combos
│   ├── ProductosDisponibles      → Productos del inventario (para agregar al combo)
│   ├── ProductosDelCombo         → Productos del combo en edición
│   ├── CostoTotalCombo           → Suma de costos calculada
│   └── Filtro / FiltroTipo       → Filtros por texto y tipo
│
├── DETALLE:
│   ├── MostrarDetalle            → Muestra panel de detalle
│   └── ComboDetalle              → Combo seleccionado para ver
│
├── FORMULARIO:
│   ├── FormNombre, FormDescripcion, FormTipo, FormPrecioVenta
│   ├── ProductoParaAgregar       → Producto seleccionado del dropdown
│   └── FormCantidadProducto      → Cantidad a agregar
│
└── COMANDOS:
    ├── CrearComboCommand, EditarComboCommand, DuplicarComboCommand
    ├── EliminarComboCommand, VerDetalleCommand, CerrarDetalleCommand
    ├── GuardarComboCommand, CancelarCommand
    ├── AgregarProductoCommand    → Agrega producto a la lista del combo
    ├── QuitarProductoCommand     → Quita producto del combo
    └── LimpiarFiltroTipoCommand, RefrescarCommand
```

### Otros ViewModels (esqueletos):
- **DashboardViewModel** — Solo hereda BaseViewModel, sin lógica
- **FichasCostoViewModel** — Solo hereda BaseViewModel
- **MovimientosViewModel** — Solo hereda BaseViewModel
- **ReportesViewModel** — Solo hereda BaseViewModel

### EntregasViewModel (`ViewModels/EntregasViewModel.cs`) — ✅ **COMPLETO** (515 líneas)

```
EntregasViewModel
│
├── SERVICIOS: IEntregaService, IComboService
│
├── CONTADORES DE URGENCIA:
│   ├── TotalPendientes           → Entregas no entregadas
│   ├── TotalUrgentes             → Entregas con ≤2 días restantes
│   └── TotalVencidas             → Entregas pasadas de plazo
│
├── DATOS:
│   ├── Entregas                  → Lista completa de la BD
│   ├── EntregasFiltradas         → Lista filtrada por estado
│   ├── CombosDisponibles         → Combos activos (para el form)
│   ├── ProductosEntrega          → Productos de la entrega seleccionada
│   ├── FiltroEstado              → ComboBox: Todas/Pendientes/Urgentes/Vencidas/Entregadas
│   └── EntregaSeleccionada       → Fila seleccionada en el DataGrid
│
├── FORMULARIO:
│   ├── FormComboSeleccionado     → Combo seleccionado (carga productos)
│   ├── FormNombreReceptor, FormDireccionReceptor
│   ├── FormTelefonoMovil, FormTelefonoFijo  → 2 teléfonos
│   ├── FormNombreRemitente, FormAgencia
│   ├── FormFechaOrden            → Auto: DateTime.Now
│   ├── FormPlazoDias             → Default: "5"
│   ├── FormNotas
│   ├── MostrarFormulario         → Visibilidad del panel
│   └── EsEdicion                 → Nuevo vs Editar
│
├── DETALLE:
│   ├── MostrarDetalle            → Visibilidad del panel de detalle
│   └── EntregaDetalle            → Entrega seleccionada para ver
│
└── COMANDOS:
    ├── NuevaEntregaCommand       → Abre formulario vacío
    ├── EditarEntregaCommand      → Carga datos en formulario
    ├── EliminarEntregaCommand    → Confirma y elimina
    ├── GuardarEntregaCommand     → Valida, guarda, recarga
    ├── CancelarCommand           → Cierra formulario
    ├── VerDetalleCommand         → Abre panel de detalle con productos
    ├── CerrarDetalleCommand      → Cierra panel de detalle
    ├── MarcarEntregadaCommand    → Pone FechaEntregada = hoy
    └── RefrescarCommand          → Recarga datos
```

**Detalles importantes:**
- Al crear una entrega, se copian los productos del combo seleccionado (`CrearDesdeComboAsync`)
- Los contadores de urgencia se actualizan al cargar datos
- El filtro de estado filtra: Pendientes (no entregadas), Urgentes (≤2 días), Vencidas (pasadas), Entregadas (completadas)
- La fecha de orden se asigna automáticamente al abrir el formulario

### ¿Qué tocar aquí?

- **Agregar funcionalidad a una pantalla existente:**
  1. Agregar propiedades (con `SetProperty`) y comandos (`RelayCommand`)
  2. Conectarlos en la Vista con `{Binding NombrePropiedad}` o `{Binding NombreCommand}`

- **Crear un nuevo ViewModel:**
  1. Crear clase que herede `BaseViewModel`
  2. Inyectar servicios necesarios por constructor
  3. Registrar en `App.xaml.cs` → `services.AddTransient<MiViewModel>()`
  4. Agregar `DataTemplate` en `App.xaml`
  5. Agregar comando de navegación en `MainViewModel`

---

## 8. Vistas

Las vistas son archivos XAML en `Views/`. Cada vista tiene un `.xaml` (diseño) y un `.xaml.cs` (code-behind, generalmente vacío).

### InventarioView.xaml — ✅ **COMPLETA** (~800 líneas)

Layout de 4 filas:
```
┌──────────────────────────────────────────────┐
│ HEADER: Título "📦 Inventario" + botón Nuevo │  Row 0
├──────────────────────────────────────────────┤
│ FILTROS: Buscador + CheckBox "Solo en stock" │  Row 1
├──────────────────────────────────────────────┤
│ CONTENIDO: DataGrid + paneles laterales      │  Row 2
│ ┌─────────────────────┬────────────────────┐ │
│ │ DataGrid            │ Panel form/stock/  │ │
│ │ (ProductosFiltrados)│ historial          │ │
│ └─────────────────────┴────────────────────┘ │
├──────────────────────────────────────────────┤
│ FOOTER: Estado + "VALOR DEL INVENTARIO $XX"  │  Row 3
└──────────────────────────────────────────────┘
```

**Columnas del DataGrid:**
| Columna | Ancho | Binding | Formato |
|---------|-------|---------|---------|
| Nombre | * (flexible) | `Nombre` | Con tooltip (descripción, stock, costo) |
| Unidad | 90px | `Unidad` | Enum |
| Costo (CUP) | 110px | `CostoCompra` | `$ {0:N2}` |
| Stock | 120px | `StockConUnidad` | Texto |
| Costo Total (CUP) | 130px | `CostoTotal` | `$ {0:N2}` |
| Ingreso | 100px | `FechaIngreso` | `dd/MM/yyyy` |
| Acciones | 200px | Botones: [+] [−] 📋 ✏️ 🗑️ | — |

**Paneles laterales** (se muestran/ocultan con `Visibility` + `BooleanToVisibilityConverter`):
- Panel formulario (crear/editar producto)
- Panel ajuste de stock (+/−)
- Panel historial de movimientos

### CombosView.xaml — ✅ **COMPLETA** (~700 líneas)

Layout con tarjetas (no DataGrid):
```
┌──────────────────────────────────────────────┐
│ HEADER: Título "🎁 Combos" + botón Crear    │
├──────────────────────────────────────────────┤
│ FILTROS: Buscador + botones tipo (C/A/F)     │
├──────────────────────────────────────────────┤
│ CONTENIDO: Grid de tarjetas + panel lateral  │
│ ┌─────────────────────┬────────────────────┐ │
│ │ [Combo 1] [Combo 2] │ Panel detalle /   │ │
│ │ [Combo 3] [Combo 4] │ formulario        │ │
│ └─────────────────────┴────────────────────┘ │
├──────────────────────────────────────────────┤
│ FOOTER: Contador de combos activos           │
└──────────────────────────────────────────────┘
```

**Tarjetas con colores por tipo:**
- 🟣 Purple (#7B1FA2) = Combo
- 🔵 Blue (#1976D2) = Agrego
- 🟠 Orange (#F57C00) = Festejo

**Cada tarjeta muestra:** Nombre, badge de tipo, cantidad de productos, costo total, precio venta, botones (👁 ver, ✏️ editar, 📋 duplicar, 🗑️ eliminar)

### MainWindow.xaml — Ventana principal

Layout de 2 columnas:
```
┌──────────┬───────────────────────────────────┐
│ SIDEBAR  │   ÁREA DE CONTENIDO              │
│ (240px)  │                                   │
│          │   ContentControl                  │
│ 💼       │   Content="{Binding               │
│ Dashboard│          CurrentViewModel}"        │
│ ─────    │                                   │
│ GESTIÓN  │   (Aquí se muestra la vista       │
│ Inventario│   que corresponda al ViewModel   │
│ Combos   │   actual)                         │
│ Fichas   │                                   │
│ Entregas ├───────────────────────────────────┤
│ ─────    │   Barra de estado                 │
│ FINANZAS │                                   │
│ Movimient│                                   │
│ Reportes │                                   │
└──────────┴───────────────────────────────────┘
```

### EntregasView.xaml — ✅ **COMPLETA** (~594 líneas)

Layout de 4 filas con urgencias visuales:
```
┌──────────────────────────────────────────────┐
│ HEADER: "📦 Entregas" + botón Nueva         │  Row 0
├──────────────────────────────────────────────┤
│ CONTADORES: [Pendientes] [Urgentes] [Vencidas] │ Row 1
│ (tarjetas con colores semáforo)              │
├──────────────────────────────────────────────┤
│ FILTRO: ComboBox de estado                   │  Row 2
├──────────────────────────────────────────────┤
│ CONTENIDO: DataGrid + panel lateral          │  Row 3
│ ┌─────────────────────┬────────────────────┐ │
│ │ DataGrid            │ Panel form /       │ │
│ │ (EntregasFiltradas) │ detalle            │ │
│ │ con barra de color  │                    │ │
│ │ por estado          │ Formulario:        │ │
│ │                     │ - Combo selector   │ │
│ │ 🔴 Vencida          │ - Fecha (auto)     │ │
│ │ 🟠 Urgente          │ - Receptor + dir   │ │
│ │ 🟢 En tiempo        │ - 2 teléfonos      │ │
│ │ ⚫ Entregada         │ - Remitente        │ │
│ │                     │ - Agencia          │ │
│ └─────────────────────┴────────────────────┘ │
└──────────────────────────────────────────────┘
```

**Columnas del DataGrid:**
| Columna | Binding | Formato |
|---------|---------|---------|
| Barra color | EntregaEstadoConverter | Rectángulo 4px ancho |
| # Orden | `NumeroOrden` | "ENT-00001" |
| Receptor | `NombreReceptor` | Texto |
| Estado | `EntregaEstadoTextoConverter` | "🟢 En tiempo (3 días)" |
| Fecha Orden | `FechaOrden` | dd/MM/yyyy |
| Acciones | Botones | 👁 ✏️ ✅ 🗑️ |

**Converters utilizados** (`Helpers/EntregaEstadoConverter.cs`):
- `EntregaEstadoConverter` → Color de fondo según estado (rojo/naranja/verde/gris)
- `EntregaEstadoTextoConverter` → Texto descriptivo con emoji y días restantes
- `InverseBoolToVisibilityConverter` → Muestra elementos cuando bool es false

### ¿Qué tocar aquí?

- **Cambiar diseño de una pantalla:** Editar el `.xaml` correspondiente
- **Agregar columna al DataGrid de inventario:** Agregar `<DataGridTextColumn>` en `InventarioView.xaml`
- **Cambiar colores/estilos:** Editar `App.xaml` (estilos globales) o el `.xaml` específico
- **Conectar un nuevo dato a la UI:** Usar `{Binding NombrePropiedad}` donde `NombrePropiedad` existe en el ViewModel

---

## 9. Navegación

### Archivos involucrados:
1. `Services/INavigationService.cs` — Interfaz
2. `Services/NavigationService.cs` — Implementación
3. `ViewModels/MainViewModel.cs` — Usa el servicio
4. `App.xaml` — DataTemplates que mapean ViewModel → View

### Cómo funciona:

```
 MainViewModel                NavigationService                WPF
     │                              │                            │
     │  NavigateTo<InventarioVM>()  │                            │
     ├─────────────────────────────>│                            │
     │                              │  1. Guarda VM anterior     │
     │                              │  2. Crea nuevo via DI      │
     │                              │  3. Llama OnNavigatedTo()  │
     │                              │  4. CurrentViewModel = nuevo│
     │                              │                            │
     │  CurrentViewModelChanged     │                            │
     │<─────────────────────────────│                            │
     │                              │                            │
     │  OnPropertyChanged(          │                            │
     │    "CurrentViewModel")       │                            │
     ├──────────────────────────────┼───────────────────────────>│
     │                              │                            │
     │                              │   WPF busca DataTemplate   │
     │                              │   para el tipo del VM      │
     │                              │   → muestra la View        │
```

### Para agregar una nueva pantalla de navegación:
1. Crear `MiViewModel.cs` en `ViewModels/`
2. Crear `MiView.xaml` en `Views/`
3. Agregar DataTemplate en `App.xaml`:
   ```xml
   <DataTemplate DataType="{x:Type vm:MiViewModel}">
       <views:MiView/>
   </DataTemplate>
   ```
4. Registrar ViewModel en `App.xaml.cs` → `services.AddTransient<MiViewModel>()`
5. Agregar comando en `MainViewModel`:
   ```csharp
   NavigateToMiCommand = new RelayCommand(_ => NavigateTo<MiViewModel>("Mi Sección"));
   ```
6. Agregar botón en `MainWindow.xaml` sidebar

---

## 10. Inyección de Dependencias

### Archivo: `App.xaml.cs` → método `ConfigureServices()`

La DI es como un "directorio" donde la app registra todos los servicios y ViewModels. Cuando alguien necesita un servicio, lo pide al contenedor y este lo crea automáticamente con todas sus dependencias.

### Registro actual:

```csharp
// BASE DE DATOS — Scoped: una instancia por "scope" (operación)
services.AddDbContext<AppDbContext>(options => ...SQLite...);

// NAVEGACIÓN — Singleton: UNA sola instancia en toda la app
services.AddSingleton<INavigationService, NavigationService>();

// SERVICIOS — Scoped: se crean cuando se necesitan, se destruyen al cerrar el scope
services.AddScoped<IConfiguracionService, ConfiguracionService>();
services.AddScoped<IProductoService, ProductoService>();
services.AddScoped<IComboService, ComboService>();
services.AddScoped<IEntregaService, EntregaService>();
services.AddScoped<IMovimientoService, MovimientoService>();
services.AddScoped<IFichaCostoService, FichaCostoService>();
services.AddScoped<IPeriodoInventarioService, PeriodoInventarioService>();
services.AddScoped<IClienteService, ClienteService>();
services.AddScoped<IAgenciaService, AgenciaService>();

// VIEWMODELS
services.AddSingleton<MainViewModel>();          // Singleton: solo hay uno
services.AddTransient<DashboardViewModel>();     // Transient: se crea uno nuevo cada vez
services.AddTransient<InventarioViewModel>();
services.AddTransient<CombosViewModel>();
services.AddTransient<EntregasViewModel>();
services.AddTransient<FichasCostoViewModel>();
services.AddTransient<MovimientosViewModel>();
services.AddTransient<ReportesViewModel>();
```

### Tipos de registro:
| Tipo | Significado | Uso |
|------|------------|-----|
| **Singleton** | Una sola instancia para toda la app | NavigationService, MainViewModel |
| **Scoped** | Una instancia por "scope" | Servicios de datos, DbContext |
| **Transient** | Instancia nueva cada vez que se pide | ViewModels de pantalla |

### ¿Qué tocar aquí?
- **Agregar servicio nuevo:** `services.AddScoped<IMiServicio, MiServicio>();`
- **Agregar ViewModel nuevo:** `services.AddTransient<MiViewModel>();`
- **Cambiar implementación:** `services.AddScoped<IProductoService, OtraImplementacion>();`

---

## 11. Guía: "Quiero cambiar X"

### Quiero agregar un campo a Producto (ej: "Proveedor")
1. **`Models/Producto.cs`** → Agregar: `public string? Proveedor { get; set; }`
2. **`ViewModels/InventarioViewModel.cs`** → Agregar campo de formulario `_formProveedor` y propiedad `FormProveedor`
3. **`ViewModels/InventarioViewModel.cs`** → En `GuardarProducto()`, asignar el valor al producto
4. **`ViewModels/InventarioViewModel.cs`** → En `EditarProducto()`, cargar el valor al formulario
5. **`Views/InventarioView.xaml`** → Agregar TextBox en el panel del formulario con `Text="{Binding FormProveedor}"`
6. (Opcional) Agregar columna al DataGrid
7. **Borrar `gestion.db`** para recrear la BD con el nuevo campo

### Quiero agregar una columna al DataGrid de inventario
1. **`Views/InventarioView.xaml`** → Buscar la sección de `<DataGrid.Columns>` y agregar:
   ```xml
   <DataGridTextColumn Header="Mi Columna" Binding="{Binding MiPropiedad}" Width="100"/>
   ```
   La propiedad debe existir en el modelo `Producto`

### Quiero cambiar los colores del sidebar
1. **`MainWindow.xaml`** → Buscar `Background="#1A237E"` (azul oscuro del sidebar)
2. Cambiar el valor hexadecimal del color

### Quiero cambiar estilos de botones globales
1. **`App.xaml`** → Buscar la sección `<Style x:Key="MenuButtonStyle">`
2. Modificar los `Setter` dentro del estilo

### Quiero agregar un tipo de combo nuevo (ej: "Especial")
1. **`Models/Combo.cs`** → Agregar `Especial` al enum `TipoCombo`
2. **`Views/CombosView.xaml`** → Buscar los botones de filtro por tipo y agregar uno nuevo
3. **`Views/CombosView.xaml`** → Agregar el color para el nuevo tipo en los DataTriggers
4. **Borrar `gestion.db`**

### Quiero agregar una categoría de movimiento nueva
1. **`Models/Movimiento.cs`** → Agregar al enum `CategoriaMovimiento`
2. No necesita más cambios a menos que se use en filtros de la UI

### Quiero implementar un módulo nuevo completo (ej: Movimientos)
1. **ViewModel:** Editar `ViewModels/MovimientosViewModel.cs`
   - Inyectar `IMovimientoService` y otros servicios necesarios
   - Crear propiedades para datos, filtros, formulario
   - Crear comandos para CRUD
   - Implementar `OnNavigatedTo()` para cargar datos
2. **Vista:** Editar `Views/MovimientosView.xaml`
   - Diseñar layout (DataGrid, filtros, formulario lateral)
   - Conectar todo vía Bindings
3. Ya está registrado en DI y tiene DataTemplate → funciona al hacer clic en el sidebar

### Quiero cambiar la ruta de la base de datos
1. **`App.xaml.cs`** → En `ConfigureServices()`, cambiar el `dbPath`:
   ```csharp
   var dbPath = System.IO.Path.Combine(
       Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
       "GestionApp",
       "gestion.db");
   ```

### Quiero agregar un nuevo paquete NuGet
1. Ejecutar en terminal: `dotnet add package NombrePaquete`
2. Se agrega automáticamente a `GestionApp.csproj`

---

## 12. Módulos y Estado Actual

| Módulo | ViewModel | Vista | Servicio | Estado |
|--------|-----------|-------|----------|--------|
| **Inventario** | InventarioViewModel (743 lín.) | InventarioView (801 lín.) | ProductoService | ✅ Completo |
| **Combos** | CombosViewModel (512 lín.) | CombosView (698 lín.) | ComboService | ✅ Completo |
| **Entregas** | EntregasViewModel (515 lín.) | EntregasView (594 lín.) | EntregaService | ✅ Completo |
| **Dashboard** | DashboardViewModel (esqueleto) | DashboardView (placeholder) | — | ⬜ Pendiente |
| **Fichas de Costo** | FichasCostoViewModel (esqueleto) | FichasCostoView (placeholder) | FichaCostoService | ⬜ Pendiente |
| **Movimientos** | MovimientosViewModel (esqueleto) | MovimientosView (placeholder) | MovimientoService | ⬜ Pendiente |
| **Reportes** | ReportesViewModel (esqueleto) | ReportesView (placeholder) | PeriodoInventarioService | ⬜ Pendiente |
| **Clientes** | — | — | ClienteService | ⬜ Sin empezar |
| **Agencias** | — | — | AgenciaService | ⬜ Sin empezar |
| **Configuración** | — | — | ConfiguracionService | ⬜ Sin empezar |

### Funcionalidades completadas en Inventario:
- ✅ CRUD completo de productos
- ✅ Búsqueda/filtrado por nombre
- ✅ Filtro "Solo en stock"
- ✅ Ajuste de stock (+/−) con registro automático de movimiento
- ✅ Historial de movimientos por producto
- ✅ Tooltip en nombre con descripción, stock y costo
- ✅ Footer con valor total del inventario en CUP
- ✅ Formato monetario: `$ X.XX` en columnas y footer

### Funcionalidades completadas en Combos:
- ✅ CRUD completo de combos
- ✅ Diseño con tarjetas (cards)
- ✅ Colores por tipo (Combo/Agrego/Festejo)
- ✅ Agregar/quitar productos del combo
- ✅ Panel de detalle con desglose de productos
- ✅ Duplicar combo
- ✅ Filtrado por nombre y tipo
- ✅ Campo Numero para identificar combos (índice único)
- ✅ Footer con contador de combos activos

### Funcionalidades completadas en Entregas:
- ✅ CRUD completo de entregas
- ✅ Creación desde combo (copia productos automáticamente)
- ✅ Número de orden automático (ENT-00001, ENT-00002...)
- ✅ Sistema de urgencia con plazo de 5 días
- ✅ Colores semáforo: 🔴 Vencida, 🟠 Urgente (≤2 días), 🟢 En tiempo, ⚫ Entregada
- ✅ Contadores de urgencia en la cabecera (pendientes, urgentes, vencidas)
- ✅ Filtro por estado (Todas/Pendientes/Urgentes/Vencidas/Entregadas)
- ✅ Dos campos de teléfono (móvil y fijo)
- ✅ Fecha de orden automática
- ✅ Marcar como entregada con un clic
- ✅ Panel de detalle con productos de la entrega

---

## 13. Cómo Compilar y Ejecutar

### Requisitos:
- .NET SDK 8.0 o superior
- Windows (WPF es exclusivo de Windows)

### Compilar:
```bash
cd "D:\Document\Proyecto de Papi"
dotnet build
```

### Ejecutar:
```bash
dotnet run
```

### Si la app está corriendo y no compila:
```bash
taskkill /F /IM GestionApp.exe
dotnet build
```

### Si cambiaste modelos y la BD no refleja los cambios:
1. Cerrar la app
2. Borrar: `%APPDATA%\GestionApp\gestion.db`
3. Volver a ejecutar (se recrea automáticamente)

---

## 14. Problemas Conocidos y Soluciones

| Problema | Causa | Solución |
|----------|-------|----------|
| Crash al escribir punto decimal | Los decimales en TextBox causan conflictos de parsing | Los campos numéricos del formulario son `string` y se parsean con `decimal.TryParse` al guardar |
| `dotnet build` falla con "file in use" | El .exe anterior sigue corriendo | `taskkill /F /IM GestionApp.exe` |
| Cambié un modelo pero la BD no tiene la columna | `EnsureCreated()` no altera tablas existentes | Borrar `%APPDATA%\GestionApp\gestion.db` y re-ejecutar |
| Los iconos (emojis) se ven cortados | RowHeight del DataGrid muy pequeña | Usar `RowHeight="46"` y `MinHeight="46"` |
| Al editar un combo, se duplican los productos | El update no borraba los viejos antes | `ComboService.ActualizarAsync` ahora borra los `ComboProducto` viejos primero |

---

## Diagrama de Arquitectura

```
┌─────────────────────────────────────────────────────────────────┐
│                        PRESENTACIÓN                             │
│                                                                 │
│  MainWindow.xaml ─── MainViewModel                              │
│       │                    │                                    │
│       │              NavigationService                          │
│       │                    │                                    │
│       ▼                    ▼                                    │
│  ┌─────────┐      ┌────────────────┐                            │
│  │  Views   │◄────►│  ViewModels    │                           │
│  │ (XAML)   │      │ (C# lógica)   │                           │
│  │          │      │               │                            │
│  │Inventario│◄────►│InventarioVM   │                           │
│  │Combos    │◄────►│CombosVM       │                           │
│  │Dashboard │◄────►│DashboardVM    │                           │
│  │etc...    │◄────►│etc...         │                           │
│  └─────────┘      └───────┬────────┘                           │
│                           │                                     │
│              {Binding} ◄──┘──► ICommand                         │
├─────────────────────────────────────────────────────────────────┤
│                     LÓGICA DE NEGOCIO                           │
│                                                                 │
│  ┌──────────────────┐    ┌──────────────────┐                  │
│  │   Interfaces     │    │   Servicios       │                 │
│  │                  │    │                   │                  │
│  │ IProductoService │───►│ ProductoService   │                 │
│  │ IComboService    │───►│ ComboService      │                 │
│  │ IMovimientoServ  │───►│ MovimientoService │                 │
│  │ etc...           │───►│ etc...            │                 │
│  └──────────────────┘    └────────┬──────────┘                 │
│                                   │                             │
├───────────────────────────────────┼─────────────────────────────┤
│                        DATOS     │                              │
│                                   ▼                             │
│                      ┌──────────────────┐                      │
│                      │  AppDbContext     │                      │
│                      │  (EF Core)       │                      │
│                      └────────┬─────────┘                      │
│                               │                                 │
│                               ▼                                 │
│                      ┌──────────────────┐                      │
│                      │   gestion.db     │                      │
│                      │   (SQLite)       │                      │
│                      └──────────────────┘                      │
│                                                                 │
│          📁 %APPDATA%/GestionApp/gestion.db                     │
└─────────────────────────────────────────────────────────────────┘
```

---

## Glosario Rápido

| Término | Significado |
|---------|-------------|
| **MVVM** | Model-View-ViewModel: patrón de diseño que separa datos, lógica y UI |
| **Binding** | Conexión automática entre una propiedad del ViewModel y un elemento de la UI |
| **ICommand** | Interfaz que permite al XAML ejecutar métodos del ViewModel (botones) |
| **DI** | Dependency Injection: el framework crea y pasa automáticamente las dependencias |
| **DbContext** | Clase de EF Core que representa la conexión a la base de datos |
| **DbSet** | Propiedad del DbContext que representa una tabla |
| **Soft-delete** | "Eliminar" poniendo `Activo = false` sin borrar el registro real |
| **ObservableCollection** | Lista que notifica a la UI cuando se agregan/quitan elementos |
| **SetProperty** | Método que cambia un valor y notifica a la UI |
| **RelayCommand** | Implementación de ICommand que conecta botones con métodos |
| **DataTemplate** | Regla XAML: "cuando el contenido sea X tipo, muestra Y vista" |
| **Scoped/Transient/Singleton** | Tiempo de vida de un servicio en DI |
| **CUP** | Peso cubano (moneda local para costos) |
| **USD** | Dólar estadounidense (moneda para precios de venta) |
