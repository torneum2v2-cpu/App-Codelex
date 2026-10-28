using Codelex.BDD;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Codelex
{
    public partial class VerificarTelefono : Window
    {
        private Usuario usuario;

        public VerificarTelefono(Usuario usuarioLogin)
        {
            InitializeComponent();
            usuario = usuarioLogin;

            if (usuario.TieneLlave)
            {
                txtTitulo.Text = "Verifica que eres tú";
                txtDescripcion.Text = "Usa tu teléfono para entrar. Te pedirá Face ID, huella o reconocimiento facial.";
                btnVerificar.Content = "Verificar con mi teléfono";
            }
            else
            {
                txtTitulo.Text = "Registra tu teléfono";
                txtDescripcion.Text = "Para entrar a Codelex necesitas registrar tu teléfono. Escanea el código QR que mostrará Windows y confirma con Face ID, huella o reconocimiento facial.";
                btnVerificar.Content = "Registrar mi teléfono";
            }
        }

        private async void Verificar_Click(object sender, RoutedEventArgs e)
        {
            txtError.Visibility = Visibility.Collapsed;
            txtEspera.Visibility = Visibility.Visible;
            btnVerificar.IsEnabled = false;

            IntPtr ventana = new WindowInteropHelper(this).Handle;
            bool primeraVez = !usuario.TieneLlave;
            string mensaje;

            try
            {
                if (primeraVez)
                    mensaje = await usuario.RegistrarLlave(ventana);
                else
                    mensaje = await usuario.VerificarLlave(ventana);
            }
            catch (Exception ex)
            {
                mensaje = "Error al conectar con la base de datos: " + ex.Message;
            }

            txtEspera.Visibility = Visibility.Collapsed;
            btnVerificar.IsEnabled = true;

            if (mensaje != "")
            {
                txtError.Text = mensaje;
                txtError.Visibility = Visibility.Visible;
                return;
            }

            if (primeraVez)
                MensajeCodelex.Mostrar(this, "Teléfono registrado", "Tu teléfono quedó registrado. La próxima vez solo tendrás que confirmar con él.", true);

            Menu menu = new Menu();
            menu.Show();
            Close();
        }

        private void Volver_Click(object sender, RoutedEventArgs e)
        {
            MainWindow login = new MainWindow();
            login.Show();
            Close();
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
