using Codelex.Clases_BDD;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (string tipo in locacionBD.Tipos)
                {
                    cmbTipo.Items.Add(tipo);
                }

                if (rtnInicial != "")
                {
                    cardClientes.Visibility = Visibility.Collapsed;
                    cardLocaciones.Margin = new Thickness(0, 16, 0, 0);
                    Grid.SetColumnSpan(cardLocaciones, 2);
                    txtNotaCliente.Visibility = Visibility.Collapsed;

                    MostrarCliente(rtnInicial);
                    txtNombre.Focus();
                }
                else
                {
                    CargarClientes();
                    txtBuscarCliente.Focus();
                }
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void CargarClientes()
        {
            DataTable tabla = clienteBD.BuscarClientes(txtBuscarCliente.Text.Trim(), "", "ACTIVOS");
            dgClientes.ItemsSource = tabla.DefaultView;
        }

        private void MostrarCliente(string rtn)
        {
            DataTable tabla = clienteBD.ObtenerCliente(rtn);

            if (tabla.Rows.Count == 0)
            {
                Aviso("No se encontró el cliente.");
                return;
            }

            DataRow fila = tabla.Rows[0];

            clienteRtn = "" + fila["identificacionCliente"];

            txtRTN.Text = clienteRtn;
            txtEmpresa.Text = "" + fila["nombreEmpresa"];
            txtContacto.Text = fila["nombreCliente"] + " " + fila["apellidoCliente"];
            txtTelefono.Text = "" + fila["telefonoCliente"];

            CargarLocaciones();
        }

        private void CargarLocaciones()
        {
            if (clienteRtn == "")
            {
                dgLocaciones.ItemsSource = null;
                return;
            }

            DataTable tabla = clienteBD.LocacionesPorCliente(clienteRtn);
            dgLocaciones.ItemsSource = tabla.DefaultView;
        }

        private void LimpiarFormulario()
        {
            txtNombre.Text = "";
            txtDireccion.Text = "";
            cmbTipo.SelectedIndex = -1;
            txtNombre.Focus();
        }

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                CargarClientes();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void dgClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgClientes.SelectedItem == null)
                return;

            DataRowView fila = (DataRowView)dgClientes.SelectedItem;

            try
            {
                MostrarCliente("" + fila["identificacionCliente"]);
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void Limpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string mensaje;

                bool guardada = locacionBD.InsertarLocacion(
                    clienteRtn, txtNombre.Text, "" + cmbTipo.SelectedItem, txtDireccion.Text, out mensaje);

                MensajeCodelex.Mostrar(this, "Locaciones", mensaje, guardada);

                if (guardada)
                {
                    LimpiarFormulario();
                    CargarLocaciones();
                }
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimizar_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Aviso(string mensaje)
        {
            MensajeCodelex.Mostrar(this, "Locaciones", mensaje, false);
        }

        private void MostrarError(Exception ex)
        {
            MensajeCodelex.Mostrar(this, "Locaciones", ex.Message, false);
        }
    }
}