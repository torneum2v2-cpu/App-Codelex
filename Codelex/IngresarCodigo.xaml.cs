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
        private const int CodeLength = 5;
        private const int ResendSeconds = 30;
        private readonly TextBox[] _boxes;
        private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };

        private string _email = "";
        private string _codigo = "";
        private int _seconds;

        public IngresarCodigo(string email)
        {
            InitializeComponent();

            _email = email;

            _boxes = new[] { txtC1, txtC2, txtC3, txtC4, txtC5 };
            foreach (var box in _boxes)
                DataObject.AddPastingHandler(box, Code_Paste);

            _timer.Tick += Timer_Tick;
            GenerarYEnviarCodigo();        
            StartCooldown(ResendSeconds);
        }

        private void ShowCodeError(string message)
        {
            txtError2.Text = message;
            txtError2.Visibility = Visibility.Visible;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
        private void Minimize_Click(object sender, RoutedEventArgs e) =>
         WindowState = WindowState.Minimized;

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void ChangeEmail_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            new IngresarCorreo().Show();
            Close();
        }

        private void Verify_Click(object sender, RoutedEventArgs e)
        {
            txtError2.Visibility = Visibility.Collapsed;

            string ingresado = string.Concat(Array.ConvertAll(_boxes, b => b.Text));
            if (ingresado.Length < CodeLength)
            {
                ShowCodeError($"Escribe los {CodeLength} dígitos del código.");
                return;
            }

            if (ingresado != _codigo)
            {
                ShowCodeError("El código no es correcto. Inténtalo de nuevo.");
                ClearCode();
                txtC1.Focus();
                return;
            }

            _timer.Stop();

            new NuevaContra().Show();
            Close();
        }
        private void GenerarYEnviarCodigo()
        {
            try
            {
                _codigo = RandomNumberGenerator.GetInt32(0, 100000).ToString("D5");

                const string p = "vkds dwtb vmtk dldo";

                using (MailMessage message = new MailMessage())
                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    message.From = new MailAddress("codelex1hn@gmail.com");
                    message.To.Add(new MailAddress(_email.Trim()));
                    message.Subject = "Código para cambiar tu contraseña";
                    message.Body = "Escribe este código en la aplicación:\n\n" + _codigo +
                                   "\n\nSi no lo solicitaste, ignora este mensaje.";

                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential("codelex1hn@gmail.com", p);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                _codigo = "";
                ShowCodeError("No se pudo enviar el correo: " + ex.Message);
            }
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            _seconds--;
            if (_seconds <= 0) _timer.Stop();
            UpdateCooldownUi();
        }

        private void UpdateCooldownUi()
        {
            bool waiting = _seconds > 0;
            txtCountdown.Text = $"Podrás reenviar el código en {_seconds} s";
            txtCountdown.Visibility = waiting ? Visibility.Visible : Visibility.Collapsed;
            tbResend.Visibility = waiting ? Visibility.Collapsed : Visibility.Visible;
        }
        private void StartCooldown(int seconds)
        {
            _seconds = seconds;
            UpdateCooldownUi();
            _timer.Start();
        }
        private void Resend_Click(object sender, RoutedEventArgs e)
        {
            GenerarYEnviarCodigo();
            ClearCode();
            txtError2.Visibility = Visibility.Collapsed;
            StartCooldown(ResendSeconds);
            txtC1.Focus();
        }

        private int IndexOf(TextBox box) => Array.IndexOf(_boxes, box);

        private void ClearCode()
        {
            foreach (var b in _boxes) b.Clear();
        }

        private void Code_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox box) box.SelectAll();
        }

        private void Code_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;

            if (sender is not TextBox box || e.Text.Length != 1) return;
            char c = e.Text[0];
            if (c < '0' || c > '9') return;

            box.Text = e.Text;
            int i = IndexOf(box);
            if (i < CodeLength - 1) _boxes[i + 1].Focus();
        }

        private void Code_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox box) return;
            int i = IndexOf(box);

            switch (e.Key)
            {
                case Key.Back:
                    e.Handled = true;
                    if (box.Text.Length > 0) box.Clear();
                    else if (i > 0)
                    {
                        _boxes[i - 1].Clear();
                        _boxes[i - 1].Focus();
                    }
                    break;

                case Key.Delete:
                    e.Handled = true;
                    box.Clear();
                    break;

                case Key.Left:
                    e.Handled = true;
                    if (i > 0) _boxes[i - 1].Focus();
                    break;

                case Key.Right:
                    e.Handled = true;
                    if (i < CodeLength - 1) _boxes[i + 1].Focus();
                    break;

                case Key.Space:
                    e.Handled = true;
                    break;
            }
        }

        private void Code_Paste(object sender, DataObjectPastingEventArgs e)
        {
            e.CancelCommand();

            if (sender is not TextBox box) return;
            if (e.DataObject.GetData(DataFormats.UnicodeText) is not string text) return;

            string digits = string.Concat(text.Where(char.IsAsciiDigit)).Trim();
            if (digits.Length == 0) return;
            if (digits.Length > CodeLength) digits = digits[..CodeLength];

            int start = digits.Length == CodeLength ? 0 : IndexOf(box);
            int last = start;
            for (int k = 0; k < digits.Length && start + k < CodeLength; k++)
            {
                _boxes[start + k].Text = digits[k].ToString();
                last = start + k;
            }

            _boxes[Math.Min(last + 1, CodeLength - 1)].Focus();
        }
    }
}
