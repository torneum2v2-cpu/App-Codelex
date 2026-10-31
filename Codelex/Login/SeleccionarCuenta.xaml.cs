using Codelex.BDD;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class SeleccionarCuenta : Window
    {
        public SeleccionarCuenta(bool entrarAutomatico = false)
        {
            InitializeComponent();
            CargarCuentas();

            if (entrarAutomatico)
                ContentRendered += EntrarAutomatico;
        }

        private async void EntrarAutomatico(object? sender, EventArgs e)
        {
            ContentRendered -= EntrarAutomatico;

            List<CuentaRecordada> cuentas = CuentasRecordadas.Listar();
            if (cuentas.Count == 1)
                await Entrar(cuentas[0]);
        }

        private void CargarCuentas()
        {
            lista.ItemsSource = CuentasRecordadas.Listar();
        }

        private void MostrarError(string mensaje)
        {
            txtError.Text = mensaje;
            txtError.Visibility = Visibility.Visible;
        }

        private async void Cuenta_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is not CuentaRecordada cuenta)
                return;

            await Entrar(cuenta);
        }

        private async Task Entrar(CuentaRecordada cuenta)
        {
            txtError.Visibility = Visibility.Collapsed;
            IsEnabled = false;
            Cargando cargando = Cargando.Iniciar(this, "Entrando...");

            Usuario usuario = new Usuario();
            string clave = CuentasRecordadas.ObtenerContrasena(cuenta);
            bool valido = false;
            string errorBD = "";

            try
            {
                if (clave != "")
                    valido = await Task.Run(() => usuario.ValidarLogin(cuenta.Usuario, clave));
            }
            catch (Exception ex)
            {
                errorBD = ex.Message;
            }

            cargando.Terminar();
            IsEnabled = true;

            if (errorBD != "")
            {
                MostrarError(errorBD);
                return;
            }

            if (!valido)
            {
                CuentasRecordadas.Quitar(cuenta.Usuario);
                CargarCuentas();
                MostrarError("La contraseña de esta cuenta cambió o ya no es válida. Agrégala de nuevo para continuar.");
                return;
            }

            new Menu().Show();
            Close();
        }

        private void Quitar_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is not CuentaRecordada cuenta)
                return;

            CuentasRecordadas.Quitar(cuenta.Usuario);

            if (CuentasRecordadas.Listar().Count == 0)
            {
                new MainWindow().Show();
                Close();
                return;
            }

            CargarCuentas();
        }

        private void Agregar_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}