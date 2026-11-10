using Codelex.AccesoDatos;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Codelex
{
    class ItemLista
    {
        public int id;
        public string texto;
        public string tipo;

        public ItemLista(int id, string texto, string tipo)
        {
            id = id;
            texto = texto;
            tipo = tipo;
        }

        public override string ToString()
        {
            return texto;
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
            txtPisoArea.MaxLength = 20;
            txtDireccionLimpieza.IsReadOnly = true;
            cmbTipoLocacion.IsEnabled = false;

            cmbLocacion.SelectionChanged += locacionSeleccionCambiada;
            Loaded += ventanaCargada;
        }
        public IngresarOrden(DataRow cotizacion) : this()
        {
            cotizacionInicial = cotizacion;
        }

        private void ventanaCargada(object sender, RoutedEventArgs e)
        {
            try
            {
                cargarListas();

                if (cotizacionInicial != null)
                    cargarCotizacion(cotizacionInicial);
                else
                    nuevaOrden();
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void cargarListas()
        {
            llenarCombo(cmbEstado, ordenBD.listarEstadosOrden(), "estado_orden_id", "nombre_estado_orden");
            servicios = ordenBD.listarServicios();

            empleados = ordenBD.listarEmpleados();
            llenarCombo(cmbSupervisor, empleados, "empleado_id", "nombre_completo");

            cmbTurno.Items.Clear();
            cmbTurno.Items.Add("Mañana");
            cmbTurno.Items.Add("Tarde");
            cmbTurno.Items.Add("Noche");

            cargarVehiculos();
        }

        private void llenarCombo(ComboBox combo, DataTable tabla, string columnaId, string columnaTexto)
        {
            combo.Items.Clear();

            foreach (DataRow fila in tabla.Rows)
            {
                combo.Items.Add(new ItemLista(Convert.ToInt32(fila[columnaId]), "" + fila[columnaTexto], ""));
            }
        }

        private void cargarVehiculos()
        {
            cmbVehiculo.Items.Clear();

            DataTable tabla = vehiculoBD.buscarVehiculos("", "ACTIVOS");

            foreach (DataRow fila in tabla.Rows)
            {
                string texto = fila["placa"] + " - " + fila["marca_vehiculo"] + " " + fila["modelo_vehiculo"];
                cmbVehiculo.Items.Add(new ItemLista(Convert.ToInt32(fila["vehiculo_id"]), texto, ""));
            }
        }

        private void cargarEmpleadosDisponibles()
        {
            lstEmpleadosDisponibles.Items.Clear();
            lstEmpleadosAsignados.Items.Clear();

            foreach (DataRow fila in empleados.Rows)
            {
                lstEmpleadosDisponibles.Items.Add(new ItemLista(Convert.ToInt32(fila["empleado_id"]), "" + fila["nombre_completo"], ""));
            }
        }

        private void cargarLocaciones(string rtn)
        {
            cmbLocacion.Items.Clear();

            locacionesCliente = clienteBD.locacionesPorCliente(rtn);

            foreach (DataRow fila in locacionesCliente.Rows)
            {
                cmbLocacion.Items.Add(new ItemLista(Convert.ToInt32(fila["locacion_id"]), "" + fila["nombre_locacion"], ""));
            }

            if (locacionesCliente.Rows.Count == 0)
                aviso("Este cliente no tiene locaciones registradas. Regístrelas desde Clientes.");
        }

        private int idSeleccionado(ComboBox combo)
        {
            ItemLista item = combo.SelectedItem as ItemLista;

            if (item == null)
                return 0;

            return item.id;
        }

        private void seleccionarPorTexto(ComboBox combo, string texto)
        {
            foreach (ItemLista item in combo.Items)
            {
                if (item.texto == texto)
                {
                    combo.SelectedItem = item;
                    return;
                }
            }

            if (combo.Items.Count > 0)
                combo.SelectedIndex = 0;
        }

        private void moverSeleccionado(ListBox origen, ListBox destino)
        {
            ItemLista item = origen.SelectedItem as ItemLista;

            if (item == null)
                return;

            origen.Items.Remove(item);
            destino.Items.Add(item);
        }

        private void nuevaOrden()
        {
            txtNOrden.Text = "(automático)";
            dpFechaSolicitud.SelectedDate = DateTime.Today;
            dpFechaLimpieza.SelectedDate = null;

            seleccionarPorTexto(cmbEstado, "Pendiente");
            cmbSupervisor.SelectedIndex = -1;
            cmbVehiculo.SelectedIndex = -1;
            cmbTurno.SelectedIndex = -1;

            lstActividadesOrden.Items.Clear();
            lstNecesidades.Items.Clear();
            txtServicio.Text = "";
            lblPersonasNecesarias.Text = "";
            personasNecesarias = 0;

            txtPisoArea.Text = "";
            txtDescripcionLimpieza.Text = "";

            cotizacionId = "";
            txtCotizacion.Text = "";
            txtTotalCotizacion.Text = "";
            lstCotizacion.Items.Clear();

            cargarEmpleadosDisponibles();
            limpiarCliente();
            irAPaso(1);
        }

        private void cargarCotizacion(DataRow cotizacion)
        {
            nuevaOrden();

            cotizacionId = "" + cotizacion["cotizacion_id"];
            txtCotizacion.Text = cotizacionId;
            txtDescripcionLimpieza.Text = "" + cotizacion["observacion"];
            txtTotalCotizacion.Text = "Total cotizado: L " + Convert.ToDecimal(cotizacion["precio_cotizado"]).ToString("N2");

            DataTable cliente = clienteBD.obtenerCliente("" + cotizacion["identificacion_cliente"]);
            if (cliente.Rows.Count > 0)
                llenarCliente(cliente.Rows[0]);

            if (cotizacion["locacion_id"] != DBNull.Value)
                seleccionarPorId(cmbLocacion, Convert.ToInt32(cotizacion["locacion_id"]));

            foreach (DataRow servicio in servicios.Rows)
            {
                if (Convert.ToInt32(servicio["servicio_id"]) == Convert.ToInt32(cotizacion["servicio_id"]))
                    txtServicio.Text = "" + servicio["nombre_servicio"];
            }

            DataTable detalle = ordenBD.detalleCotizacion(cotizacionId);
            foreach (DataRow linea in detalle.Rows)
            {
                string texto = linea["concepto"] + "   L " + Convert.ToDecimal(linea["subtotal"]).ToString("N2");
                lstCotizacion.Items.Add(texto);
                lstActividadesOrden.Items.Add(linea["concepto"]);
            }

            DataTable necesidades = ordenBD.resumenCotizacion(cotizacionId);
            foreach (DataRow fila in necesidades.Rows)
            {
                lstNecesidades.Items.Add(ordenBD.textoNecesidad(fila));

                if (("" + fila["tipo"]) == "Personas")
                    personasNecesarias = Convert.ToInt32(fila["cantidad"]);
            }

            lblPersonasNecesarias.Text = "La cotización necesita " + personasNecesarias + " empleados.";
        }

        private void seleccionarPorId(ComboBox combo, int id)
        {
            foreach (ItemLista item in combo.Items)
            {
                if (item.id == id)
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
        }

        private void limpiarCliente()
        {
            clienteRtn = "";

            txtIdentidad.Text = "";
            txtEmpresa.Text = "";
            txtContacto.Text = "";
            txtApellido.Text = "";
            txtTelefono.Text = "";
            txtCorreo.Text = "";

            cmbLocacion.Items.Clear();
        }

        private void cambiarCotizacionClic(object sender, RoutedEventArgs e)
        {
            DataRow? cotizacion = SeleccionarCotizacion.elegir(this, "");

            if (cotizacion == null)
                return;

            try
            {
                cargarCotizacion(cotizacion);
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void llenarCliente(DataRow fila)
        {
            clienteRtn = "" + fila["identificacion_cliente"];

            txtIdentidad.Text = clienteRtn;
            txtEmpresa.Text = "" + fila["nombre_empresa"];
            txtContacto.Text = "" + fila["nombre_cliente"];
            txtApellido.Text = "" + fila["apellido_cliente"];
            txtTelefono.Text = "" + fila["telefono_cliente"];
            txtCorreo.Text = "" + fila["correo_cliente"];

            try
            {
                cargarLocaciones(clienteRtn);
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void locacionSeleccionCambiada(object sender, SelectionChangedEventArgs e)
        {
            cmbTipoLocacion.Items.Clear();
            txtDireccionLimpieza.Text = "";

            int locacionId = idSeleccionado(cmbLocacion);
            if (locacionId == 0)
                return;

            foreach (DataRow fila in locacionesCliente.Rows)
            {
                if (Convert.ToInt32(fila["locacion_id"]) == locacionId)
                {
                    cmbTipoLocacion.Items.Add("" + fila["tipo_locacion"]);
                    cmbTipoLocacion.SelectedIndex = 0;
                    txtDireccionLimpieza.Text = "" + fila["direccion"];
                }
            }
        }

        private void agregarEmpleadoClic(object sender, RoutedEventArgs e)
        {
            moverSeleccionado(lstEmpleadosDisponibles, lstEmpleadosAsignados);
        }

        private void quitarEmpleadoClic(object sender, RoutedEventArgs e)
        {
            moverSeleccionado(lstEmpleadosAsignados, lstEmpleadosDisponibles);
        }

        private void guardarOrdenClic(object sender, RoutedEventArgs e)
        {
            for (int paso = 1; paso <= 3; paso++)
            {
                string problema = validarPaso(paso);

                if (problema != "")
                {
                    irAPaso(paso);
                    mostrarErrorPaso(problema);
                    return;
                }
            }

            DatosOrden datos = new DatosOrden();

            datos.cotizacionId = cotizacionId;
            datos.rtn = clienteRtn;
            datos.locacionId = idSeleccionado(cmbLocacion);
            datos.supervisorId = idSeleccionado(cmbSupervisor);
            datos.vehiculoId = idSeleccionado(cmbVehiculo);
            datos.fechaSolicitud = dpFechaSolicitud.SelectedDate;
            datos.fechaServicio = dpFechaLimpieza.SelectedDate;
            datos.estadoId = idSeleccionado(cmbEstado);
            datos.observaciones = txtDescripcionLimpieza.Text;
            datos.turno = "" + cmbTurno.SelectedItem;
            datos.pisoArea = txtPisoArea.Text;
            datos.personasNecesarias = personasNecesarias;

            foreach (ItemLista item in lstEmpleadosAsignados.Items)
            {
                datos.empleados.Add(item.id);
            }

            try
            {
                string mensaje;
                string ordenId;

                bool guardada = ordenBD.guardarOrden(datos, out mensaje, out ordenId);

                MensajeCodelex.mostrar(this, "Órdenes", mensaje, guardada);

                if (guardada)
                    Close();
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void cancelarClic(object sender, RoutedEventArgs e)
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

        // ---------- Pasos ----------

        int pasoActual = 1;

        private void irAPaso(int paso)
        {
            pasoActual = paso;

            panelPaso1.Visibility = paso == 1 ? Visibility.Visible : Visibility.Collapsed;
            panelPaso2.Visibility = paso == 2 ? Visibility.Visible : Visibility.Collapsed;
            panelPaso3.Visibility = paso == 3 ? Visibility.Visible : Visibility.Collapsed;

            btnAtras.Visibility = paso > 1 ? Visibility.Visible : Visibility.Collapsed;
            btnSiguiente.Visibility = paso < 3 ? Visibility.Visible : Visibility.Collapsed;
            btnGuardarOrden.Visibility = paso == 3 ? Visibility.Visible : Visibility.Collapsed;

            txtErrorPaso.Visibility = Visibility.Collapsed;

            pintarIndicador(1, circulo1, numero1, titulo1);
            pintarIndicador(2, circulo2, numero2, titulo2);
            pintarIndicador(3, circulo3, numero3, titulo3);

            if (paso == 3)
                armarResumen();

            scrollPasos.ScrollToTop();
        }

        private void pintarIndicador(int numero, Border circulo, TextBlock texto, TextBlock titulo)
        {
            if (numero < pasoActual)
            {
                circulo.Background = (Brush)FindResource("PillText");
                texto.Text = "✓";
                texto.Foreground = Brushes.White;
                titulo.Foreground = (Brush)FindResource("DarkText");
                titulo.FontWeight = FontWeights.SemiBold;
            }
            else if (numero == pasoActual)
            {
                circulo.Background = (Brush)FindResource("Primary");
                texto.Text = numero.ToString();
                texto.Foreground = Brushes.White;
                titulo.Foreground = (Brush)FindResource("DarkText");
                titulo.FontWeight = FontWeights.Bold;
            }
            else
            {
                circulo.Background = (Brush)FindResource("CardLine");
                texto.Text = numero.ToString();
                texto.Foreground = (Brush)FindResource("SoftText");
                titulo.Foreground = (Brush)FindResource("SoftText");
                titulo.FontWeight = FontWeights.SemiBold;
            }
        }

        // Avanza hasta el paso pedido validando cada paso intermedio. Si uno falla, se queda en él.
        private void avanzarHasta(int destino)
        {
            for (int paso = pasoActual; paso < destino; paso++)
            {
                string problema = validarPaso(paso);

                if (problema != "")
                {
                    irAPaso(paso);
                    mostrarErrorPaso(problema);
                    return;
                }
            }

            irAPaso(destino);
        }

        private string validarPaso(int paso)
        {
            if (paso == 1)
            {
                if (cotizacionId == "")
                    return "Seleccione una cotización.";
                if (clienteRtn == "")
                    return "La cotización no tiene un cliente registrado.";
            }
            else if (paso == 2)
            {
                if (lstActividadesOrden.Items.Count == 0)
                    return "La orden necesita al menos una actividad.";
                if (dpFechaSolicitud.SelectedDate == null)
                    return "Seleccione la fecha de solicitud.";
                if (dpFechaLimpieza.SelectedDate == null)
                    return "Seleccione la fecha de limpieza.";
                if (dpFechaLimpieza.SelectedDate < dpFechaSolicitud.SelectedDate)
                    return "La fecha de limpieza no puede ser anterior a la fecha de solicitud.";
                if (idSeleccionado(cmbEstado) == 0)
                    return "Seleccione el estado de la orden.";
                if (idSeleccionado(cmbSupervisor) == 0)
                    return "Seleccione el supervisor.";
                if (idSeleccionado(cmbLocacion) == 0)
                    return "Seleccione la locación.";
            }
            else if (paso == 3)
            {
                if (lstEmpleadosAsignados.Items.Count == 0)
                    return "Asigne al menos un empleado.";
                if (lstEmpleadosAsignados.Items.Count < personasNecesarias)
                    return "La cotización necesita " + personasNecesarias + " empleados. Asigne más.";
                if (cmbTurno.SelectedItem == null)
                    return "Seleccione el turno.";
                if (txtPisoArea.Text.Trim() == "")
                    return "Escriba el piso o área.";
                if (idSeleccionado(cmbVehiculo) == 0)
                    return "Seleccione el vehículo.";
            }

            return "";
        }

        private void mostrarErrorPaso(string mensaje)
        {
            txtErrorPaso.Text = mensaje;
            txtErrorPaso.Visibility = Visibility.Visible;
        }

        private void armarResumen()
        {
            string fecha = dpFechaLimpieza.SelectedDate == null
                ? ""
                : dpFechaLimpieza.SelectedDate.Value.ToString("dd/MM/yyyy");

            txtResumen.Text =
                "Cotización: " + txtCotizacion.Text + "  |  " + txtTotalCotizacion.Text + "\n" +
                "Cliente: " + txtEmpresa.Text + " (" + txtContacto.Text + " " + txtApellido.Text + ")\n" +
                "Servicio: " + txtServicio.Text + " (" + lstActividadesOrden.Items.Count + " actividades)\n" +
                "Lugar: " + cmbLocacion.SelectedItem + " - " + txtDireccionLimpieza.Text + "\n" +
                "Fecha de limpieza: " + fecha + "\n" +
                "Supervisor: " + cmbSupervisor.SelectedItem;
        }

        private void pasoClic(object sender, MouseButtonEventArgs e)
        {
            int destino = int.Parse("" + ((FrameworkElement)sender).Tag);

            if (destino < pasoActual)
                irAPaso(destino);
            else if (destino > pasoActual)
                avanzarHasta(destino);
        }

        private void siguienteClic(object sender, RoutedEventArgs e)
        {
            avanzarHasta(pasoActual + 1);
        }

        private void atrasClic(object sender, RoutedEventArgs e)
        {
            if (pasoActual > 1)
                irAPaso(pasoActual - 1);
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
            MensajeCodelex.mostrar(this, "Órdenes", mensaje, false);
        }

        private void mostrarError(Exception ex)
        {
            MensajeCodelex.mostrar(this, "Órdenes", ex.Message, false);
        }
    }
}