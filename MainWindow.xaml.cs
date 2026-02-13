using System.Windows;

namespace GestionApp;

/// <summary>
/// Ventana principal de la aplicación.
/// El DataContext se establece desde App.xaml.cs mediante DI.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}