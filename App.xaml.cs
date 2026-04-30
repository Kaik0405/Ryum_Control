using System.IO;
using System.Windows;
using System.Windows.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using GestionApp.Data;
using GestionApp.Services;
using GestionApp.ViewModels;

namespace GestionApp;

/// <summary>
/// Punto de entrada de la aplicación.
/// Configura la inyección de dependencias y el ciclo de vida.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Proveedor de servicios para inyección de dependencias.
    /// </summary>
    public static IServiceProvider Services { get; private set; } = null!;

    public App()
    {
        // Configurar servicios antes de iniciar la aplicación
        Services = ConfigureServices();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Capturar excepciones globales para diagnóstico
        DispatcherUnhandledException += (s, args) =>
        {
            System.IO.File.WriteAllText(
                System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "GestionApp", "crash.log"),
                $"[{DateTime.Now}]\n{args.Exception}\n\nInner: {args.Exception.InnerException}");
            MessageBox.Show(
                $"Error: {args.Exception.Message}\n\n{args.Exception.InnerException?.Message}",
                "Error no controlado", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        // Asegurar que la base de datos está creada con el esquema actual
        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            // Migrador seguro: crea la BD si no existe, agrega columnas/tablas faltantes sin borrar datos
            DatabaseMigrator.Migrar(context);

            // Seed: agencias por defecto (Rios y Yumury)
            if (!context.Agencias.Any())
            {
                context.Agencias.AddRange(
                    new GestionApp.Models.Agencia { Nombre = "Rios", LogoPath = "rios", Activo = true },
                    new GestionApp.Models.Agencia { Nombre = "Yumury", LogoPath = "yumury", Activo = true }
                );
                context.SaveChanges();
            }
        }

        // Copiar logos embebidos a AppData si no existen (primera ejecución)
        CopiarLogosEmbebidos();

        // Crear y mostrar la ventana principal
        var mainWindow = new MainWindow
        {
            DataContext = Services.GetRequiredService<MainViewModel>()
        };
        mainWindow.Show();
    }

    /// <summary>
    /// Configura todos los servicios y ViewModels para DI.
    /// </summary>
    /// <summary>
    /// Copia los logos embebidos (Assets/Logos/) a %AppData%/GestionApp/Logos/
    /// solo si no existen ya, para no sobreescribir logos personalizados del usuario.
    /// </summary>
    private static void CopiarLogosEmbebidos()
    {
        try
        {
            var logosDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GestionApp", "Logos");
            Directory.CreateDirectory(logosDir);

            // Lista de logos embebidos conocidos
            var logosEmbebidos = new[] { "rios", "yumury" };

            foreach (var logo in logosEmbebidos)
            {
                var destino = Path.Combine(logosDir, $"{logo}.png");
                if (File.Exists(destino)) continue; // No sobreescribir logos personalizados

                try
                {
                    var uri = new Uri($"pack://application:,,,/Assets/Logos/{logo}.png", UriKind.Absolute);
                    var streamInfo = Application.GetResourceStream(uri);
                    if (streamInfo != null)
                    {
                        using var fileStream = File.Create(destino);
                        streamInfo.Stream.CopyTo(fileStream);
                    }
                }
                catch { }
            }
        }
        catch { }
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Registrar DbContext con SQLite
        services.AddDbContext<AppDbContext>(options =>
        {
            var dbPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GestionApp",
                "gestion.db");
            
            // Crear directorio si no existe
            var dir = System.IO.Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            options.UseSqlite($"Data Source={dbPath}");
        });

        // Registrar servicios de navegación
        services.AddSingleton<INavigationService, NavigationService>();

        // Registrar servicios de negocio
        services.AddScoped<IConfiguracionService, ConfiguracionService>();
        services.AddScoped<IProductoService, ProductoService>();
        services.AddScoped<IComboService, ComboService>();
        services.AddScoped<IMovimientoService, MovimientoService>();
        services.AddScoped<IFichaCostoService, FichaCostoService>();
        services.AddScoped<IPeriodoInventarioService, PeriodoInventarioService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IAgenciaService, AgenciaService>();
        services.AddScoped<IEntregaService, EntregaService>();
        services.AddSingleton<ReportesPdfService>();

        // Registrar ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<InventarioViewModel>();
        services.AddSingleton<CombosViewModel>();
        services.AddSingleton<EntregasViewModel>();
        services.AddSingleton<FichasCostoViewModel>();
        services.AddSingleton<MovimientosViewModel>();
        services.AddSingleton<ReportesViewModel>();
        services.AddSingleton<AjustesViewModel>();

        return services.BuildServiceProvider();
    }
}

