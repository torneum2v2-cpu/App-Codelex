using Codelex.AccesoDatos;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class IngresarCorreo : Window
    {
        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        public IngresarCorreo()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Muestra el error al pedir el código de verificación.
        /// </summary>
        /// <param name="message">Mensaje a mostrar.</param>
        private void mostrarErrorCodigo(string message)
        {
            txtError1.Text = message;
            txtError1.Visibility = Visibility.Visible;
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

        /// <summary>
        /// Atiende el clic de volver inicio.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void volverInicioClic(Object sender, RoutedEventArgs e)
        {
            VentanaPrincipal m = new VentanaPrincipal();
            m.Show();
            Close();
        }
        private string correoConsulta = "";

        /// <summary>
        /// Indica si existe correo en segundo plano.
        /// </summary>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        private bool existeCorreoEnSegundoPlano()
        {
            Empleado emp = new Empleado();
            return emp.existeCorreo(correoConsulta);
        }

        /// <summary>
        /// Atiende el clic de siguiente.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private async void siguienteClic(Object sender, RoutedEventArgs e)
        {
            txtError1.Visibility = Visibility.Collapsed;

            string correo = txtEmail.Text.Trim();

            if (correo == "")
            {
                mostrarErrorCodigo("Ingrese un correo.");
                return;
            }

            correoConsulta = correo;
            IsEnabled = false;
            Cargando cargando = Cargando.iniciar(this, "Verificando correo...");

            bool existe = false;
            string errorBD = "";

            try
            {
                existe = await Task.Run(existeCorreoEnSegundoPlano);
            }
            catch (Exception ex)
            {
                errorBD = ex.Message;
            }

            cargando.terminar();
            IsEnabled = true;

            if (errorBD != "")
            {
                mostrarErrorCodigo(errorBD);
                return;
            }

            if (!existe)
            {
                mostrarErrorCodigo("El correo ingresado no tiene una cuenta registrada");
                txtEmail.Focus();
                return;
            }

            new IngresarCodigo(correo).Show();
            Close();
        }
    }
}
