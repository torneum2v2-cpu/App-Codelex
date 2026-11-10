using Codelex.Clases_BDD;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Codelex
{
    class ItemLista
    {
        public int Id;
        public string Texto;
        public string Tipo;

        public ItemLista(int id, string texto, string tipo)
        {
            Id = id;
            Texto = texto;
            Tipo = tipo;
        }

        public override string ToString()
        {
            return Texto;
        }
    }

    public partial class IngresarOrden : Window
    {
        Orden ordenBD = new Orden();
        Cliente clienteBD = new Cliente();
        Vehiculo vehiculoBD = new Vehiculo();

        string clienteRtn = "";
        DataTable servicios = new DataTable();
        DataTable empleados = new DataTable();
        DataTable locacionesCliente = new DataTable();

        string cotizacionId = "";
        int personasNecesarias = 0;
        DataRow? cotizacionInicial = null;

        private IngresarOrden()
        {
            InitializeComponent();

            txtDescripcionLimpieza.MaxLength = 200;
            txtPiso_area.MaxLength = 20;
            txtDireccion_limpieza.IsReadOnly = true;
            cmbTipoLocacion.IsEnabled = false;

            cmbLocacion.SelectionChanged += cmbLocacion_SelectionChanged;
            Loaded += Window_Loaded;
        }
        public IngresarOrden(DataRow cotizacion) : this()
        {
            cotizacionInicial = cotizacion;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CargarListas();

                if (cotizacionInicial != null)
                    CargarCotizacion(cotizacionInicial);
                else
                    NuevaOrden();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void CargarListas()
        {
            LlenarCombo(cmbEstado, ordenBD.ListarEstadosOrden(), "estado_orden_id", "nombreEstadoOrden");
            servicios = ordenBD.ListarServicios();

            empleados = ordenBD.ListarEmpleados();
            LlenarCombo(cmbSupervisor, empleados, "empleado_id", "nombreCompleto");

            cmbTurno.Items.Clear();
            cmbTurno.Items.Add("Mañana");
            cmbTurno.Items.Add("Tarde");
            cmbTurno.Items.Add("Noche");

            CargarVehiculos();
        }

        private void LlenarCombo(ComboBox combo, DataTable tabla, string columnaId, string columnaTexto)
        {
            combo.Items.Clear();

            foreach (DataRow fila in tabla.Rows)
            {
                combo.Items.Add(new ItemLista(Convert.ToInt32(fila[columnaId]), "" + fila[columnaTexto], ""));
            }
        }

        private void CargarVehiculos()
        {
            cmbVehiculo.Items.Clear();

            DataTable tabla = vehiculoBD.BuscarVehiculos("", "ACTIVOS");

            foreach (DataRow fila in tabla.Rows)
            {
                string texto = fila["placa"] + " - " + fila["marcaVehiculo"] + " " + fila["modeloVehiculo"];
                cmbVehiculo.Items.Add(new ItemLista(Convert.ToInt32(fila["vehiculo_id"]), texto, ""));
            }
        }

        private void CargarEmpleadosDisponibles()
        {
            lstEmpleadosDisponibles.Items.Clear();
            lstEmpleadosAsignados.Items.Clear();

            foreach (DataRow fila in empleados.Rows)
            {
                lstEmpleadosDisponibles.Items.Add(new ItemLista(Convert.ToInt32(fila["empleado_id"]), "" + fila["nombreCompleto"], ""));
            }
        }

        private void CargarLocaciones(string rtn)
        {
            cmbLocacion.Items.Clear();

            locacionesCliente = clienteBD.LocacionesPorCliente(rtn);

            foreach (DataRow fila in locacionesCliente.Rows)
            {
                cmbLocacion.Items.Add(new ItemLista(Convert.ToInt32(fila["locacion_id"]), "" + fila["nombreLocacion"], ""));
            }

            if (locacionesCliente.Rows.Count == 0)
                Aviso("Este cliente no tiene locaciones registradas. Regístrelas desde Clientes.");
        }

        private int IdSeleccionado(ComboBox combo)
        {
            ItemLista item = combo.SelectedItem as ItemLista;

            if (item == null)
                return 0;

            return item.Id;
        }

        private void SeleccionarPorTexto(ComboBox combo, string texto)
        {
            foreach (ItemLista item in combo.Items)
            {
                if (item.Texto == texto)
                {
                    combo.SelectedItem = item;
                    return;
                }
            }

            if (combo.Items.Count > 0)
                combo.SelectedIndex = 0;
        }

        private void MoverSeleccionado(ListBox origen, ListBox destino)
        {
            ItemLista item = origen.SelectedItem as ItemLista;

            if (item == null)
                return;

            origen.Items.Remove(item);
            destino.Items.Add(item);
        }

        private void NuevaOrden()
        {
            txtN_orden.Text = "(automático)";
            dpFechaSolicitud.SelectedDate = DateTime.Today;
            dpFechaLimpieza.SelectedDate = null;

            SeleccionarPorTexto(cmbEstado, "Pendiente");
            cmbSupervisor.SelectedIndex = -1;
            cmbVehiculo.SelectedIndex = -1;
            cmbTurno.SelectedIndex = -1;

            lstActividadesOrden.Items.Clear();
            lstNecesidades.Items.Clear();
            txtServicio.Text = "";
            lblPersonasNecesarias.Text = "";
            personasNecesarias = 0;

            txtPiso_area.Text = "";
            txtDescripcionLimpieza.Text = "";

            cotizacionId = "";
            txtCotizacion.Text = "";
            txtTotalCotizacion.Text = "";
            lstCotizacion.Items.Clear();

            CargarEmpleadosDisponibles();
            LimpiarCliente();
        }

        private void CargarCotizacion(DataRow cotizacion)
        {
            NuevaOrden();

            cotizacionId = "" + cotizacion["cotizacion_id"];
            txtCotizacion.Text = cotizacionId;
            txtDescripcionLimpieza.Text = "" + cotizacion["observaciones"];
            txtTotalCotizacion.Text = "Total cotizado: L " + Convert.ToDecimal(cotizacion["precio_cotizado"]).ToString("N2");

            DataTable cliente = clienteBD.ObtenerCliente("" + cotizacion["identificacionCliente"]);
            if (cliente.Rows.Count > 0)
                LlenarCliente(cliente.Rows[0]);

            if (cotizacion["locacion_id"] != DBNull.Value)
                SeleccionarPorId(cmbLocacion, Convert.ToInt32(cotizacion["locacion_id"]));

            foreach (DataRow servicio in servicios.Rows)
            {
                if (Convert.ToInt32(servicio["servicio_id"]) == Convert.ToInt32(cotizacion["servicio_id"]))
                    txtServicio.Text = "" + servicio["nombreServicio"];
            }

            DataTable detalle = ordenBD.DetalleCotizacion(cotizacionId);
            foreach (DataRow linea in detalle.Rows)
            {
                string texto = linea["concepto"] + "   L " + Convert.ToDecimal(linea["subtotal"]).ToString("N2");
                lstCotizacion.Items.Add(texto);
                lstActividadesOrden.Items.Add(linea["concepto"]);
            }

            DataTable necesidades = ordenBD.ResumenCotizacion(cotizacionId);
            foreach (DataRow fila in necesidades.Rows)
            {
                lstNecesidades.Items.Add(ordenBD.TextoNecesidad(fila));

                if (("" + fila["tipo"]) == "Personas")
                    personasNecesarias = Convert.ToInt32(fila["cantidad"]);
            }

            lblPersonasNecesarias.Text = "La cotización necesita " + personasNecesarias + " empleados.";
        }

        private void SeleccionarPorId(ComboBox combo, int id)
        {
            foreach (ItemLista item in combo.Items)
            {
                if (item.Id == id)
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
        }

        private void LimpiarCliente()
        {
            clienteRtn = "";

            TxtIdentidad.Text = "";
            TxtEmpresa.Text = "";
            TxtContacto.Text = "";
            TxtApellido.Text = "";
            TxtTelefono.Text = "";
            TxtCorreo.Text = "";

            cmbLocacion.Items.Clear();
        }

        private void CambiarCotizacion_Click(object sender, RoutedEventArgs e)
        {
            DataRow? cotizacion = SeleccionarCotizacion.Elegir(this, "");

            if (cotizacion == null)
                return;

            try
            {
                CargarCotizacion(cotizacion);
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void LlenarCliente(DataRow fila)
        {
            clienteRtn = "" + fila["identificacionCliente"];

            TxtIdentidad.Text = clienteRtn;
            TxtEmpresa.Text = "" + fila["nombreEmpresa"];
            TxtContacto.Text = "" + fila["nombreCliente"];
            TxtApellido.Text = "" + fila["apellidoCliente"];
            TxtTelefono.Text = "" + fila["telefonoCliente"];
            TxtCorreo.Text = "" + fila["correoCliente"];

            try
            {
                CargarLocaciones(clienteRtn);
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void cmbLocacion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmbTipoLocacion.Items.Clear();
            txtDireccion_limpieza.Text = "";

            int locacionId = IdSeleccionado(cmbLocacion);
            if (locacionId == 0)
                return;

            foreach (DataRow fila in locacionesCliente.Rows)
            {
                if (Convert.ToInt32(fila["locacion_id"]) == locacionId)
                {
                    cmbTipoLocacion.Items.Add("" + fila["tipoLocacion"]);
                    cmbTipoLocacion.SelectedIndex = 0;
                    txtDireccion_limpieza.Text = "" + fila["direccion"];
                }
            }
        }

        private void btnAgregarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            MoverSeleccionado(lstEmpleadosDisponibles, lstEmpleadosAsignados);
        }

        private void btnQuitarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            MoverSeleccionado(lstEmpleadosAsignados, lstEmpleadosDisponibles);
        }

        private void btnGuardarOrden_Click(object sender, RoutedEventArgs e)
        {
            DatosOrden datos = new DatosOrden();

            datos.CotizacionId = cotizacionId;
            datos.Rtn = clienteRtn;
            datos.LocacionId = IdSeleccionado(cmbLocacion);
            datos.SupervisorId = IdSeleccionado(cmbSupervisor);
            datos.VehiculoId = IdSeleccionado(cmbVehiculo);
            datos.FechaSolicitud = dpFechaSolicitud.SelectedDate;
            datos.FechaServicio = dpFechaLimpieza.SelectedDate;
            datos.EstadoId = IdSeleccionado(cmbEstado);
            datos.Observaciones = txtDescripcionLimpieza.Text;
            datos.Turno = "" + cmbTurno.SelectedItem;
            datos.PisoArea = txtPiso_area.Text;
            datos.PersonasNecesarias = personasNecesarias;

            foreach (ItemLista item in lstEmpleadosAsignados.Items)
            {
                datos.Empleados.Add(item.Id);
            }

            try
            {
                string mensaje;
                string ordenId;

                bool guardada = ordenBD.GuardarOrden(datos, out mensaje, out ordenId);

                MensajeCodelex.Mostrar(this, "Órdenes", mensaje, guardada);

                if (guardada)
                    Close();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            if (clienteRtn != "")
            {
                MessageBoxResult respuesta = MessageBox.Show(
                    "¿Desea cancelar la orden? Se perderán los datos ingresados.",
                    "Órdenes", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (respuesta != MessageBoxResult.Yes)
                    return;
            }

            Close();
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
            MensajeCodelex.Mostrar(this, "Órdenes", mensaje, false);
        }

        private void MostrarError(Exception ex)
        {
            MensajeCodelex.Mostrar(this, "Órdenes", ex.Message, false);
        }
    }
}
