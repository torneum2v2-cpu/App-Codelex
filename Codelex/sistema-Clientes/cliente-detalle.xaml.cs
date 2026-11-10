using Codelex.AccesoDatos;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class ClienteDetalle : Window
    {
        Cliente clienteBD = new Cliente();
        string rtn;

        public bool editarCliente = false;

        public ClienteDetalle(string rtn)
        {
            InitializeComponent();
            this.rtn = rtn;
        }

        private void ventanaCargada(object sender, RoutedEventArgs e)
        {
            try
            {
                cargarDatosCliente();
                cargarLocaciones();
                cargarOrdenes();
                cargarFacturas();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Detalle del cliente", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cargarDatosCliente()
        {
            DataTable tabla = clienteBD.obtenerCliente(rtn);

            if (tabla.Rows.Count == 0)
            {
                MessageBox.Show("No se encontró el cliente.", "Detalle del cliente", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRow fila = tabla.Rows[0];

            lblTipoId.Text = fila["tipo_identificacion"].ToString();
            txtRTN.Text = fila["identificacion_cliente"].ToString();
            txtEmpresa.Text = fila["nombre_empresa"].ToString();
            txtContacto.Text = fila["nombre_cliente"].ToString();
            txtApellido.Text = fila["apellido_cliente"].ToString();
            txtTelefono.Text = fila["telefono_cliente"].ToString();
            txtCorreo.Text = fila["correo_cliente"].ToString();
            txtEstado.Text = fila["estado"].ToString();
        }

        private void cargarLocaciones()
        {
            DataTable tabla = clienteBD.locacionesPorCliente(rtn);
            lstLocaciones.Items.Clear();

            foreach (DataRow fila in tabla.Rows)
            {
                string linea = fila["nombre_locacion"] + " · " + fila["tipo_locacion"] + " · " + fila["direccion"];
                lstLocaciones.Items.Add(linea);
            }
        }

        private void cargarOrdenes()
        {
            DataTable tabla = clienteBD.ordenesPorCliente(rtn);
            lstOrdenes.Items.Clear();

            foreach (DataRow fila in tabla.Rows)
            {
                string linea = fila["orden_id"] + " · " + fila["servicio"] + " · " + fecha(fila["fecha"]) + " · " + fila["estado"];
                lstOrdenes.Items.Add(linea);
            }
        }

        private void cargarFacturas()
        {
            DataTable tabla = clienteBD.facturasPorCliente(rtn);
            lstFacturas.Items.Clear();

            foreach (DataRow fila in tabla.Rows)
            {
                string monto = "";
                if (fila["monto"] != DBNull.Value)
                    monto = "L " + Convert.ToDecimal(fila["monto"]).ToString("N2");

                string linea = fila["factura_id"] + " · " + fecha(fila["fecha"]) + " · " + fila["estado_pago"] + " · " + monto;
                lstFacturas.Items.Add(linea);
            }
        }

        private string fecha(object valor)
        {
            if (valor == DBNull.Value)
                return "";

            return Convert.ToDateTime(valor).ToString("dd/MM/yyyy");
        }

        private void nuevaOrdenClic(object sender, RoutedEventArgs e)
        {
            abrirOrden();
        }

        private void agregarOrdenClic(object sender, RoutedEventArgs e)
        {
            abrirOrden();
        }

        private void abrirOrden()
        {
            DataRow? cotizacion = SeleccionarCotizacion.elegir(this, rtn);
            if (cotizacion == null)
                return;

            IngresarOrden ventana = new IngresarOrden(cotizacion);
            ventana.Owner = this;
            ventana.ShowDialog();

            try
            {
                cargarOrdenes();
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void agregarLocacionClic(object sender, RoutedEventArgs e)
        {
            IngresarLocacion ventana = new IngresarLocacion(rtn);
            ventana.Owner = this;
            ventana.ShowDialog();

            try
            {
                cargarLocaciones();
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void agregarFacturaClic(object sender, RoutedEventArgs e)
        {
            MensajeCodelex.mostrar(this, "Facturas", "Próximamente", true);
        }

        private void mostrarError(Exception ex)
        {
            MensajeCodelex.mostrar(this, "Detalle del cliente", ex.Message, false);
        }

        private void editarClic(object sender, RoutedEventArgs e)
        {
            editarCliente = true;
            Close();
        }

        private void cerrarClic(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void arrastrarVentana(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
