using Codelex.Clases_BDD;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class ClienteDetalle : Window
    {
        Cliente clienteBD = new Cliente();
        string rtn;

        public bool EditarCliente = false;

        public ClienteDetalle(string rtn)
        {
            InitializeComponent();
            this.rtn = rtn;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CargarDatosCliente();
                CargarLocaciones();
                CargarOrdenes();
                CargarFacturas();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Detalle del cliente", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarDatosCliente()
        {
            DataTable tabla = clienteBD.ObtenerCliente(rtn);

            if (tabla.Rows.Count == 0)
            {
                MessageBox.Show("No se encontró el cliente.", "Detalle del cliente", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRow fila = tabla.Rows[0];

            txtRTN.Text = fila["identificacionCliente"].ToString();
            txtEmpresa.Text = fila["nombreEmpresa"].ToString();
            txtContacto.Text = fila["nombreCliente"].ToString();
            txtApellido.Text = fila["apellidoCliente"].ToString();
            txtTelefono.Text = fila["telefonoCliente"].ToString();
            txtCorreo.Text = fila["correoCliente"].ToString();
            txtEstado.Text = fila["estado"].ToString();
        }

        private void CargarLocaciones()
        {
            DataTable tabla = clienteBD.LocacionesPorCliente(rtn);
            lstLocaciones.Items.Clear();

            foreach (DataRow fila in tabla.Rows)
            {
                string linea = fila["nombreLocacion"] + " · " + fila["tipoLocacion"] + " · " + fila["direccion"];
                lstLocaciones.Items.Add(linea);
            }
        }

        private void CargarOrdenes()
        {
            DataTable tabla = clienteBD.OrdenesPorCliente(rtn);
            lstOrdenes.Items.Clear();

            foreach (DataRow fila in tabla.Rows)
            {
                string linea = fila["orden_id"] + " · " + fila["servicio"] + " · " + Fecha(fila["fecha"]) + " · " + fila["estado"];
                lstOrdenes.Items.Add(linea);
            }
        }

        private void CargarFacturas()
        {
            DataTable tabla = clienteBD.FacturasPorCliente(rtn);
            lstFacturas.Items.Clear();

            foreach (DataRow fila in tabla.Rows)
            {
                string monto = "";
                if (fila["monto"] != DBNull.Value)
                    monto = "L " + Convert.ToDecimal(fila["monto"]).ToString("N2");

                string linea = fila["factura_id"] + " · " + Fecha(fila["fecha"]) + " · " + fila["estado_pago"] + " · " + monto;
                lstFacturas.Items.Add(linea);
            }
        }

        private string Fecha(object valor)
        {
            if (valor == DBNull.Value)
                return "";

            return Convert.ToDateTime(valor).ToString("dd/MM/yyyy");
        }

        private void NuevaOrden_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("El módulo de Órdenes todavía no está disponible.", "Detalle del cliente",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Editar_Click(object sender, RoutedEventArgs e)
        {
            EditarCliente = true;
            Close();
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
