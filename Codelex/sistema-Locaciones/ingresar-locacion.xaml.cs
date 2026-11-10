using Codelex.AccesoDatos;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Codelex
{
    public partial class IngresarLocacion : Window
    {
        Cliente clienteBD = new Cliente();
        Locacion locacionBD = new Locacion();

        string clienteRtn = "";
        string rtnInicial = "";

        public IngresarLocacion()
        {
            InitializeComponent();
        }

        public IngresarLocacion(string rtn) : this()
        {
            rtnInicial = rtn;
        }

        private void ventanaCargada(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (string tipo in locacionBD.tipos)
                {
                    cmbTipo.Items.Add(tipo);
                }

                if (rtnInicial != "")
                {
                    cardClientes.Visibility = Visibility.Collapsed;
                    cardLocaciones.Margin = new Thickness(0, 16, 0, 0);
                    Grid.SetColumnSpan(cardLocaciones, 2);
                    txtNotaCliente.Visibility = Visibility.Collapsed;

                    mostrarCliente(rtnInicial);
                    txtNombre.Focus();
                }
                else
                {
                    cargarClientes();
                    txtBuscarCliente.Focus();
                }
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void cargarClientes()
        {
            DataTable tabla = clienteBD.buscarClientes(txtBuscarCliente.Text.Trim(), "", "ACTIVOS");
            dgClientes.ItemsSource = tabla.DefaultView;
        }

        private void mostrarCliente(string rtn)
        {
            DataTable tabla = clienteBD.obtenerCliente(rtn);

            if (tabla.Rows.Count == 0)
            {
                aviso("No se encontró el cliente.");
                return;
            }

            DataRow fila = tabla.Rows[0];

            clienteRtn = "" + fila["identificacion_cliente"];

            txtRTN.Text = clienteRtn;
            txtEmpresa.Text = "" + fila["nombre_empresa"];
            txtContacto.Text = fila["nombre_cliente"] + " " + fila["apellido_cliente"];
            txtTelefono.Text = "" + fila["telefono_cliente"];

            cargarLocaciones();
        }

        private void cargarLocaciones()
        {
            if (clienteRtn == "")
            {
                dgLocaciones.ItemsSource = null;
                return;
            }

            DataTable tabla = clienteBD.locacionesPorCliente(clienteRtn);
            dgLocaciones.ItemsSource = tabla.DefaultView;
        }

        private void limpiarFormulario()
        {
            txtNombre.Text = "";
            txtDireccion.Text = "";
            cmbTipo.SelectedIndex = -1;
            txtNombre.Focus();
        }

        private void buscarTextoCambiado(object sender, TextChangedEventArgs e)
        {
            try
            {
                cargarClientes();
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void clientesSeleccionCambiada(object sender, SelectionChangedEventArgs e)
        {
            if (dgClientes.SelectedItem == null)
                return;

            DataRowView fila = (DataRowView)dgClientes.SelectedItem;

            try
            {
                mostrarCliente("" + fila["identificacion_cliente"]);
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void limpiarClic(object sender, RoutedEventArgs e)
        {
            limpiarFormulario();
        }

        private void guardarClic(object sender, RoutedEventArgs e)
        {
            try
            {
                string mensaje;

                bool guardada = locacionBD.insertarLocacion(
                    clienteRtn, txtNombre.Text, "" + cmbTipo.SelectedItem, txtDireccion.Text, out mensaje);

                MensajeCodelex.mostrar(this, "Locaciones", mensaje, guardada);

                if (guardada)
                {
                    limpiarFormulario();
                    cargarLocaciones();
                }
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void arrastrarVentana(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void minimizarClic(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void salirClic(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void aviso(string mensaje)
        {
            MensajeCodelex.mostrar(this, "Locaciones", mensaje, false);
        }

        private void mostrarError(Exception ex)
        {
            MensajeCodelex.mostrar(this, "Locaciones", ex.Message, false);
        }
    }
}