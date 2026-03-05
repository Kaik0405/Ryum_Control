using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GestionApp.Helpers
{
    /// <summary>
    /// Convierte el nombre de una agencia a su imagen de logo.
    /// Prioridad: 1) %AppData%/GestionApp/Logos/ 2) recurso embebido Assets/Logos/
    /// Los logos del usuario en AppData siempre tienen prioridad sobre los embebidos.
    /// </summary>
    public class AgenciaLogoConverter : IValueConverter
    {
        private static readonly Dictionary<string, ImageSource?> _cache = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Invalida la caché para un nombre específico o toda la caché.
        /// Llamar después de cambiar/guardar un logo.
        /// </summary>
        public static void InvalidarCache(string? nombre = null)
        {
            if (nombre != null)
            {
                var key = NormalizarNombre(nombre);
                _cache.Remove(key);
            }
            else
            {
                _cache.Clear();
            }
        }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var nombre = value as string;
            if (string.IsNullOrWhiteSpace(nombre)) return null;

            var nombreNorm = NormalizarNombre(nombre);

            if (_cache.TryGetValue(nombreNorm, out var cached))
                return cached;

            ImageSource? imagen = null;

            // 1. PRIORIDAD: Buscar en carpeta de usuario (AppData)
            try
            {
                var logosDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "GestionApp", "Logos");

                // Buscar con varias extensiones
                foreach (var ext in new[] { ".png", ".jpg", ".jpeg", ".bmp" })
                {
                    var filePath = Path.Combine(logosDir, $"{nombreNorm}{ext}");
                    if (File.Exists(filePath))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        imagen = bitmap;
                        break;
                    }
                }
            }
            catch { }

            // 2. FALLBACK: recurso embebido
            if (imagen == null)
            {
                try
                {
                    var uri = new Uri($"pack://application:,,,/Assets/Logos/{nombreNorm}.png", UriKind.Absolute);
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = uri;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    imagen = bitmap;
                }
                catch { }
            }

            // Solo cachear si encontramos imagen (no cachear null para reintentar)
            if (imagen != null)
                _cache[nombreNorm] = imagen;

            return imagen;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();

        /// <summary>
        /// Normaliza el nombre: minúsculas, sin espacios, sin acentos.
        /// "Ríos" → "rios", "Yumury" → "yumury"
        /// </summary>
        public static string NormalizarNombre(string nombre)
        {
            return nombre.Trim().ToLowerInvariant()
                .Replace("á", "a").Replace("é", "e").Replace("í", "i")
                .Replace("ó", "o").Replace("ú", "u").Replace("ñ", "n")
                .Replace(" ", "_");
        }
    }
}
