using Codelex.Clases_BDD;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class IngresarCorreo : Window
    {
        public IngresarCorreo()
        {
            InitializeComponent();
        }
        private void mostrarErrorCodigo(string message)
        {
            txtError1.Text = message;
            txtError1.Visibility = Visibility.Visible;
        }

        private void arrastrarVentana(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
        private void minimizarClic(object sender, RoutedEventArgs e) =>
         WindowState = WindowState.Minimized;

        private void cerrarClic(object sender, RoutedEventArgs e) => Close();

        private void volverInicioClic(Object sender, RoutedEventArgs e)
        {
            VentanaPrincipal m = new VentanaPrincipal();
            m.Show();
            Close();
        }
        private string correoConsulta = "";

        private bool existeCorreoEnSegundoPlano()
        {
            Empleado emp = new Empleado();
            return emp.existeCorreo(correoConsulta);
        }

        private async void siguienteClic(Object sender, RoutedEventArgs e)
        {
            txtError1.Visibility = Visibility.Collapsed;

            string correo = txtEmail.Text.Trim();

            if (correo == "")
            {
                mostrarErrorCodigo("Ingrese un correo.");
                return;
            }

            correoConsulta = correo;
            IsEnabled = false;
            Cargando cargando = Cargando.iniciar(this, "Verificando correo...");

            bool existe = false;
            string errorBD = "";

            try
            {
                existe = await Task.Run(existeCorreoEnSegundoPlano);
            }
            catch (Exception ex)
            {
                errorBD = ex.Message;
            }

            cargando.terminar();
            IsEnabled = true;

            if (errorBD != "")
            {
                mostrarErrorCodigo(errorBD);
                return;
            }

            if (!existe)
            {
                mostrarErrorCodigo("El correo ingresado no tiene una cuenta registrada");
                txtEmail.Focus();
                return;
            }

            new IngresarCodigo(correo).Show();
            Close();
        }
    }
}
