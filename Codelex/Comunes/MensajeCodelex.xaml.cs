using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Codelex
{
    public partial class MensajeCodelex : Window
    {
        public MensajeCodelex(string titulo, string mensaje, bool exito)
        {
            InitializeComponent();

            txtTitulo.Text = titulo;
            txtMensaje.Text = mensaje;

            if (exito)
            {
                pathCheck.Visibility = Visibility.Visible;
                pathX.Visibility = Visibility.Collapsed;
                elIcono.Stroke = (Brush)FindResource("Primary");
            }
            else
            {
                pathCheck.Visibility = Visibility.Collapsed;
                pathX.Visibility = Visibility.Visible;
                elIcono.Stroke = (Brush)FindResource("Danger");
            }
        }

        public static void Mostrar(Window dueno, string titulo, string mensaje, bool exito)
        {
            MensajeCodelex ventana = new MensajeCodelex(titulo, mensaje, exito);

            if (dueno != null)
            {
                ventana.Owner = dueno;
                ventana.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                ventana.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            ventana.ShowDialog();
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
