using Codelex.BDD;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class VentanaPrincipal : Window
    {
        public VentanaPrincipal()
        {
            InitializeComponent();
            Loaded += (_, _) => txtUser.Focus();
        }

        private void arrastrarVentana(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void minimizarClic(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        private void cerrarClic(object sender, RoutedEventArgs e) => Close();

        private void mostrarContrasenaCambiado(object sender, RoutedEventArgs e)
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

        private void iniciarSesionClic(object sender, RoutedEventArgs e)
        {
            txtError.Visibility = Visibility.Collapsed;

            string usuario = txtUser.Text.Trim();
            string password = CurrentPassword;

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrEmpty(password))
            {
                mostrarError("Escribe tu usuario y tu contraseña para continuar.");
                return;
            }

            try
            {
                Usuario usuarioDAL = new Usuario();

                bool esValido = usuarioDAL.validarLogin(usuario, password);

                if (esValido)
                {
                    bool recordar = chkRemember.IsChecked == true;

                    // Cuenta ya recordada en este equipo: entra directo, sin verificar con el teléfono.
                    if (CuentasRecordadas.existe(usuarioDAL.NombreUsuario))
                    {
                        if (recordar)
                            CuentasRecordadas.guardar(usuarioDAL.NombreUsuario, usuarioDAL.NombreCompleto, password);

                        Sesion.iniciar(usuarioDAL);
                        new Menu().Show();
                        Close();
                        return;
                    }

                    VerificarTelefono verificacion = new VerificarTelefono(usuarioDAL, recordar, password);
                    verificacion.Show();
                    Close();
                }
                else
                {
                    mostrarError("Usuario o contraseña incorrectos. Revisa los datos e inténtalo de nuevo.");
                }
            }
            catch (Exception ex)
            {
                mostrarError("Error al conectar con la base de datos: " + ex.Message);
            }
        }

        private void mostrarError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }

        private void olvidoContrasenaClic(object sender, RoutedEventArgs e)
        {
            IngresarCorreo con = new IngresarCorreo();
            con.Show();
            Close();
        }

        private void solicitarAccesoClic(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Aquí puedes abrir tu formulario o contacto de soporte.",
                            "Codelex", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}