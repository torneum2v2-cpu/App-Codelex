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
        private void Next_Click(Object sender, RoutedEventArgs e)
        {
            Empleado emp = new Empleado();

            if (!emp.ExisteCorreo(txtEmail.Text))
            {
                ShowCodeError("El correo ingresado no tiene una cuenta registrada");
                return;
            }
            IngresarCodigo ingresar = new IngresarCodigo(txtEmail.Text);
            ingresar.Show();
            Close();
        }
    }
}
