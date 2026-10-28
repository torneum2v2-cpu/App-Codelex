using Codelex.BDD;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (_, _) => txtUser.Focus();
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void ShowPassword_Changed(object sender, RoutedEventArgs e)
        {
            if (btnShowPassword.IsChecked == true)
            {
                txtPasswordVisible.Text = txtPassword.Password;
                txtPassword.Visibility = Visibility.Collapsed;
                txtPasswordVisible.Visibility = Visibility.Visible;
                txtPasswordVisible.Focus();
                txtPasswordVisible.CaretIndex = txtPasswordVisible.Text.Length;
            }
            else
            {
                txtPassword.Password = txtPasswordVisible.Text;
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                txtPassword.Visibility = Visibility.Visible;
                txtPassword.Focus();
            }
        }

        private string CurrentPassword =>
            btnShowPassword.IsChecked == true ? txtPasswordVisible.Text : txtPassword.Password;

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            txtError.Visibility = Visibility.Collapsed;

            string usuario = txtUser.Text.Trim();
            string password = CurrentPassword;

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrEmpty(password))
            {
                ShowError("Escribe tu usuario y tu contraseña para continuar.");
                return;
            }

            try
            {
                Usuario usuarioDAL = new Usuario();

                bool esValido = usuarioDAL.ValidarLogin(usuario, password);

                if (esValido)
                {
                    DobleFactor verificacion = new DobleFactor(usuarioDAL);
                    verificacion.Show();
                    Close();
                }
                else
                {
                    ShowError("Usuario o contraseña incorrectos. Revisa los datos e inténtalo de nuevo.");
                }
            }
            catch (Exception ex)
            {
                ShowError("Error al conectar con la base de datos: " + ex.Message);
            }
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            IngresarCorreo con = new IngresarCorreo();
            con.Show();
            Close();
        }

        private void RequestAccess_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Aquí puedes abrir tu formulario o contacto de soporte.",
                            "Codelex", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
