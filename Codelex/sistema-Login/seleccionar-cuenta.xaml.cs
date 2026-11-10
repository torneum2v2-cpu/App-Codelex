using Codelex.AccesoDatos;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class SeleccionarCuenta : Window
    {
        public SeleccionarCuenta(bool entrarAutomatico = false)
        {
            InitializeComponent();
            cargarCuentas();

            if (entrarAutomatico)
                ContentRendered += entrarAutomaticamente;
        }

        private async void entrarAutomaticamente(object? sender, EventArgs e)
        {
            ContentRendered -= entrarAutomaticamente;

            try
            {
                List<CuentaRecordada> cuentas = CuentasRecordadas.listar();
                if (cuentas.Count == 1)
                    await entrar(cuentas[0]);
            }
            catch (Exception ex)
            {
                IsEnabled = true;
                mostrarError("No se pudo entrar: " + ex.Message);
            }
        }

        private void cargarCuentas()
        {
            lista.ItemsSource = CuentasRecordadas.listar();
        }

        private void mostrarError(string mensaje)
        {
            txtError.Text = mensaje;
            txtError.Visibility = Visibility.Visible;
        }

        private async void cuentaClic(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is not CuentaRecordada cuenta)
                return;

            await entrar(cuenta);
        }

        private async Task entrar(CuentaRecordada cuenta)
        {
            txtError.Visibility = Visibility.Collapsed;
            IsEnabled = false;
            Cargando? cargando = null;
            try { cargando = Cargando.iniciar(this, "Entrando..."); } catch { }

            Usuario usuario = new Usuario();
            string clave = CuentasRecordadas.obtenerContrasena(cuenta);
            bool valido = false;
            string errorBD = "";

            try
            {
                if (clave != "")
                    valido = await Task.Run(() => usuario.validarLogin(cuenta.Usuario, clave));
            }
            catch (Exception ex)
            {
                errorBD = ex.Message;
            }

            try { cargando?.terminar(); } catch { }
            IsEnabled = true;

            if (errorBD != "")
            {
                mostrarError(errorBD);
                return;
            }

            if (!valido)
            {
                CuentasRecordadas.quitar(cuenta.Usuario);
                cargarCuentas();
                mostrarError("La contraseña de esta cuenta cambió o ya no es válida. Agrégala de nuevo para continuar.");
                return;
            }

            Sesion.iniciar(usuario);
            new Menu().Show();
            Close();
        }

        private void quitarClic(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is not CuentaRecordada cuenta)
                return;

            CuentasRecordadas.quitar(cuenta.Usuario);

            if (CuentasRecordadas.listar().Count == 0)
            {
                new VentanaPrincipal().Show();
                Close();
                return;
            }

            cargarCuentas();
        }

        private void agregarClic(object sender, RoutedEventArgs e)
        {
            new VentanaPrincipal().Show();
            Close();
        }

        private void arrastrarVentana(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void minimizarClic(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        private void cerrarClic(object sender, RoutedEventArgs e) => Close();
    }
}