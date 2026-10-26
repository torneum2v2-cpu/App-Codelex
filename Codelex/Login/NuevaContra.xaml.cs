using Codelex.BDD;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Codelex
{
    public partial class NuevaContra : Window
    {
        private string _email = "";

        public NuevaContra(string email)
        {
            InitializeComponent();
            _email = email;
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private string ObtenerContra1()
        {
            if (btnShowPassword.IsChecked == true)
                return txtPasswordVisible.Text;
            return txtPassword.Password;
        }

        private string ObtenerContra2()
        {
            if (btnShowPassword2.IsChecked == true)
                return txtPasswordVisible2.Text;
            return txtPassword2.Password;
        }

        private void MostrarOcultar(ToggleButton boton, PasswordBox oculta, TextBox visible)
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

        private void ShowPassword_Changed(object sender, RoutedEventArgs e)
        {
            if (sender == btnShowPassword)
                MostrarOcultar(btnShowPassword, txtPassword, txtPasswordVisible);
            else
                MostrarOcultar(btnShowPassword2, txtPassword2, txtPasswordVisible2);
        }

        private void Rest_Click(Object sender, RoutedEventArgs e)
        {
            txtError.Visibility = Visibility.Collapsed;

            Usuario user = new Usuario();
            string mensaje = "";

            try
            {
                bool cambiada = user.CambiarContrasena(_email, ObtenerContra1(), ObtenerContra2(), out mensaje);

                if (!cambiada)
                {
                    ShowError(mensaje);
                    return;
                }
            }
            catch (Exception ex)
            {
                ShowError("Error al conectar con la base de datos: " + ex.Message);
                return;
            }

            MensajeCodelex.Mostrar(this, "Contraseña actualizada",
                    "Tu contraseña se cambió correctamente. Ya puedes iniciar sesión con la nueva.", true);
            new MainWindow().Show();
            Close();
        }

        private void BackToLogin_Click(Object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }
    }
}
