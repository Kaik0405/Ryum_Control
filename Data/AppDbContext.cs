using System.IO;
using Microsoft.EntityFrameworkCore;
using GestionApp.Models;

namespace GestionApp.Data
{
    /// <summary>
    /// Contexto de base de datos de la aplicación.
    /// Utiliza SQLite como motor de base de datos.
    /// </summary>
    public class AppDbContext : DbContext
    {
        #region DbSets - Tablas principales

        // Productos e Inventario
        public DbSet<Producto> Productos { get; set; }
        public DbSet<ProductoVariante> ProductoVariantes { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<CompraProducto> ComprasProductos { get; set; }

        // Combos
        public DbSet<Combo> Combos { get; set; }
        public DbSet<ComboProducto> ComboProductos { get; set; }

        // Fichas de Costo
        public DbSet<FichaCosto> FichasCosto { get; set; }
        public DbSet<FichaCostoProducto> FichaCostoProductos { get; set; }

        // Conformidad
        public DbSet<ModeloConformidad> ModelosConformidad { get; set; }
        public DbSet<ConformidadProducto> ConformidadProductos { get; set; }

        // Movimientos Financieros
        public DbSet<Movimiento> Movimientos { get; set; }

        // Períodos de Inventario
        public DbSet<PeriodoInventario> PeriodosInventario { get; set; }
        public DbSet<InventarioSnapshot> InventarioSnapshots { get; set; }

        // Entidades auxiliares
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Agencia> Agencias { get; set; }
        public DbSet<ConfiguracionDistribuidor> Configuracion { get; set; }

        #endregion

        public string DbPath { get; }

        public AppDbContext()
        {
            // La base de datos se guardará en la carpeta del usuario
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            DbPath = Path.Combine(folder, "GestionApp", "gestion.db");
            
            // Crear directorio si no existe
            var directory = Path.GetDirectoryName(DbPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            DbPath = string.Empty;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite($"Data Source={DbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Producto y Variantes

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ProductoVariante>()
                .HasOne(v => v.Producto)
                .WithMany(p => p.Variantes)
                .HasForeignKey(v => v.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompraProducto>()
                .HasOne(c => c.Producto)
                .WithMany(p => p.Compras)
                .HasForeignKey(c => c.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region Combos

            modelBuilder.Entity<ComboProducto>()
                .HasOne(cp => cp.Combo)
                .WithMany(c => c.Productos)
                .HasForeignKey(cp => cp.ComboId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ComboProducto>()
                .HasOne(cp => cp.Producto)
                .WithMany()
                .HasForeignKey(cp => cp.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region Fichas de Costo

            modelBuilder.Entity<FichaCosto>()
                .HasOne(f => f.Combo)
                .WithMany()
                .HasForeignKey(f => f.ComboId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<FichaCosto>()
                .HasOne(f => f.Cliente)
                .WithMany()
                .HasForeignKey(f => f.ClienteId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<FichaCosto>()
                .HasOne(f => f.Agencia)
                .WithMany()
                .HasForeignKey(f => f.AgenciaId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<FichaCosto>()
                .HasOne(f => f.PeriodoInventario)
                .WithMany(p => p.FichasCosto)
                .HasForeignKey(f => f.PeriodoInventarioId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<FichaCostoProducto>()
                .HasOne(fp => fp.FichaCosto)
                .WithMany(f => f.Productos)
                .HasForeignKey(fp => fp.FichaCostoId)
                .OnDelete(DeleteBehavior.Cascade);

            #endregion

            #region Modelo de Conformidad

            modelBuilder.Entity<ModeloConformidad>()
                .HasOne(m => m.FichaCosto)
                .WithOne(f => f.ModeloConformidad)
                .HasForeignKey<ModeloConformidad>(m => m.FichaCostoId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ConformidadProducto>()
                .HasOne(cp => cp.ModeloConformidad)
                .WithMany(m => m.Productos)
                .HasForeignKey(cp => cp.ModeloConformidadId)
                .OnDelete(DeleteBehavior.Cascade);

            #endregion

            #region Movimientos

            modelBuilder.Entity<Movimiento>()
                .HasOne(m => m.FichaCosto)
                .WithMany()
                .HasForeignKey(m => m.FichaCostoId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Movimiento>()
                .HasOne(m => m.Producto)
                .WithMany()
                .HasForeignKey(m => m.ProductoId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Movimiento>()
                .HasOne(m => m.PeriodoInventario)
                .WithMany(p => p.Movimientos)
                .HasForeignKey(m => m.PeriodoInventarioId)
                .OnDelete(DeleteBehavior.SetNull);

            #endregion

            #region Período de Inventario

            modelBuilder.Entity<PeriodoInventario>()
                .HasIndex(p => new { p.Año, p.Mes })
                .IsUnique();

            modelBuilder.Entity<InventarioSnapshot>()
                .HasOne(s => s.PeriodoInventario)
                .WithMany(p => p.InventarioInicial)
                .HasForeignKey(s => s.PeriodoInventarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InventarioSnapshot>()
                .HasOne(s => s.Producto)
                .WithMany()
                .HasForeignKey(s => s.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CompraProducto>()
                .HasOne(c => c.PeriodoInventario)
                .WithMany(p => p.Compras)
                .HasForeignKey(c => c.PeriodoInventarioId)
                .OnDelete(DeleteBehavior.SetNull);

            #endregion

            #region Configuración

            modelBuilder.Entity<ConfiguracionDistribuidor>()
                .HasOne(c => c.PeriodoActual)
                .WithMany()
                .HasForeignKey(c => c.PeriodoActualId)
                .OnDelete(DeleteBehavior.SetNull);

            #endregion

            #region Índices adicionales

            modelBuilder.Entity<Producto>().HasIndex(p => p.Nombre);
            modelBuilder.Entity<Producto>().HasIndex(p => p.EnStock);
            modelBuilder.Entity<Combo>().HasIndex(c => c.Nombre);
            modelBuilder.Entity<Cliente>().HasIndex(c => c.NombreCompleto);
            modelBuilder.Entity<FichaCosto>().HasIndex(f => f.NumeroFicha);
            modelBuilder.Entity<FichaCosto>().HasIndex(f => f.FechaEnvio);
            modelBuilder.Entity<Movimiento>().HasIndex(m => m.Fecha);
            modelBuilder.Entity<Movimiento>().HasIndex(m => m.Tipo);

            #endregion
        }
    }
}
