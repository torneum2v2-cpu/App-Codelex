using Codelex.BDD;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class DobleFactor : Window
    {
        private Usuario usuario;
        private string secreto;

        public DobleFactor(Usuario usuarioLogin)
        {
            InitializeComponent();
            usuario = usuarioLogin;

            if (usuario.DobleFactorActivo)
            {
                secreto = usuario.SecretoDobleFactor;
                pnlQR.Visibility = Visibility.Collapsed;
                txtTitulo.Text = "Verificación en dos pasos";
                txtDescripcion.Text = "Abre la app de códigos en tu iPhone, desbloquéala con Face ID y escribe el código de Codelex.";
            }
            else
            {
                secreto = usuario.GenerarSecreto();
                imgQR.Source = usuario.CrearCodigoQR(secreto);
                txtTitulo.Text = "Activa la verificación en dos pasos";
                txtDescripcion.Text = "Escanea este código con la app de códigos de tu iPhone y escribe el código de 6 dígitos que aparece.";
            }

            txtCodigo.Focus();
        }

        private void Verificar_Click(object sender, RoutedEventArgs e)
        {
            string mensaje;
            if (!usuario.ConfirmarDobleFactor(secreto, txtCodigo.Text, out mensaje))
            {
                txtError.Text = mensaje;
                txtError.Visibility = Visibility.Visible;
                return;
            }

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

