using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using GestionApp.Helpers;
using GestionApp.Models;
using GestionApp.Services;

namespace GestionApp.ViewModels
{
    /// <summary>
    /// ViewModel para la gestión de combos.
    /// Los combos son predefinidos con productos de texto libre (no del inventario).
    /// El costo real se calcula después en la Ficha de Costo.
    /// 
    /// FLUJO:
    /// 1. Usuario ve la cuadrícula de combos existentes.
    /// 2. Crea un combo → se abre panel con formulario simple.
    /// 3. Escribe nombre, precio y agrega productos (texto + cantidad + unidad).
    /// 4. Guarda → combo se guarda en BD.
    /// </summary>
    public class CombosViewModel : BaseViewModel
    {
        private readonly IComboService _comboService;

        // Listas principales
        private ObservableCollection<Combo> _combos = new();
        private ObservableCollection<Combo> _combosFiltrados = new();
        private ObservableCollection<ComboProducto> _productosDelCombo = new();

        // Selección
        private Combo? _comboSeleccionado;

        // Filtros
        private string _filtro = string.Empty;
        private TipoCombo? _filtroTipo;

        // Formulario
        private bool _mostrarFormulario;
        private bool _esEdicion;
        private string _formNumero = string.Empty;
        private string _formNombre = string.Empty;
        private string _formDescripcion = string.Empty;
        private TipoCombo _formTipo = TipoCombo.Combo;
        private string _formPrecioVenta = string.Empty;

        // Campos para agregar producto al combo
        private string _formNombreProducto = string.Empty;
        private string _formCantidadProducto = string.Empty;
        private UnidadMedida _formUnidadProducto = UnidadMedida.Unidad;

        // Estado
        private string _mensajeEstado = string.Empty;

        #region Propiedades de Datos

        public ObservableCollection<Combo> Combos
        {
            get => _combos;
            set => SetProperty(ref _combos, value);
        }

        public ObservableCollection<Combo> CombosFiltrados
        {
            get => _combosFiltrados;
            set => SetProperty(ref _combosFiltrados, value);
        }

        public ObservableCollection<ComboProducto> ProductosDelCombo
        {
            get => _productosDelCombo;
            set => SetProperty(ref _productosDelCombo, value);
        }

        public Combo? ComboSeleccionado
        {
            get => _comboSeleccionado;
            set => SetProperty(ref _comboSeleccionado, value);
        }

        public string Filtro
        {
            get => _filtro;
            set => SetProperty(ref _filtro, value, FiltrarCombos);
        }

        public TipoCombo? FiltroTipo
        {
            get => _filtroTipo;
            set => SetProperty(ref _filtroTipo, value, FiltrarCombos);
        }

        public Array TiposCombo => Enum.GetValues(typeof(TipoCombo));
        public Array UnidadesMedida => Enum.GetValues(typeof(UnidadMedida));

        public string MensajeEstado
        {
            get => _mensajeEstado;
            set => SetProperty(ref _mensajeEstado, value);
        }

        #endregion

        #region Propiedades del Formulario

        public bool MostrarFormulario
        {
            get => _mostrarFormulario;
            set => SetProperty(ref _mostrarFormulario, value);
        }

        public bool EsEdicion
        {
            get => _esEdicion;
            set => SetProperty(ref _esEdicion, value);
        }

        public string TituloFormulario => EsEdicion ? "✏️ Editar Combo" : "➕ Nuevo Combo";

        public string FormNumero
        {
            get => _formNumero;
            set => SetProperty(ref _formNumero, value);
        }

        public string FormNombre
        {
            get => _formNombre;
            set => SetProperty(ref _formNombre, value);
        }

        public string FormDescripcion
        {
            get => _formDescripcion;
            set => SetProperty(ref _formDescripcion, value);
        }

        public TipoCombo FormTipo
        {
            get => _formTipo;
            set => SetProperty(ref _formTipo, value);
        }

        public string FormPrecioVenta
        {
            get => _formPrecioVenta;
            set => SetProperty(ref _formPrecioVenta, value);
        }

        public string FormNombreProducto
        {
            get => _formNombreProducto;
            set => SetProperty(ref _formNombreProducto, value);
        }

        public string FormCantidadProducto
        {
            get => _formCantidadProducto;
            set => SetProperty(ref _formCantidadProducto, value);
        }

        public UnidadMedida FormUnidadProducto
        {
            get => _formUnidadProducto;
            set => SetProperty(ref _formUnidadProducto, value);
        }

        #endregion

        #region Comandos

        public ICommand CrearComboCommand { get; }
        public ICommand EditarComboCommand { get; }
        public ICommand DuplicarComboCommand { get; }
        public ICommand EliminarComboCommand { get; }
        public ICommand GuardarComboCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand AgregarProductoCommand { get; }
        public ICommand QuitarProductoCommand { get; }
        public ICommand RefrescarCommand { get; }

        #endregion

        public CombosViewModel(IComboService comboService)
        {
            _comboService = comboService;

            CrearComboCommand = new RelayCommand(_ => PrepararNuevoCombo());
            EditarComboCommand = new RelayCommand(param => PrepararEdicion(param as Combo));
            DuplicarComboCommand = new RelayCommand(async param => await DuplicarComboAsync(param as Combo));
            EliminarComboCommand = new RelayCommand(async param => await EliminarComboAsync(param as Combo));
            GuardarComboCommand = new RelayCommand(
                async _ => await GuardarComboAsync(),
                _ => !string.IsNullOrWhiteSpace(FormNombre) &&
                     !string.IsNullOrWhiteSpace(FormPrecioVenta) &&
                     string.IsNullOrWhiteSpace(FormNombreProducto));
            CancelarCommand = new RelayCommand(_ => CerrarFormulario());
            AgregarProductoCommand = new RelayCommand(_ => AgregarProductoAlCombo(), _ => !string.IsNullOrWhiteSpace(FormNombreProducto));
            QuitarProductoCommand = new RelayCommand(param => QuitarProductoDelCombo(param as ComboProducto));
            RefrescarCommand = new RelayCommand(async _ => await CargarDatosAsync());
        }

        public override void OnNavigatedTo(object? parameter = null)
        {
            base.OnNavigatedTo(parameter);
            _ = CargarDatosAsync();
        }

        // ═══════════════════════════════════════════════════
        // MÉTODOS DE DATOS
        // ═══════════════════════════════════════════════════

        private async Task CargarDatosAsync()
        {
            try
            {
                var combos = await _comboService.ObtenerTodosAsync();
                Combos = new ObservableCollection<Combo>(combos);
                FiltrarCombos();
                MensajeEstado = $"{combos.Count} combo(s) cargados";
            }
            catch (Exception ex)
            {
                MensajeEstado = $"Error al cargar: {ex.Message}";
            }
        }

        private void FiltrarCombos()
        {
            var filtrados = Combos.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(Filtro))
            {
                filtrados = filtrados.Where(c =>
                    c.Nombre.Contains(Filtro, StringComparison.OrdinalIgnoreCase) ||
                    (c.Descripcion?.Contains(Filtro, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            if (FiltroTipo.HasValue)
            {
                filtrados = filtrados.Where(c => c.Tipo == FiltroTipo.Value);
            }

            CombosFiltrados = new ObservableCollection<Combo>(filtrados);
        }

        // ═══════════════════════════════════════════════════
        // MÉTODOS DEL FORMULARIO
        // ═══════════════════════════════════════════════════

        private void PrepararNuevoCombo()
        {
            EsEdicion = false;
            LimpiarFormulario();
            MostrarFormulario = true;
            OnPropertyChanged(nameof(TituloFormulario));
        }

        private void PrepararEdicion(Combo? combo)
        {
            if (combo == null) return;
            EsEdicion = true;
            ComboSeleccionado = combo;

            FormNumero = combo.Numero.ToString();
            FormNombre = combo.Nombre;
            FormDescripcion = combo.Descripcion ?? string.Empty;
            FormTipo = combo.Tipo;
            FormPrecioVenta = combo.PrecioVenta.ToString();

            // Cargar productos del combo (copia)
            ProductosDelCombo = new ObservableCollection<ComboProducto>(
                combo.Productos.Select(p => new ComboProducto
                {
                    NombreProducto = p.NombreProducto,
                    Cantidad = p.Cantidad,
                    Unidad = p.Unidad
                }));

            MostrarFormulario = true;
            OnPropertyChanged(nameof(TituloFormulario));
        }

        private void AgregarProductoAlCombo()
        {
            if (string.IsNullOrWhiteSpace(FormNombreProducto)) return;

            // Parsear cantidad
            if (!decimal.TryParse(FormCantidadProducto.Replace('.', ','), out var cantidad) &&
                !decimal.TryParse(FormCantidadProducto, out cantidad))
            {
                cantidad = 1;
            }
            if (cantidad <= 0) cantidad = 1;

            ProductosDelCombo.Add(new ComboProducto
            {
                NombreProducto = FormNombreProducto.Trim(),
                Cantidad = cantidad,
                Unidad = FormUnidadProducto
            });

            // Limpiar campos del producto
            FormNombreProducto = string.Empty;
            FormCantidadProducto = string.Empty;
            FormUnidadProducto = UnidadMedida.Unidad;
            MensajeEstado = "Producto agregado al combo";
        }

        private void QuitarProductoDelCombo(ComboProducto? item)
        {
            if (item == null) return;
            ProductosDelCombo.Remove(item);
        }

        private async Task GuardarComboAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(FormNombre))
                {
                    MensajeEstado = "❌ El nombre es obligatorio";
                    return;
                }

                if (!decimal.TryParse(FormPrecioVenta.Replace('.', ','), out var precioVenta) &&
                    !decimal.TryParse(FormPrecioVenta, out precioVenta))
                {
                    precioVenta = 0;
                }

                if (!int.TryParse(FormNumero, out var numero))
                {
                    MensajeEstado = "❌ El número del combo debe ser un entero";
                    return;
                }

                if (EsEdicion && ComboSeleccionado != null)
                {
                    ComboSeleccionado.Numero = numero;
                    ComboSeleccionado.Nombre = FormNombre.Trim();
                    ComboSeleccionado.Descripcion = string.IsNullOrWhiteSpace(FormDescripcion) ? null : FormDescripcion.Trim();
                    ComboSeleccionado.Tipo = FormTipo;
                    ComboSeleccionado.PrecioVenta = precioVenta;

                    // Reemplazar productos
                    ComboSeleccionado.Productos.Clear();
                    foreach (var p in ProductosDelCombo)
                    {
                        ComboSeleccionado.Productos.Add(new ComboProducto
                        {
                            ComboId = ComboSeleccionado.Id,
                            NombreProducto = p.NombreProducto,
                            Cantidad = p.Cantidad,
                            Unidad = p.Unidad
                        });
                    }

                    await _comboService.ActualizarAsync(ComboSeleccionado);
                    MensajeEstado = $"✅ Combo \"{FormNombre}\" actualizado";
                }
                else
                {
                    var nuevoCombo = new Combo
                    {
                        Numero = numero,
                        Nombre = FormNombre.Trim(),
                        Descripcion = string.IsNullOrWhiteSpace(FormDescripcion) ? null : FormDescripcion.Trim(),
                        Tipo = FormTipo,
                        PrecioVenta = precioVenta
                    };

                    foreach (var p in ProductosDelCombo)
                    {
                        nuevoCombo.Productos.Add(new ComboProducto
                        {
                            NombreProducto = p.NombreProducto,
                            Cantidad = p.Cantidad,
                            Unidad = p.Unidad
                        });
                    }

                    await _comboService.CrearAsync(nuevoCombo);
                    MensajeEstado = $"✅ Combo \"{FormNombre}\" creado con {ProductosDelCombo.Count} productos";
                }

                CerrarFormulario();
                await CargarDatosAsync();
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error al guardar: {ex.Message}";
            }
        }

        private async Task DuplicarComboAsync(Combo? combo)
        {
            if (combo == null) return;

            try
            {
                var nuevoNombre = $"{combo.Nombre} (copia)";
                await _comboService.DuplicarComboAsync(combo.Id, nuevoNombre);
                MensajeEstado = $"✅ Combo duplicado como \"{nuevoNombre}\"";
                await CargarDatosAsync();
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error al duplicar: {ex.Message}";
            }
        }

        private async Task EliminarComboAsync(Combo? combo)
        {
            if (combo == null) return;

            var resultado = MessageBox.Show(
                $"¿Eliminar el combo \"{combo.Nombre}\"?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes) return;

            try
            {
                var nombre = combo.Nombre;
                await _comboService.EliminarAsync(combo.Id);
                MensajeEstado = $"🗑️ Combo \"{nombre}\" eliminado";
                await CargarDatosAsync();
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error al eliminar: {ex.Message}";
            }
        }

        private void CerrarFormulario()
        {
            MostrarFormulario = false;
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            FormNumero = string.Empty;
            FormNombre = string.Empty;
            FormDescripcion = string.Empty;
            FormTipo = TipoCombo.Combo;
            FormPrecioVenta = string.Empty;
            FormNombreProducto = string.Empty;
            FormCantidadProducto = string.Empty;
            FormUnidadProducto = UnidadMedida.Unidad;
            ProductosDelCombo = new ObservableCollection<ComboProducto>();
            ComboSeleccionado = null;
        }
    }
}
