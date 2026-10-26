using Codelex.Clases_BDD;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class ElegirEmpleado : Window
    {
        UsuarioBD usuarioBD = new UsuarioBD();

        public DataRowView? EmpleadoElegido = null;

        public ElegirEmpleado()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarEmpleados();
            txtBuscar.Focus();
        }

        private void CargarEmpleados()
        {
            try
            {
                DataTable tabla = usuarioBD.BuscarEmpleadosSinUsuario(txtBuscar.Text);
                dgEmpleados.ItemsSource = tabla.DefaultView;
                txtResultado.Text = "Empleados disponibles (" + tabla.Rows.Count + ")";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Buscar empleado", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Elegir()
        {
            if (dgEmpleados.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un empleado de la tabla.", "Buscar empleado",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            EmpleadoElegido = (DataRowView)dgEmpleados.SelectedItem;
            DialogResult = true;
        }

        private void Buscar_Click(object sender, RoutedEventArgs e)
        {
            CargarEmpleados();
        }

        private void Buscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                CargarEmpleados();
        }

        private void Seleccionar_Click(object sender, RoutedEventArgs e)
        {
            Elegir();
        }

        private void dgEmpleados_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            Elegir();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
