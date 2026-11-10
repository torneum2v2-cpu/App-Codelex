using Codelex.AccesoDatos;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Codelex
{
    public partial class VerificarTelefono : Window
    {
        private Usuario usuario;
        private readonly bool recordar;
        private readonly string contrasena;

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        /// <param name="usuarioLogin">Usuario login.</param>
        /// <param name="recordar">Recordar.</param>
        /// <param name="password">Password.</param>
        public VerificarTelefono(Usuario usuarioLogin, bool recordar = false, string password = "")
        {
            InitializeComponent();
            usuario = usuarioLogin;
            this.recordar = recordar;
            contrasena = password;

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

        /// <summary>
        /// Atiende el clic de verificar.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private async void verificarClic(object sender, RoutedEventArgs e)
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
                    mensaje = await usuario.registrarLlave(ventana);
                else
                    mensaje = await usuario.verificarLlave(ventana);
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

            
            if (recordar)
                CuentasRecordadas.guardar(usuario.NombreUsuario, usuario.NombreCompleto, contrasena);

            if (primeraVez)
                MensajeCodelex.mostrar(this, "Teléfono registrado", "Tu teléfono quedó registrado. La próxima vez solo tendrás que confirmar con él.", true);

            Sesion.iniciar(usuario);
            Menu menu = new Menu();
            menu.Show();
            Close();
        }

        /// <summary>
        /// Vuelve a la ventana anterior.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void volverClic(object sender, RoutedEventArgs e)
        {
            VentanaPrincipal login = new VentanaPrincipal();
            login.Show();
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
    }
}