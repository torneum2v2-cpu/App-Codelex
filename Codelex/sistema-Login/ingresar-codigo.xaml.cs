using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Codelex
{
    public partial class IngresarCodigo : Window
    {
        private const int LONGITUD_CODIGO = 5;
        private const int SEGUNDOS_REENVIO = 30;
        private readonly TextBox[] casillas;
        private readonly DispatcherTimer temporizador = new() { Interval = TimeSpan.FromSeconds(1) };

        private string correo = "";
        private string codigo = "";
        private int segundos;

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        /// <param name="email">Email.</param>
        public IngresarCodigo(string email)
        {
            InitializeComponent();

            correo = email;

            casillas = new[] { txtC1, txtC2, txtC3, txtC4, txtC5 };
            foreach (TextBox box in casillas)
                DataObject.AddPastingHandler(box, codigoPegado);

            temporizador.Tick += temporizadorTic;
            ContentRendered += ventanaLista;
        }
        /// <summary>
        /// Pone el cursor en la primera casilla cuando la ventana termina de dibujarse.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void ventanaLista(object sender, EventArgs e)
        {
            ContentRendered -= ventanaLista;
            enviarYContar();
        }

        /// <summary>
        /// Envía y contar.
        /// </summary>
        private async void enviarYContar()
        {
            IsEnabled = false;
            Cargando cargando = Cargando.iniciar(this, "Enviando código...");

            await generarYEnviarCodigo();

            cargando.terminar();
            IsEnabled = true;

            if (codigo != "")
            {
                iniciarEspera(SEGUNDOS_REENVIO);
            }
            else
            {
                segundos = 0;
                actualizarEspera();
            }

            txtC1.Focus();
        }

        /// <summary>
        /// Muestra el error del código de verificación escrito.
        /// </summary>
        /// <param name="message">Mensaje a mostrar.</param>
        private void mostrarErrorCodigo(string message)
        {
            txtError2.Text = message;
            txtError2.Visibility = Visibility.Visible;
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
        /// Atiende el clic de cambiar correo.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void cambiarCorreoClic(object sender, RoutedEventArgs e)
        {
            temporizador.Stop();
            new IngresarCorreo().Show();
            Close();
        }

        /// <summary>
        /// Atiende el clic de verificar.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void verificarClic(object sender, RoutedEventArgs e)
        {
            txtError2.Visibility = Visibility.Collapsed;

            if (codigo == "")
            {
                mostrarErrorCodigo("Aún no se ha enviado un código. Usa la opción de reenviar.");
                return;
            }

            string ingresado = string.Concat(Array.ConvertAll(casillas, b => b.Text));
            if (ingresado.Length < LONGITUD_CODIGO)
            {
                mostrarErrorCodigo($"Escribe los {LONGITUD_CODIGO} dígitos del código.");
                return;
            }

            if (ingresado != codigo)
            {
                mostrarErrorCodigo("El código no es correcto. Inténtalo de nuevo.");
                limpiarCodigo();
                txtC1.Focus();
                return;
            }

            temporizador.Stop();

            new NuevaContra(correo).Show();
            Close();
        }
        /// <summary>
        /// Genera y enviar código.
        /// </summary>
        private async Task generarYEnviarCodigo()
        {
            try
            {
                codigo = RandomNumberGenerator.GetInt32(0, 100000).ToString("D5");

                string? p = Environment.GetEnvironmentVariable("CODELEX_CLAVE_CORREO");
                if (string.IsNullOrEmpty(p))
                {
                    codigo = "";
                    mostrarErrorCodigo("Falta configurar la variable de entorno CODELEX_CLAVE_CORREO.");
                    return;
                }

                using (MailMessage message = new MailMessage())
                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    message.From = new MailAddress("codelex1hn@gmail.com");
                    message.To.Add(new MailAddress(correo.Trim()));
                    message.Subject = "Código para cambiar tu contraseña";
                    message.Body = "Escribe este código en la aplicación:\n\n" + codigo +
                                   "\n\nSi no lo solicitaste, ignora este mensaje.";

                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential("codelex1hn@gmail.com", p);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    await smtp.SendMailAsync(message);
                }
            }
            catch (Exception ex)
            {
                codigo = "";
                mostrarErrorCodigo("No se pudo enviar el correo: " + ex.Message);
            }
        }
        /// <summary>
        /// Atiende cada tic de temporizador.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void temporizadorTic(object? sender, EventArgs e)
        {
            segundos--;
            if (segundos <= 0) temporizador.Stop();
            actualizarEspera();
        }

        /// <summary>
        /// Actualiza espera.
        /// </summary>
        private void actualizarEspera()
        {
            bool waiting = segundos > 0;
            txtCountdown.Text = $"Podrás reenviar el código en {segundos} s";
            if (waiting)
                txtCountdown.Visibility = Visibility.Visible;
            else
                txtCountdown.Visibility = Visibility.Collapsed;
            if (waiting)
                tbResend.Visibility = Visibility.Collapsed;
            else
                tbResend.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// Inicia espera.
        /// </summary>
        /// <param name="seconds">Seconds.</param>
        private void iniciarEspera(int seconds)
        {
            segundos = seconds;
            actualizarEspera();
            temporizador.Start();
        }
        /// <summary>
        /// Atiende el clic de reenviar.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void reenviarClic(object sender, RoutedEventArgs e)
        {
            limpiarCodigo();
            txtError2.Visibility = Visibility.Collapsed;
            enviarYContar();
        }

        /// <summary>
        /// Devuelve la posición de la casilla dentro del código de verificación.
        /// </summary>
        /// <param name="box">Casilla de texto.</param>
        /// <returns>Posición de la casilla (0 a 5).</returns>
        private int indiceDe(TextBox box) => Array.IndexOf(casillas, box);

        /// <summary>
        /// Limpia código.
        /// </summary>
        private void limpiarCodigo()
        {
            foreach (TextBox b in casillas) b.Clear();
        }

        /// <summary>
        /// Atiende el foco recibido por código.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void codigoFocoRecibido(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox box) box.SelectAll();
        }

        /// <summary>
        /// Atiende el texto escrito en código.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void codigoPreviaTextoIngresado(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;

            if (sender is not TextBox box || e.Text.Length != 1) return;
            char c = e.Text[0];
            if (c < '0' || c > '9') return;

            box.Text = e.Text;
            int i = indiceDe(box);
            if (i < LONGITUD_CODIGO - 1) casillas[i + 1].Focus();
        }

        /// <summary>
        /// Atiende la tecla presionada en código.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void codigoPreviaTeclaPresionada(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox box) return;
            int i = indiceDe(box);

            switch (e.Key)
            {
                case Key.Back:
                    e.Handled = true;
                    if (box.Text.Length > 0) box.Clear();
                    else if (i > 0)
                    {
                        casillas[i - 1].Clear();
                        casillas[i - 1].Focus();
                    }
                    break;

                case Key.Delete:
                    e.Handled = true;
                    box.Clear();
                    break;

                case Key.Left:
                    e.Handled = true;
                    if (i > 0) casillas[i - 1].Focus();
                    break;

                case Key.Right:
                    e.Handled = true;
                    if (i < LONGITUD_CODIGO - 1) casillas[i + 1].Focus();
                    break;

                case Key.Space:
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// Atiende el pegado en código.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void codigoPegado(object sender, DataObjectPastingEventArgs e)
        {
            e.CancelCommand();

            if (sender is not TextBox box) return;
            if (e.DataObject.GetData(DataFormats.UnicodeText) is not string text) return;

            string digits = string.Concat(text.Where(char.IsAsciiDigit)).Trim();
            if (digits.Length == 0) return;
            if (digits.Length > LONGITUD_CODIGO) digits = digits[..LONGITUD_CODIGO];

            int start = indiceDe(box);
            if (digits.Length == LONGITUD_CODIGO)
                start = 0;
            int last = start;
            for (int k = 0; k < digits.Length && start + k < LONGITUD_CODIGO; k++)
            {
                casillas[start + k].Text = digits[k].ToString();
                last = start + k;
            }

            casillas[Math.Min(last + 1, LONGITUD_CODIGO - 1)].Focus();
        }
    }
}
