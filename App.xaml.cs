using System.Windows;
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

        // Asegurar que la base de datos está creada
        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();
        }

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
        services.AddScoped<ICategoriaService, CategoriaService>();
        services.AddScoped<IComboService, ComboService>();
        services.AddScoped<IMovimientoService, MovimientoService>();
        services.AddScoped<IFichaCostoService, FichaCostoService>();
        services.AddScoped<IPeriodoInventarioService, PeriodoInventarioService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IAgenciaService, AgenciaService>();

        // Registrar ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<InventarioViewModel>();
        services.AddTransient<CombosViewModel>();
        services.AddTransient<FichasCostoViewModel>();
        services.AddTransient<MovimientosViewModel>();
        services.AddTransient<ReportesViewModel>();

        return services.BuildServiceProvider();
    }
}

