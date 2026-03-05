using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GestionApp.Data
{
    /// <summary>
    /// Migrador automático de base de datos para SQLite.
    /// Detecta columnas y tablas faltantes comparando el modelo de EF Core
    /// con el esquema real de la BD. No requiere mantenimiento manual
    /// cuando se agregan nuevas propiedades o entidades a los modelos.
    /// </summary>
    public static class DatabaseMigrator
    {
        /// <summary>
        /// Asegura que la base de datos exista y tenga el esquema actualizado.
        /// - Si la BD no existe: la crea completa con EnsureCreated.
        /// - Si la BD existe: detecta automáticamente tablas y columnas
        ///   faltantes y las agrega sin perder datos.
        /// </summary>
        public static void Migrar(AppDbContext context)
        {
            // BD nueva → crear esquema completo de golpe
            if (context.Database.EnsureCreated())
                return;

            // BD existente → detectar y aplicar cambios incrementales
            var conn = context.Database.GetDbConnection();
            conn.Open();

            try
            {
                using var cmd = conn.CreateCommand();

                // DDL completo que EF Core espera (para crear tablas nuevas)
                var scriptCompleto = context.Database.GenerateCreateScript();

                // Tablas que ya existen en SQLite
                var tablasExistentes = ObtenerTablas(cmd);

                foreach (var entidad in context.Model.GetEntityTypes())
                {
                    var tabla = entidad.GetTableName();
                    if (tabla == null) continue;

                    if (!tablasExistentes.Contains(tabla))
                    {
                        // Tabla nueva → extraer CREATE TABLE del script de EF Core
                        var sql = ExtraerCreateTable(scriptCompleto, tabla);
                        if (sql != null)
                        {
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // Tabla existente → detectar columnas faltantes
                        var columnasActuales = ObtenerColumnas(cmd, tabla);

                        foreach (var prop in entidad.GetProperties())
                        {
                            var columna = prop.GetColumnName();
                            if (columna == null || columnasActuales.Contains(columna, StringComparer.OrdinalIgnoreCase))
                                continue;

                            var definicion = ConstruirDefinicionColumna(prop);
                            cmd.CommandText = $"ALTER TABLE \"{tabla}\" ADD COLUMN \"{columna}\" {definicion}";
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // Crear índices faltantes
                AplicarIndices(cmd, scriptCompleto);
            }
            finally
            {
                conn.Close();
            }
        }

        private static HashSet<string> ObtenerTablas(System.Data.Common.DbCommand cmd)
        {
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
            var tablas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                tablas.Add(reader.GetString(0));
            return tablas;
        }

        private static HashSet<string> ObtenerColumnas(System.Data.Common.DbCommand cmd, string tabla)
        {
            cmd.CommandText = $"PRAGMA table_info(\"{tabla}\")";
            var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                cols.Add(reader.GetString(1));
            return cols;
        }

        private static string? ExtraerCreateTable(string script, string tabla)
        {
            var pattern = $@"CREATE TABLE ""{Regex.Escape(tabla)}""\s*\([^;]+\);";
            var match = Regex.Match(script, pattern, RegexOptions.Singleline);
            return match.Success ? match.Value : null;
        }

        /// <summary>
        /// Construye la definición de columna para ALTER TABLE ADD COLUMN
        /// usando los metadatos del modelo de EF Core.
        /// </summary>
        private static string ConstruirDefinicionColumna(IReadOnlyProperty property)
        {
            var tipo = property.GetColumnType() ?? MapearTipo(property.ClrType);
            var sb = new StringBuilder(tipo);

            if (!property.IsNullable)
                sb.Append(" NOT NULL");

            // Valor por defecto configurado en el modelo
            var defaultSql = property.GetDefaultValueSql();
            if (defaultSql != null)
            {
                sb.Append($" DEFAULT ({defaultSql})");
            }
            else
            {
                var defVal = property.GetDefaultValue();
                if (defVal != null)
                {
                    sb.Append($" DEFAULT {Formatear(defVal)}");
                }
                else if (!property.IsNullable)
                {
                    // SQLite exige DEFAULT para columnas NOT NULL en ALTER TABLE ADD COLUMN
                    sb.Append($" DEFAULT {DefaultParaTipo(tipo, property.ClrType)}");
                }
            }

            return sb.ToString();
        }

        private static string MapearTipo(Type clr)
        {
            var t = Nullable.GetUnderlyingType(clr) ?? clr;
            if (t == typeof(int) || t == typeof(long) || t == typeof(bool) || t.IsEnum)
                return "INTEGER";
            if (t == typeof(double) || t == typeof(float))
                return "REAL";
            return "TEXT";
        }

        private static string Formatear(object valor) => valor switch
        {
            bool b => b ? "1" : "0",
            int i => i.ToString(),
            long l => l.ToString(),
            double d => d.ToString(System.Globalization.CultureInfo.InvariantCulture),
            string s => $"'{s.Replace("'", "''")}'",
            _ => $"'{valor}'"
        };

        private static string DefaultParaTipo(string sqlType, Type clrType)
        {
            var t = Nullable.GetUnderlyingType(clrType) ?? clrType;
            if (t == typeof(DateTime)) return "'0001-01-01 00:00:00'";
            if (t == typeof(decimal)) return "'0'";

            return sqlType.ToUpperInvariant() switch
            {
                "INTEGER" => "0",
                "REAL" => "0.0",
                _ => "''"
            };
        }

        private static void AplicarIndices(System.Data.Common.DbCommand cmd, string script)
        {
            foreach (Match m in Regex.Matches(script,
                @"CREATE\s+(UNIQUE\s+)?INDEX\s+""[^""]+""[^;]+;", RegexOptions.Singleline))
            {
                var sql = Regex.Replace(m.Value, @"CREATE(\s+UNIQUE)?\s+INDEX",
                    match => match.Groups[1].Success
                        ? "CREATE UNIQUE INDEX IF NOT EXISTS"
                        : "CREATE INDEX IF NOT EXISTS");
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }
    }
}
