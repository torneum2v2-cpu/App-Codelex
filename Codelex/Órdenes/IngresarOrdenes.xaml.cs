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
        public string Nombre = "";
        public int Stock = 0;
        public int Cantidad = 0;


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
        DataTable actividades = new DataTable();
        DataTable empleados = new DataTable();
        DataTable locacionesCliente = new DataTable();

        string rtnInicial = "";

        public IngresarOrden()
        {
            InitializeComponent();

            txtDescripcionLimpieza.MaxLength = 200;
            txtPiso_area.MaxLength = 20;
            txtCantidad.MaxLength = 4;
            txtDireccion_limpieza.IsReadOnly = true;
            cmbTipoLocacion.IsEnabled = false;

            cmbLocacion.SelectionChanged += cmbLocacion_SelectionChanged;
            cmbTipo_Servicio.SelectionChanged += cmbTipo_Servicio_SelectionChanged;
            txtCantidad.PreviewTextInput += SoloNumeros;
            Loaded += Window_Loaded;
        }
        public IngresarOrden(string rtn) : this()
        {
            rtnInicial = rtn;
        }

        private void SeleccionarClienteInicial()
        {
            txtBuscarCliente.Text = rtnInicial;

            foreach (DataRowView fila in dgClientes.Items)
            {
                if ("" + fila["identificacionCliente"] == rtnInicial)
                {
                    dgClientes.SelectedItem = fila;
                    return;
                }
            }

            Aviso("El cliente está inactivo, no se pueden crear órdenes para él.");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CargarListas();
                NuevaOrden();

                if (rtnInicial != "")
                    SeleccionarClienteInicial();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void CargarListas()
        {
            LlenarCombo(cmbEstado, ordenBD.ListarEstadosOrden(), "estado_orden_id", "nombreEstadoOrden");
            LlenarCombo(cmbTipo_Servicio, ordenBD.ListarServicios(), "servicio_id", "nombreServicio");

            empleados = ordenBD.ListarEmpleados();
            LlenarCombo(cmbSupervisor, empleados, "empleado_id", "nombreCompleto");

            actividades = ordenBD.ListarActividades();

            cmbTurno.Items.Clear();
            cmbTurno.Items.Add("Mañana");
            cmbTurno.Items.Add("Tarde");
            cmbTurno.Items.Add("Noche");

            CargarVehiculos();
            CargarProductosEquipos();
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

        private void CargarProductosEquipos()
        {
            cmbProducto_equipo.Items.Clear();

            DataTable tabla = ordenBD.ListarProductosEquipos();

            foreach (DataRow fila in tabla.Rows)
            {
                string tipo = "" + fila["tipo"];
                string nombre = "" + fila["nombre"];
                int id = Convert.ToInt32(fila["id"]);

                ItemLista item;

                if (tipo == "P")
                {
                    int stock = Convert.ToInt32(fila["stock"]);
                    item = new ItemLista(id, "Producto: " + nombre + " (stock " + stock + ")", "P");
                    item.Stock = stock;
                }
                else
                {
                    string detalle = "" + fila["detalle"];
                    string texto = "Equipo: " + nombre;
                    if (detalle != "")
                        texto = texto + " (" + detalle + ")";

                    item = new ItemLista(id, texto, "E");
                }

                item.Nombre = nombre;
                cmbProducto_equipo.Items.Add(item);
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

        private void CargarClientes()
        {
            try
            {
                DataTable tabla = clienteBD.BuscarClientes(txtBuscarCliente.Text.Trim(), "", "ACTIVOS");
                dgClientes.ItemsSource = tabla.DefaultView;
            }
            catch (Exception ex)
            {
                MostrarError(ex);
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
            cmbProducto_equipo.SelectedIndex = -1;
            cmbTipo_Servicio.SelectedIndex = -1;

            lstActividadesOrden.Items.Clear();
            lstActividadesDisponibles.Items.Clear();
            lstProductosOrden.Items.Clear();

            txtPiso_area.Text = "";
            txtCantidad.Text = "";
            txtDescripcionLimpieza.Text = "";

            CargarEmpleadosDisponibles();
            LimpiarCliente();

            if (txtBuscarCliente.Text != "")
                txtBuscarCliente.Text = "";

            CargarClientes();
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
            dgClientes.SelectedItem = null;
        }

        private void SoloNumeros(object sender, TextCompositionEventArgs e)
        {
            if (!clienteBD.SoloNumeros(e.Text))
                e.Handled = true;
        }

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            CargarClientes();
        }

        private void dgClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgClientes.SelectedItem == null)
                return;

            DataRowView fila = (DataRowView)dgClientes.SelectedItem;

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

        private void btnLimpiarClientes(object sender, RoutedEventArgs e)
        {
            LimpiarCliente();
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

        private void cmbTipo_Servicio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lstActividadesOrden.Items.Clear();
            lstActividadesDisponibles.Items.Clear();

            int servicioId = IdSeleccionado(cmbTipo_Servicio);
            if (servicioId == 0)
                return;

            foreach (DataRow fila in actividades.Rows)
            {
                ItemLista item = new ItemLista(Convert.ToInt32(fila["actividad_id"]), "" + fila["texto"], "");

                if (Convert.ToInt32(fila["servicio_id"]) == servicioId)
                    lstActividadesOrden.Items.Add(item);
                else
                    lstActividadesDisponibles.Items.Add(item);
            }
        }

        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            MoverSeleccionado(lstActividadesDisponibles, lstActividadesOrden);
        }

        private void btnQuitar_Click(object sender, RoutedEventArgs e)
        {
            MoverSeleccionado(lstActividadesOrden, lstActividadesDisponibles);
        }

        private void btnAgregarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            MoverSeleccionado(lstEmpleadosDisponibles, lstEmpleadosAsignados);
        }

        private void btnQuitarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            MoverSeleccionado(lstEmpleadosAsignados, lstEmpleadosDisponibles);
        }

        private void btnAgregarProducto_Click(object sender, RoutedEventArgs e)
        {
            ItemLista elegido = cmbProducto_equipo.SelectedItem as ItemLista;

            if (elegido == null)
            {
                Aviso("Seleccione un producto o equipo.");
                return;
            }

            foreach (ItemLista item in lstProductosOrden.Items)
            {
                if (item.Tipo == elegido.Tipo && item.Id == elegido.Id)
                {
                    Aviso("Ya está en la lista. Quítelo para agregarlo con otra cantidad.");
                    return;
                }
            }

            int cantidad = 1;

            if (elegido.Tipo == "P")
            {
                string mensaje = ordenBD.ValidarCantidad(txtCantidad.Text, elegido.Stock, out cantidad);

                if (mensaje != "")
                {
                    Aviso(mensaje);
                    return;
                }
            }

            string texto;
            if (elegido.Tipo == "P")
                texto = elegido.Nombre + " x " + cantidad;
            else
                texto = elegido.Nombre + " (equipo)";

            ItemLista nuevo = new ItemLista(elegido.Id, texto, elegido.Tipo);
            nuevo.Nombre = elegido.Nombre;
            nuevo.Cantidad = cantidad;

            lstProductosOrden.Items.Add(nuevo);

            txtCantidad.Text = "";
            cmbProducto_equipo.SelectedIndex = -1;
        }

        private void btnQuitarProducto_Click(object sender, RoutedEventArgs e)
        {
            ItemLista item = lstProductosOrden.SelectedItem as ItemLista;

            if (item == null)
                return;

            lstProductosOrden.Items.Remove(item);
        }

        private void btnNuevaOrden_Click(object sender, RoutedEventArgs e)
        {
            NuevaOrden();
        }

        private void btnGuardarOrden_Click(object sender, RoutedEventArgs e)
        {
            DatosOrden datos = new DatosOrden();

            datos.Rtn = clienteRtn;
            datos.LocacionId = IdSeleccionado(cmbLocacion);
            datos.ServicioId = IdSeleccionado(cmbTipo_Servicio);
            datos.SupervisorId = IdSeleccionado(cmbSupervisor);
            datos.VehiculoId = IdSeleccionado(cmbVehiculo);
            datos.FechaSolicitud = dpFechaSolicitud.SelectedDate;
            datos.FechaServicio = dpFechaLimpieza.SelectedDate;
            datos.EstadoId = IdSeleccionado(cmbEstado);
            datos.Observaciones = txtDescripcionLimpieza.Text;
            datos.Turno = "" + cmbTurno.SelectedItem;
            datos.PisoArea = txtPiso_area.Text;

            foreach (ItemLista item in lstActividadesOrden.Items)
            {
                datos.Actividades.Add(item.Id);
            }

            foreach (ItemLista item in lstEmpleadosAsignados.Items)
            {
                datos.Empleados.Add(item.Id);
            }

            foreach (ItemLista item in lstProductosOrden.Items)
            {
                if (item.Tipo == "P")
                {
                    datos.ProductosId.Add(item.Id);
                    datos.ProductosCantidad.Add(item.Cantidad);
                }
                else
                {
                    datos.EquiposId.Add(item.Id);
                }
            }

            try
            {
                string mensaje;
                string ordenId;

                bool guardada = ordenBD.GuardarOrden(datos, out mensaje, out ordenId);

                MensajeCodelex.Mostrar(this, "Órdenes", mensaje, guardada);

                if (guardada)
                {
                    NuevaOrden();
                }
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
