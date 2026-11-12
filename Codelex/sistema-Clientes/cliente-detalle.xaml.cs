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

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        /// <param name="rtn">RTN o identidad del cliente (sin guiones).</param>
        public ClienteDetalle(string rtn)
        {
            InitializeComponent();
            this.rtn = rtn;
        }

        /// <summary>
        /// Prepara la ventana y carga los datos iniciales cuando termina de abrirse.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
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
                MensajeCodelex.mostrar(this, "Detalle del cliente", ex.Message, false);
            }
        }

        /// <summary>
        /// Carga datos cliente.
        /// </summary>
        private void cargarDatosCliente()
        {
            DataTable tabla = clienteBD.obtenerCliente(rtn);

            if (tabla.Rows.Count == 0)
            {
                MensajeCodelex.mostrar(this, "Detalle del cliente", "No se encontró el cliente.", false);
                return;
            }

            DataRow fila = tabla.Rows[0];

            lblTipoId.Text = fila["tipo_identificacion"].ToString();
            txtRTN.Text = fila["identificacion_cliente"].ToString();
            txtEmpresa.Text = fila["nombre_empresa"].ToString();
            txtContacto.Text = fila["nombre_cliente"].ToString();
            txtApellido.Text = fila["apellido_cliente"].ToString();
            txtTelefono.Text = Validacion.formatoTelefono("" + fila["telefono_cliente"]);
            txtCorreo.Text = fila["correo_cliente"].ToString();
            txtEstado.Text = fila["estado"].ToString();
        }

        /// <summary>
        /// Carga locaciones.
        /// </summary>
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

        /// <summary>
        /// Carga órdenes.
        /// </summary>
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

        /// <summary>
        /// Carga facturas.
        /// </summary>
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

        /// <summary>
        /// Convierte un valor de fecha en texto dd/MM/yyyy, o un guion si no hay fecha.
        /// </summary>
        /// <param name="valor">Valor a usar.</param>
        /// <returns>Fecha como dd/MM/yyyy, o un guion si no hay fecha.</returns>
        private string fecha(object valor)
        {
            if (valor == DBNull.Value)
                return "";

            return Convert.ToDateTime(valor).ToString("dd/MM/yyyy");
        }

        /// <summary>
        /// Atiende el clic de nueva orden.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void nuevaOrdenClic(object sender, RoutedEventArgs e)
        {
            abrirOrden();
        }

        /// <summary>
        /// Atiende el clic de agregar orden.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void agregarOrdenClic(object sender, RoutedEventArgs e)
        {
            abrirOrden();
        }

        /// <summary>
        /// Abre orden.
        /// </summary>
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

        /// <summary>
        /// Atiende el clic de agregar locación.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
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

        /// <summary>
        /// Atiende el clic de agregar factura.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void agregarFacturaClic(object sender, RoutedEventArgs e)
        {
            MensajeCodelex.mostrar(this, "Facturas", "Próximamente", true);
        }

        /// <summary>
        /// Muestra un mensaje con el error ocurrido.
        /// </summary>
        /// <param name="ex">Excepción que se produjo.</param>
        private void mostrarError(Exception ex)
        {
            MensajeCodelex.mostrar(this, "Detalle del cliente", ex.Message, false);
        }

        /// <summary>
        /// Atiende el clic de editar.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void editarClic(object sender, RoutedEventArgs e)
        {
            editarCliente = true;
            Close();
        }

        /// <summary>
        /// Cierra la ventana.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void cerrarClic(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Permite mover la ventana arrastrando la barra superior con el mouse.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void arrastrarVentana(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
