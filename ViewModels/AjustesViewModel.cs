using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GestionApp.Helpers;
using GestionApp.Models;
using GestionApp.Services;
using Microsoft.Win32;

namespace GestionApp.ViewModels
{
    /// <summary>
    /// ViewModel para la sección de Ajustes.
    /// Gestiona: datos del distribuidor, agencias de envío, preferencias generales.
    /// </summary>
    public class AjustesViewModel : BaseViewModel
    {
        private readonly IConfiguracionService _configuracionService;
        private readonly IAgenciaService _agenciaService;

        // ── Distribuidor ──
        private string _distNombre = string.Empty;
        private string _distPrefijoFicha = "FC-";
        private string _distPrefijoConformidad = "CONF-";
        private decimal _distTasaCambio = 300m;
        private bool _modoEdicion;
        private string _rutaReportes = string.Empty;

        // ── Agencias ──
        private ObservableCollection<Agencia> _agencias = new();
        private Agencia? _agenciaSeleccionada;
        private bool _mostrarFormAgencia;
        private bool _esEdicionAgencia;
        private string _formAgNombre = string.Empty;
        private string _formAgDireccion = string.Empty;
        private string _formAgTelefono = string.Empty;
        private string _formAgNotas = string.Empty;
        private string? _formAgLogoPath;
        private BitmapImage? _formAgLogoPreview;

        // ── Estado ──
        private string _mensajeEstado = string.Empty;

        #region Propiedades Distribuidor

        public string DistNombre
        {
            get => _distNombre;
            set => SetProperty(ref _distNombre, value);
        }

        public string DistPrefijoFicha
        {
            get => _distPrefijoFicha;
            set => SetProperty(ref _distPrefijoFicha, value);
        }

        public string DistPrefijoConformidad
        {
            get => _distPrefijoConformidad;
            set => SetProperty(ref _distPrefijoConformidad, value);
        }

        /// <summary>
        /// Tasa de cambio: 1 USD = X CUP.
        /// </summary>
        public decimal DistTasaCambio
        {
            get => _distTasaCambio;
            set => SetProperty(ref _distTasaCambio, value);
        }

        public bool ModoEdicion
        {
            get => _modoEdicion;
            set => SetProperty(ref _modoEdicion, value);
        }

        public string RutaReportes
        {
            get => _rutaReportes;
            set => SetProperty(ref _rutaReportes, value);
        }

        #endregion

        #region Propiedades Agencias

        public ObservableCollection<Agencia> Agencias
        {
            get => _agencias;
            set => SetProperty(ref _agencias, value);
        }

        public Agencia? AgenciaSeleccionada
        {
            get => _agenciaSeleccionada;
            set => SetProperty(ref _agenciaSeleccionada, value);
        }

        public bool MostrarFormAgencia
        {
            get => _mostrarFormAgencia;
            set => SetProperty(ref _mostrarFormAgencia, value);
        }

        public bool EsEdicionAgencia
        {
            get => _esEdicionAgencia;
            set => SetProperty(ref _esEdicionAgencia, value);
        }

        public string FormAgNombre
        {
            get => _formAgNombre;
            set => SetProperty(ref _formAgNombre, value);
        }

        public string FormAgDireccion
        {
            get => _formAgDireccion;
            set => SetProperty(ref _formAgDireccion, value);
        }

        public string FormAgTelefono
        {
            get => _formAgTelefono;
            set => SetProperty(ref _formAgTelefono, value);
        }

        public string FormAgNotas
        {
            get => _formAgNotas;
            set => SetProperty(ref _formAgNotas, value);
        }

        public string? FormAgLogoPath
        {
            get => _formAgLogoPath;
            set => SetProperty(ref _formAgLogoPath, value);
        }

        public BitmapImage? FormAgLogoPreview
        {
            get => _formAgLogoPreview;
            set => SetProperty(ref _formAgLogoPreview, value);
        }

        #endregion

        #region Propiedades Estado

        public string MensajeEstado
        {
            get => _mensajeEstado;
            set => SetProperty(ref _mensajeEstado, value);
        }

        #endregion

        #region Comandos

        public ICommand GuardarDistribuidorCommand { get; }
        public ICommand EditarDistribuidorCommand { get; }
        public ICommand CancelarEdicionCommand { get; }
        public ICommand GuardarTasaCommand { get; }
        public ICommand NuevaAgenciaCommand { get; }
        public ICommand EditarAgenciaCommand { get; }
        public ICommand EliminarAgenciaCommand { get; }
        public ICommand GuardarAgenciaCommand { get; }
        public ICommand CancelarAgenciaCommand { get; }
        public ICommand SeleccionarLogoCommand { get; }
        public ICommand ToggleActivoAgenciaCommand { get; }
        public ICommand CambiarRutaReportesCommand { get; }

        #endregion

        public AjustesViewModel(IConfiguracionService configuracionService, IAgenciaService agenciaService)
        {
            _configuracionService = configuracionService;
            _agenciaService = agenciaService;

            GuardarDistribuidorCommand = new RelayCommand(async _ => await GuardarDistribuidorAsync());
            EditarDistribuidorCommand = new RelayCommand(_ => ModoEdicion = true);
            CancelarEdicionCommand = new RelayCommand(async _ => { ModoEdicion = false; await CargarDatosAsync(); });
            GuardarTasaCommand = new RelayCommand(async _ => await GuardarTasaAsync());
            NuevaAgenciaCommand = new RelayCommand(_ => PrepararNuevaAgencia());
            EditarAgenciaCommand = new RelayCommand(param => PrepararEdicionAgencia(param as Agencia));
            EliminarAgenciaCommand = new RelayCommand(async param => await EliminarAgenciaAsync(param as Agencia));
            GuardarAgenciaCommand = new RelayCommand(async _ => await GuardarAgenciaAsync());
            CancelarAgenciaCommand = new RelayCommand(_ => CerrarFormAgencia());
            SeleccionarLogoCommand = new RelayCommand(_ => SeleccionarLogo());
            ToggleActivoAgenciaCommand = new RelayCommand(async param => await ToggleActivoAsync(param as Agencia));
            CambiarRutaReportesCommand = new RelayCommand(_ => CambiarRutaReportes());
        }

        public override void OnNavigatedTo(object? parameter = null)
        {
            base.OnNavigatedTo(parameter);
            _ = CargarDatosAsync();
        }

        // ═══════════════════════════════════════════════════
        // CARGAR DATOS
        // ═══════════════════════════════════════════════════

        private async Task CargarDatosAsync()
        {
            try
            {
                // Cargar configuración del distribuidor
                var config = await _configuracionService.ObtenerConfiguracionAsync();
                DistNombre = config.Nombre;
                DistPrefijoFicha = config.PrefijoFicha;
                DistPrefijoConformidad = config.PrefijoConformidad;
                DistTasaCambio = config.TasaCambioCUP;
                RutaReportes = config.RutaReportes
                    ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GestionApp", "Reportes");
                ModoEdicion = false;

                // Cargar agencias
                await CargarAgenciasAsync();

                MensajeEstado = "Configuración cargada";
            }
            catch (Exception ex)
            {
                MensajeEstado = $"Error al cargar: {ex.Message}";
            }
        }

        private async Task CargarAgenciasAsync()
        {
            var agencias = await _agenciaService.ObtenerTodosAsync();
            Agencias = new ObservableCollection<Agencia>(agencias);
        }

        // ═══════════════════════════════════════════════════
        // DISTRIBUIDOR
        // ═══════════════════════════════════════════════════

        private async Task GuardarDistribuidorAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(DistNombre))
                {
                    MensajeEstado = "❌ El nombre del distribuidor es obligatorio";
                    return;
                }

                var config = await _configuracionService.ObtenerConfiguracionAsync();
                config.Nombre = DistNombre.Trim();
                config.PrefijoFicha = string.IsNullOrWhiteSpace(DistPrefijoFicha) ? "FC-" : DistPrefijoFicha.Trim();
                config.PrefijoConformidad = string.IsNullOrWhiteSpace(DistPrefijoConformidad) ? "CONF-" : DistPrefijoConformidad.Trim();
                config.TasaCambioCUP = DistTasaCambio > 0 ? DistTasaCambio : 300m;

                await _configuracionService.ActualizarConfiguracionAsync(config);
                ModoEdicion = false;
                MensajeEstado = "✅ Datos guardados correctamente";
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error: {ex.Message}";
            }
        }

        private async Task GuardarTasaAsync()
        {
            try
            {
                var config = await _configuracionService.ObtenerConfiguracionAsync();
                config.TasaCambioCUP = DistTasaCambio > 0 ? DistTasaCambio : 300m;
                await _configuracionService.ActualizarConfiguracionAsync(config);
                MensajeEstado = "✅ Tasa de cambio actualizada";
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error: {ex.Message}";
            }
        }

        // ═══════════════════════════════════════════════════
        // RUTA DE REPORTES PDF
        // ═══════════════════════════════════════════════════

        private async void CambiarRutaReportes()
        {
            var dlg = new OpenFolderDialog
            {
                Title = "Seleccionar carpeta para guardar reportes PDF",
                Multiselect = false
            };

            if (!string.IsNullOrWhiteSpace(RutaReportes) && Directory.Exists(RutaReportes))
                dlg.InitialDirectory = RutaReportes;

            if (dlg.ShowDialog() != true) return;

            try
            {
                RutaReportes = dlg.FolderName;
                var config = await _configuracionService.ObtenerConfiguracionAsync();
                config.RutaReportes = RutaReportes;
                await _configuracionService.ActualizarConfiguracionAsync(config);
                MensajeEstado = "✅ Ruta de reportes actualizada";
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error: {ex.Message}";
            }
        }

        // ═══════════════════════════════════════════════════
        // AGENCIAS
        // ═══════════════════════════════════════════════════

        private void PrepararNuevaAgencia()
        {
            EsEdicionAgencia = false;
            AgenciaSeleccionada = null;
            FormAgNombre = string.Empty;
            FormAgDireccion = string.Empty;
            FormAgTelefono = string.Empty;
            FormAgNotas = string.Empty;
            FormAgLogoPath = null;
            FormAgLogoPreview = null;
            MostrarFormAgencia = true;
        }

        private void PrepararEdicionAgencia(Agencia? agencia)
        {
            if (agencia == null) return;
            EsEdicionAgencia = true;
            AgenciaSeleccionada = agencia;
            FormAgNombre = agencia.Nombre;
            FormAgDireccion = agencia.Direccion ?? string.Empty;
            FormAgTelefono = agencia.Telefono ?? string.Empty;
            FormAgNotas = agencia.Notas ?? string.Empty;
            FormAgLogoPath = agencia.LogoPath;
            
            // Cargar preview del logo
            CargarLogoPreview(agencia.Nombre);

            MostrarFormAgencia = true;
        }

        private void CargarLogoPreview(string nombre)
        {
            try
            {
                var nombreNorm = nombre.Trim().ToLowerInvariant()
                    .Replace("á", "a").Replace("é", "e").Replace("í", "i")
                    .Replace("ó", "o").Replace("ú", "u").Replace("ñ", "n")
                    .Replace(" ", "_");

                // Intentar recurso embebido
                try
                {
                    var uri = new Uri($"pack://application:,,,/Assets/Logos/{nombreNorm}.png", UriKind.Absolute);
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = uri;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.DecodePixelHeight = 64;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    FormAgLogoPreview = bitmap;
                    return;
                }
                catch { }

                // Intentar AppData
                var appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "GestionApp", "Logos", $"{nombreNorm}.png");
                if (File.Exists(appDataPath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(appDataPath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.DecodePixelHeight = 64;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    FormAgLogoPreview = bitmap;
                    return;
                }

                FormAgLogoPreview = null;
            }
            catch
            {
                FormAgLogoPreview = null;
            }
        }

        private void SeleccionarLogo()
        {
            var dlg = new OpenFileDialog
            {
                Title = "Seleccionar logo de la agencia",
                Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp|Todos|*.*",
                Multiselect = false
            };

            if (dlg.ShowDialog() != true) return;

            try
            {
                // Copiar a %AppData%/GestionApp/Logos/
                var logosDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "GestionApp", "Logos");
                Directory.CreateDirectory(logosDir);

                var nombreArchivo = string.IsNullOrWhiteSpace(FormAgNombre) 
                    ? Path.GetFileName(dlg.FileName) 
                    : FormAgNombre.Trim().ToLowerInvariant()
                        .Replace("á", "a").Replace("é", "e").Replace("í", "i")
                        .Replace("ó", "o").Replace("ú", "u").Replace("ñ", "n")
                        .Replace(" ", "_") + ".png";

                var destino = Path.Combine(logosDir, nombreArchivo);
                File.Copy(dlg.FileName, destino, overwrite: true);

                FormAgLogoPath = Path.GetFileNameWithoutExtension(nombreArchivo);

                // Invalidar caché del converter para que recargue la imagen
                AgenciaLogoConverter.InvalidarCache(FormAgNombre);

                // Cargar preview
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(destino, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.EndInit();
                bitmap.Freeze();
                FormAgLogoPreview = bitmap;

                MensajeEstado = "Logo seleccionado";
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error al seleccionar logo: {ex.Message}";
            }
        }

        private async Task GuardarAgenciaAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(FormAgNombre))
                {
                    MensajeEstado = "❌ El nombre de la agencia es obligatorio";
                    return;
                }

                if (EsEdicionAgencia && AgenciaSeleccionada != null)
                {
                    AgenciaSeleccionada.Nombre = FormAgNombre.Trim();
                    AgenciaSeleccionada.Direccion = string.IsNullOrWhiteSpace(FormAgDireccion) ? null : FormAgDireccion.Trim();
                    AgenciaSeleccionada.Telefono = string.IsNullOrWhiteSpace(FormAgTelefono) ? null : FormAgTelefono.Trim();
                    AgenciaSeleccionada.Notas = string.IsNullOrWhiteSpace(FormAgNotas) ? null : FormAgNotas.Trim();
                    if (FormAgLogoPath != null)
                        AgenciaSeleccionada.LogoPath = FormAgLogoPath;

                    await _agenciaService.ActualizarAsync(AgenciaSeleccionada);
                    MensajeEstado = $"✅ Agencia «{AgenciaSeleccionada.Nombre}» actualizada";
                }
                else
                {
                    var nueva = new Agencia
                    {
                        Nombre = FormAgNombre.Trim(),
                        Direccion = string.IsNullOrWhiteSpace(FormAgDireccion) ? null : FormAgDireccion.Trim(),
                        Telefono = string.IsNullOrWhiteSpace(FormAgTelefono) ? null : FormAgTelefono.Trim(),
                        Notas = string.IsNullOrWhiteSpace(FormAgNotas) ? null : FormAgNotas.Trim(),
                        LogoPath = FormAgLogoPath ?? AgenciaLogoConverter.NormalizarNombre(FormAgNombre),
                        Activo = true
                    };
                    await _agenciaService.CrearAsync(nueva);
                    MensajeEstado = $"✅ Agencia «{nueva.Nombre}» creada";
                }

                // Invalidar caché de logos para que las vistas se actualicen
                AgenciaLogoConverter.InvalidarCache();

                CerrarFormAgencia();
                await CargarAgenciasAsync();
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error: {ex.Message}";
            }
        }

        private async Task EliminarAgenciaAsync(Agencia? agencia)
        {
            if (agencia == null) return;

            var resultado = MessageBox.Show(
                $"¿Eliminar la agencia «{agencia.Nombre}»?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes) return;

            try
            {
                await _agenciaService.EliminarAsync(agencia.Id);
                MensajeEstado = $"🗑️ Agencia «{agencia.Nombre}» eliminada";
                await CargarAgenciasAsync();
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error: {ex.Message}";
            }
        }

        private async Task ToggleActivoAsync(Agencia? agencia)
        {
            if (agencia == null) return;

            try
            {
                agencia.Activo = !agencia.Activo;
                await _agenciaService.ActualizarAsync(agencia);
                MensajeEstado = agencia.Activo
                    ? $"✅ Agencia «{agencia.Nombre}» activada"
                    : $"⏸️ Agencia «{agencia.Nombre}» desactivada";
                await CargarAgenciasAsync();
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error: {ex.Message}";
            }
        }

        private void CerrarFormAgencia()
        {
            MostrarFormAgencia = false;
            AgenciaSeleccionada = null;
            FormAgNombre = string.Empty;
            FormAgDireccion = string.Empty;
            FormAgTelefono = string.Empty;
            FormAgNotas = string.Empty;
            FormAgLogoPath = null;
            FormAgLogoPreview = null;
        }
    }
}
