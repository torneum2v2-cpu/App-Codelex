using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    /// <summary>
    /// Lógica de interacción para Menu.xaml
    /// </summary>
    public partial class Menu : Window
    {
        public Menu()
        {
            InitializeComponent();

            string fecha = DateTime.Now.ToString("dddd, d 'de' MMMM 'de' yyyy", new CultureInfo("es"));
            txtFecha.Text = char.ToUpper(fecha[0]) + fecha.Substring(1);
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        // Abre una ventana, oculta el menú y lo vuelve a mostrar al cerrarla
        private void AbrirModulo(Window ventana)
        {
            ventana.Closed += (_, _) => Show();
            Hide();
            ventana.Show();
        }

        private void Clientes_Click(object sender, RoutedEventArgs e)
        {
            AbrirModulo(new Clientes());
        }

        private void Inventario_Click(object sender, RoutedEventArgs e)
        {
            // TODO: AbrirModulo(new Inventario());
            MessageBox.Show("El módulo de Inventario estará disponible pronto.",
                            "Codelex", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Ordenes_Click(object sender, RoutedEventArgs e)
        {
            // TODO: AbrirModulo(new Ordenes());
            MessageBox.Show("El módulo de Órdenes estará disponible pronto.",
                            "Codelex", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Empleados_Click(object sender, RoutedEventArgs e)
        {
            // TODO: AbrirModulo(new Empleados());
            MessageBox.Show("El módulo de Empleados estará disponible pronto.",
                            "Codelex", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }
    }
}
