using Codelex.BDD;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Codelex
{
    public partial class NuevaContra : Window
    {
        private string correo = "";

        public NuevaContra(string email)
        {
            InitializeComponent();
            correo = email;
        }

        private void mostrarError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }

        private void arrastrarVentana(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void minimizarClic(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        private void cerrarClic(object sender, RoutedEventArgs e) => Close();

        private string obtenerContra1()
        {
            if (btnShowPassword.IsChecked == true)
                return txtPasswordVisible.Text;
            return txtPassword.Password;
        }

        private string obtenerContra2()
        {
            if (btnShowPassword2.IsChecked == true)
                return txtPasswordVisible2.Text;
            return txtPassword2.Password;
        }

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

        private void mostrarContrasenaCambiado(object sender, RoutedEventArgs e)
        {
            if (sender == btnShowPassword)
                mostrarOcultar(btnShowPassword, txtPassword, txtPasswordVisible);
            else
                mostrarOcultar(btnShowPassword2, txtPassword2, txtPasswordVisible2);
        }

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

        private void volverInicioClic(Object sender, RoutedEventArgs e)
        {
            new VentanaPrincipal().Show();
            Close();
        }
    }
}
