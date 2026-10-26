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
        private void ShowCodeError(string message)
        {
            txtError1.Text = message;
            txtError1.Visibility = Visibility.Visible;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
        private void Minimize_Click(object sender, RoutedEventArgs e) =>
         WindowState = WindowState.Minimized;

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void BackToLogin_Click(Object sender, RoutedEventArgs e)
        {
            MainWindow m = new MainWindow();
            m.Show();
            Close();
        }
        private string _correoConsulta = "";

        private bool ExisteCorreoEnSegundoPlano()
        {
            Empleado emp = new Empleado();
            return emp.ExisteCorreo(_correoConsulta);
        }

        private async void Next_Click(Object sender, RoutedEventArgs e)
        {
            txtError1.Visibility = Visibility.Collapsed;

            string correo = txtEmail.Text.Trim();

            if (correo == "")
            {
                ShowCodeError("Ingrese un correo.");
                return;
            }

            _correoConsulta = correo;
            IsEnabled = false;
            Cargando cargando = Cargando.Iniciar(this, "Verificando correo...");

            bool existe = false;
            string errorBD = "";

            try
            {
                existe = await Task.Run(ExisteCorreoEnSegundoPlano);
            }
            catch (Exception ex)
            {
                errorBD = ex.Message;
            }

            cargando.Terminar();
            IsEnabled = true;

            if (errorBD != "")
            {
                ShowCodeError(errorBD);
                return;
            }

            if (!existe)
            {
                ShowCodeError("El correo ingresado no tiene una cuenta registrada");
                txtEmail.Focus();
                return;
            }

            new IngresarCodigo(correo).Show();
            Close();
        }
    }
}
