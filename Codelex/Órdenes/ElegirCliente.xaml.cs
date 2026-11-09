using Codelex.Clases_BDD;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class ElegirCliente : Window
    {
        Cliente clienteBD = new Cliente();

        public DataRowView? ClienteElegido = null;

        public ElegirCliente()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarClientes();
            txtBuscar.Focus();
        }

        private void CargarClientes()
        {
            try
            {
                DataTable tabla = clienteBD.BuscarClientes(txtBuscar.Text.Trim(), "", "ACTIVOS");
                dgClientes.ItemsSource = tabla.DefaultView;
                txtResultado.Text = "Clientes activos (" + tabla.Rows.Count + ")";
            }
            catch (Exception ex)
            {
                MensajeCodelex.Mostrar(this, "Buscar cliente", ex.Message, false);
            }
        }

        private void Elegir()
        {
            if (dgClientes.SelectedItem == null)
            {
                MensajeCodelex.Mostrar(this, "Buscar cliente", "Seleccione un cliente de la tabla.", false);
                return;
            }

            ClienteElegido = (DataRowView)dgClientes.SelectedItem;
            DialogResult = true;
        }

        private void Buscar_Click(object sender, RoutedEventArgs e)
        {
            CargarClientes();
        }

        private void Buscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                CargarClientes();
        }

        private void Seleccionar_Click(object sender, RoutedEventArgs e)
        {
            Elegir();
        }

        private void dgClientes_DoubleClick(object sender, MouseButtonEventArgs e)
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
