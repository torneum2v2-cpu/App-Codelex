using Codelex.AccesoDatos;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class ElegirCliente : Window
    {
        Cliente clienteBD = new Cliente();

        public DataRowView? clienteElegido = null;

        public ElegirCliente()
        {
            InitializeComponent();
        }

        private void ventanaCargada(object sender, RoutedEventArgs e)
        {
            cargarClientes();
            txtBuscar.Focus();
        }

        private void cargarClientes()
        {
            try
            {
                DataTable tabla = clienteBD.buscarClientes(txtBuscar.Text.Trim(), "", "ACTIVOS");
                dgClientes.ItemsSource = tabla.DefaultView;
                txtResultado.Text = "Clientes activos (" + tabla.Rows.Count + ")";
            }
            catch (Exception ex)
            {
                MensajeCodelex.mostrar(this, "Buscar cliente", ex.Message, false);
            }
        }

        private void elegir()
        {
            if (dgClientes.SelectedItem == null)
            {
                MensajeCodelex.mostrar(this, "Buscar cliente", "Seleccione un cliente de la tabla.", false);
                return;
            }

            clienteElegido = (DataRowView)dgClientes.SelectedItem;
            DialogResult = true;
        }

        private void buscarClic(object sender, RoutedEventArgs e)
        {
            cargarClientes();
        }

        private void buscarTeclaPresionada(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                cargarClientes();
        }

        private void seleccionarClic(object sender, RoutedEventArgs e)
        {
            elegir();
        }

        private void clientesDobleClic(object sender, MouseButtonEventArgs e)
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
