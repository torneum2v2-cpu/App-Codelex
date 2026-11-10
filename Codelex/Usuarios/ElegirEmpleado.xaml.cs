using Codelex.Clases_BDD;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class ElegirEmpleado : Window
    {
        UsuarioBD usuarioBD = new UsuarioBD();

        public DataRowView? empleadoElegido = null;

        public ElegirEmpleado()
        {
            InitializeComponent();
        }

        private void ventanaCargada(object sender, RoutedEventArgs e)
        {
            cargarEmpleados();
            txtBuscar.Focus();
        }

        private void cargarEmpleados()
        {
            try
            {
                DataTable tabla = usuarioBD.buscarEmpleadosSinUsuario(txtBuscar.Text);
                dgEmpleados.ItemsSource = tabla.DefaultView;
                txtResultado.Text = "Empleados disponibles (" + tabla.Rows.Count + ")";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Buscar empleado", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void elegir()
        {
            if (dgEmpleados.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un empleado de la tabla.", "Buscar empleado",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            empleadoElegido = (DataRowView)dgEmpleados.SelectedItem;
            DialogResult = true;
        }

        private void buscarClic(object sender, RoutedEventArgs e)
        {
            cargarEmpleados();
        }

        private void buscarTeclaPresionada(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                cargarEmpleados();
        }

        private void seleccionarClic(object sender, RoutedEventArgs e)
        {
            elegir();
        }

        private void empleadosDobleClic(object sender, MouseButtonEventArgs e)
        {
            elegir();
        }

        private void cancelarClic(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void arrastrarVentana(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
