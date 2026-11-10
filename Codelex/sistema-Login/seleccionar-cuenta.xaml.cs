using Codelex.AccesoDatos;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class SeleccionarCuenta : Window
    {
        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        /// <param name="entrarAutomatico">Entrar automatico.</param>
        public SeleccionarCuenta(bool entrarAutomatico = false)
        {
            InitializeComponent();
            cargarCuentas();

            if (entrarAutomatico)
                ContentRendered += entrarAutomaticamente;
        }

        /// <summary>
        /// Entra solo cuando hay una única cuenta recordada.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
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

        /// <summary>
        /// Carga cuentas.
        /// </summary>
        private void cargarCuentas()
        {
            lista.ItemsSource = CuentasRecordadas.listar();
        }

        /// <summary>
        /// Muestra un mensaje con el error ocurrido.
        /// </summary>
        /// <param name="mensaje">Mensaje con el resultado.</param>
        private void mostrarError(string mensaje)
        {
            txtError.Text = mensaje;
            txtError.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Atiende el clic de cuenta.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private async void cuentaClic(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is not CuentaRecordada cuenta)
                return;

            await entrar(cuenta);
        }

        /// <summary>
        /// Inicia sesión con la cuenta recordada que se eligió.
        /// </summary>
        /// <param name="cuenta">Cuenta recordada elegida.</param>
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

        /// <summary>
        /// Atiende el clic de quitar.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
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

        /// <summary>
        /// Atiende el clic de agregar.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void agregarClic(object sender, RoutedEventArgs e)
        {
            new VentanaPrincipal().Show();
            Close();
        }

        /// <summary>
        /// Permite mover la ventana arrastrando la barra superior con el mouse.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void arrastrarVentana(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        /// <summary>
        /// Minimiza la ventana.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void minimizarClic(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        /// <summary>
        /// Cierra la ventana.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void cerrarClic(object sender, RoutedEventArgs e) => Close();
    }
}