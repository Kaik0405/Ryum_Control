# Ryum Control

**Ryum Control** es una aplicación de escritorio para administrar en un solo lugar las operaciones de inventario, preparación de combos, entregas, fichas de costo y control financiero. Centraliza la información del negocio y permite consultar su estado mediante un panel de indicadores y reportes.

> Aplicación de escritorio para Windows. El proyecto utiliza `GestionApp` como nombre técnico del ensamblado y del espacio de nombres.

## Funcionalidades

### Panel principal

- Resumen del balance, ingresos y egresos del período.
- Indicadores de entregas y remesas pendientes.
- Alertas de productos con poco o ningún stock.
- Entregas próximas, movimientos recientes y destinos registrados.

### Inventario

- Crear, editar y eliminar productos.
- Registrar unidad de medida, descripción, tipo de adquisición y costo.
- Controlar las existencias mediante entradas y salidas de stock.
- Consultar el historial de movimientos de cada producto.
- Visualizar el valor total del inventario.

### Combos

- Crear, editar, duplicar y eliminar combos de envío.
- Definir nombre, tipo, precio de venta y productos incluidos.
- Establecer cantidades mínimas y máximas de los productos.
- Vincular productos del combo con artículos del inventario o añadir productos sin vínculo.

### Entregas y remesas

- Registrar entregas y remesas.
- Asociar combos, agencias, remitentes y destinatarios.
- Gestionar fechas, direcciones, teléfonos y observaciones.
- Consultar, filtrar y actualizar el estado de las órdenes.
- Revisar el detalle de cada entrega y generar su modelo de conformidad para exportar o imprimir.

### Fichas de costo

- Consultar y filtrar fichas por período o por datos de la operación.
- Revisar productos, cantidades, costos, transporte, precio de venta y ganancias.
- Editar los costos asociados y consultar el detalle de cada ficha.
- Confirmar el descuento de los productos utilizados del inventario.
- Consultar indicadores de ventas, costos y ganancias.

### Finanzas y movimientos

- Registrar ingresos, egresos y gastos adicionales.
- Editar y eliminar movimientos.
- Filtrar los registros por período y categoría.
- Consultar el balance acumulado, ingresos, egresos e inversión en productos y transporte.
- Consultar el resumen de ganancias por ficha de costo.

### Reportes

- Generar reportes PDF de entregas.
- Generar reportes PDF de fichas de costo.
- Generar resúmenes mensuales con información financiera y operativa.
- Abrir el último reporte generado o la carpeta de reportes.

### Ajustes

- Configurar los datos del distribuidor y los prefijos de documentos.
- Definir la tasa de cambio USD/CUP utilizada en los cálculos.
- Administrar las agencias de envío, sus datos, estado y logotipos.
- Elegir la carpeta donde se guardan los reportes PDF.

## Tecnologías

- **C#** y **.NET 8**
- **WPF** para la interfaz de escritorio.
- **SQLite** como base de datos local.
- **Entity Framework Core** para el acceso y la persistencia de datos.
- **CommunityToolkit.Mvvm** para componentes del patrón MVVM.
- **Microsoft.Extensions.DependencyInjection** para la inyección de dependencias.
- **QuestPDF** para la generación de documentos PDF.

## Instalación

1. Abre la sección [Releases](../../releases) del repositorio.
2. Descarga el instalador de la versión que quieras utilizar (`RyumControl-Setup-...exe`).
3. Ejecuta el instalador y sigue los pasos indicados.
4. Abre **Ryum Control** desde el acceso directo creado o desde el menú Inicio.

Consulta las notas de cada versión en GitHub Releases para conocer sus cambios y requisitos específicos.

## Ejecutar desde el código fuente

### Requisitos

- Windows 10 o Windows 11 de 64 bits.
- .NET 8 SDK.
- Conexión a Internet para restaurar los paquetes NuGet la primera vez.
- Visual Studio, Visual Studio Code u otro editor compatible con C#.

### Compilar y ejecutar

Clona el repositorio y abre una terminal en la carpeta que contiene `GestionApp.csproj`:

```powershell
git clone <URL_DEL_REPOSITORIO>
cd <CARPETA_DEL_PROYECTO>
dotnet restore .\GestionApp.csproj
dotnet build .\GestionApp.csproj -c Release
dotnet run --project .\GestionApp.csproj
```

### Publicar para Windows x64

Para generar una publicación autocontenida, que incluya el runtime de .NET:

```powershell
dotnet publish .\GestionApp.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -o .\publish\win-x64
```

El resultado se genera en `publish\win-x64`. Para distribuir la aplicación manualmente, conserva y entrega todo el contenido de esa carpeta, no solamente `GestionApp.exe`. Cuando se distribuya mediante el instalador oficial, utiliza el instalador adjunto a la Release correspondiente.

## Almacenamiento y copias de seguridad

Ryum Control utiliza una base de datos SQLite local. Los datos de la aplicación se guardan en el perfil del usuario de Windows:

```text
%APPDATA%\GestionApp\
├── gestion.db
├── Logos\
└── crash.log
```

Los reportes PDF se guardan en la carpeta configurada desde **Ajustes**.

**Copia de seguridad:** cierra la aplicación y copia la carpeta completa `%APPDATA%\GestionApp\` a una ubicación segura. Para restaurar los datos, cierra Ryum Control y devuelve la carpeta respaldada a esa misma ubicación. No reemplaces la base de datos mientras la aplicación esté abierta.

La base de datos es local al usuario y al equipo donde se ejecuta la aplicación; no representa un almacenamiento compartido o sincronizado en la nube.

## Estructura general del proyecto

```text
GestionApp/
├── Assets/       # Recursos gráficos
├── Data/         # DbContext y gestión del esquema de SQLite
├── Helpers/      # Comandos y conversores auxiliares
├── Models/       # Entidades del dominio
├── Services/     # Lógica de negocio y acceso a datos
├── ViewModels/   # Estado y comandos de las vistas
├── Views/        # Interfaz WPF (XAML)
├── App.xaml      # Recursos y plantillas de navegación
├── App.xaml.cs   # Inicialización e inyección de dependencias
└── MainWindow.xaml
```

## Versiones

Las versiones publicadas, sus instaladores y sus notas de cambios se encuentran en [GitHub Releases](../../releases).
