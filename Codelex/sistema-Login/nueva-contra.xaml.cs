using Codelex.AccesoDatos;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Codelex
{
    public partial class NuevaContra : Window
    {
        private string correo = "";

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        /// <param name="email">Email.</param>
        public NuevaContra(string email)
        {
            InitializeComponent();
            correo = email;
        }

        /// <summary>
        /// Muestra un mensaje con el error ocurrido.
        /// </summary>
        /// <param name="message">Mensaje a mostrar.</param>
        private void mostrarError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
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
        private void minimizarClic(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Cierra la ventana.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void cerrarClic(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Obtiene contra 1.
        /// </summary>
        /// <returns>Texto resultante.</returns>
        private string obtenerContra1()
        {
            if (btnShowPassword.IsChecked == true)
                return txtPasswordVisible.Text;
            return txtPassword.Password;
        }

        /// <summary>
        /// Obtiene contra 2.
        /// </summary>
        /// <returns>Texto resultante.</returns>
        private string obtenerContra2()
        {
            if (btnShowPassword2.IsChecked == true)
                return txtPasswordVisible2.Text;
            return txtPassword2.Password;
        }

        /// <summary>
        /// Muestra ocultar.
        /// </summary>
        /// <param name="boton">Boton.</param>
        /// <param name="oculta">Oculta.</param>
        /// <param name="visible">Visible.</param>
        private void mostrarOcultar(ToggleButton boton, PasswordBox oculta, TextBox visible)
        {
            if (boton.IsChecked == true)
            {
                visible.Text = oculta.Password;
                oculta.Visibility = Visibility.Collapsed;
                visible.Visibility = Visibility.Visible;
                visible.Focus();
                visible.CaretIndex = visible.Text.Length;
            }
            else
            {
                oculta.Password = visible.Text;
                visible.Visibility = Visibility.Collapsed;
                oculta.Visibility = Visibility.Visible;
                oculta.Focus();
            }
        }

        /// <summary>
        /// Atiende el cambio de mostrar contraseña.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void mostrarContrasenaCambiado(object sender, RoutedEventArgs e)
        {
            if (sender == btnShowPassword)
                mostrarOcultar(btnShowPassword, txtPassword, txtPasswordVisible);
            else
                mostrarOcultar(btnShowPassword2, txtPassword2, txtPasswordVisible2);
        }

        /// <summary>
        /// Atiende el clic de restablecer.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void restablecerClic(Object sender, RoutedEventArgs e)
        {
            txtError.Visibility = Visibility.Collapsed;

            Usuario user = new Usuario();
            string mensaje = "";

            try
            {
                bool cambiada = user.cambiarContrasena(correo, obtenerContra1(), obtenerContra2(), out mensaje);

                if (!cambiada)
                {
                    mostrarError(mensaje);
                    return;
                }
            }
            catch (Exception ex)
            {
                mostrarError("Error al conectar con la base de datos: " + ex.Message);
                return;
            }

            MensajeCodelex.mostrar(this, "Contraseña actualizada",
                    "Tu contraseña se cambió correctamente. Ya puedes iniciar sesión con la nueva.", true);
            new VentanaPrincipal().Show();
            Close();
        }

        /// <summary>
        /// Atiende el clic de volver inicio.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void volverInicioClic(Object sender, RoutedEventArgs e)
        {
            new VentanaPrincipal().Show();
            Close();
        }
    }
}
