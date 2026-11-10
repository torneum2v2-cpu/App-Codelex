using Codelex.AccesoDatos;
using global::Codelex.AccesoDatos;
using global::Codelex.FacturasCotizaciones;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Codelex.Ordenes
{

        // Elemento de los combos de filtro y de cambio de estado.
        class OpcionFiltro
        {
            public int id;
            public string texto;

            /// <summary>
            /// Crea el objeto con sus valores iniciales.
            /// </summary>
            /// <param name="id">Identificador.</param>
            /// <param name="texto">Texto a usar.</param>
            public OpcionFiltro(int id, string texto)
            {
                id = id;
                texto = texto;
            }

            /// <summary>
            /// Devuelve el texto que se muestra en las listas.
            /// </summary>
            /// <returns>Texto resultante.</returns>
            public override string ToString()
            {
                return texto;
            }
        }

        // Una fila de la tabla de órdenes. Las propiedades son las que usa el XAML.
        class FilaOrden
        {
            public string OrdenId { get; }
            public string Rtn { get; }
            public string Empresa { get; }
            public string Contacto { get; }
            public string Cliente { get; }
            public string Servicio { get; }
            public int SupervisorId { get; }
            public string Supervisor { get; }
            public DateTime? FechaSolicitud { get; }
            public DateTime? FechaServicio { get; }
            public int EstadoId { get; }
            public string Estado { get; }
            public Brush EstadoFondo { get; }
            public Brush EstadoTexto { get; }

            // Completada y Cancelada ya no pueden cambiar de estado.
            public bool EsFinal
            {
                get { return Estado == "Completada" || Estado == "Cancelada"; }
            }

            /// <summary>
            /// Crea el objeto con sus valores iniciales.
            /// </summary>
            /// <param name="fila">Fila con los datos.</param>
            public FilaOrden(DataRow fila)
            {
                OrdenId = "" + fila["orden_id"];
                Rtn = "" + fila["identificacion_cliente"];
                Empresa = "" + fila["nombre_empresa"];
                Contacto = "" + fila["contacto"];
                Servicio = "" + fila["servicio"];
                SupervisorId = Convert.ToInt32(fila["supervisor_id"]);
                Estado = "" + fila["estado"];
                EstadoId = Convert.ToInt32(fila["estado_orden_id"]);
                FechaSolicitud = fecha(fila["fecha_solicitud"]);
                FechaServicio = fecha(fila["fecha_servicio"]);

                string supervisor = ("" + fila["supervisor"]).Trim();
                Supervisor = supervisor == "" ? "—" : supervisor;

                if (Empresa.Trim() != "")
                    Cliente = Empresa;
                else if (Contacto.Trim() != "")
                    Cliente = Contacto;
                else
                    Cliente = Rtn;

                switch (Estado)
                {
                    case "Pendiente":
                        EstadoFondo = pincel("#FFF3D6");
                        EstadoTexto = pincel("#9A6B00");
                        break;
                    case "En proceso":
                        EstadoFondo = pincel("#E3F0FA");
                        EstadoTexto = pincel("#1F6FB2");
                        break;
                    case "Completada":
                        EstadoFondo = pincel("#E3F7F4");
                        EstadoTexto = pincel("#14837A");
                        break;
                    case "Cancelada":
                        EstadoFondo = pincel("#FBE6E6");
                        EstadoTexto = pincel("#B03A3A");
                        break;
                    default:
                        EstadoFondo = pincel("#EEF2F2");
                        EstadoTexto = pincel("#5B7C78");
                        break;
                }
            }

            /// <summary>
            /// Convierte un valor de fecha en texto dd/MM/yyyy, o un guion si no hay fecha.
            /// </summary>
            /// <param name="valor">Valor a usar.</param>
            /// <returns>Fecha como dd/MM/yyyy, o un guion si no hay fecha.</returns>
            private static DateTime? fecha(object valor)
            {
                if (valor is DateTime fecha)
                    return fecha;

                return null;
            }

            /// <summary>
            /// Crea un pincel a partir de un color en formato hexadecimal.
            /// </summary>
            /// <param name="color">Color en formato hexadecimal.</param>
            /// <returns>Pincel con el color pedido.</returns>
            private static Brush pincel(string color)
            {
                SolidColorBrush pincel = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
                pincel.Freeze();
                return pincel;
            }
        }

        public partial class GestionOrdenes : Window
        {
            GestionOrdenesBD gestionBD = new GestionOrdenesBD();
            Orden ordenBD = new Orden();

            List<FilaOrden> todas = new List<FilaOrden>();

            // Evita reaccionar a los eventos mientras se llenan los controles.
            bool listo = false;
            bool reconstruyendo = false;

            string detalleMostrado = "";
            // Factura de la orden mostrada en el detalle (0 = todavía no tiene).
            int facturaIdActual = 0;

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        public GestionOrdenes()
            {
                InitializeComponent();
                cmbCambiarEstado.SelectionChanged += cambiarEstadoSeleccionCambiada;
            }

            // ---------- Carga ----------

            /// <summary>
            /// Prepara la ventana y carga los datos iniciales cuando termina de abrirse.
            /// </summary>
            /// <param name="sender">Control que originó el evento.</param>
            /// <param name="e">Datos del evento.</param>
            private void ventanaCargada(object sender, RoutedEventArgs e)
            {
                try
                {
                    cargarEstados();
                    limpiarDetalle();

                    listo = true;
                    cargarOrdenes("", true);
                }
                catch (Exception ex)
                {
                    mostrarError(ex);
                }
            }

            /// <summary>
            /// Carga estados.
            /// </summary>
            private void cargarEstados()
            {
                DataTable estados = ordenBD.listarEstadosOrden();

                cmbFiltroEstado.Items.Clear();
                cmbFiltroEstado.Items.Add(new OpcionFiltro(0, "Todos"));
                cmbCambiarEstado.Items.Clear();

                foreach (DataRow fila in estados.Rows)
                {
                    int id = Convert.ToInt32(fila["estado_orden_id"]);
                    string nombre = "" + fila["nombre_estado_orden"];

                    cmbFiltroEstado.Items.Add(new OpcionFiltro(id, nombre));
                    cmbCambiarEstado.Items.Add(new OpcionFiltro(id, nombre));
                }

                cmbFiltroEstado.SelectedIndex = 0;
            }

            // Lee las órdenes de la base y vuelve a aplicar los filtros.
            // forzarDetalle = true vuelve a leer el detalle de la orden seleccionada.
            /// <summary>
            /// Carga órdenes.
            /// </summary>
            /// <param name="idSeleccionar">Id seleccionar.</param>
            /// <param name="forzarDetalle">Forzar detalle.</param>
            private void cargarOrdenes(string idSeleccionar, bool forzarDetalle)
            {
                try
                {
                    DataTable tabla = gestionBD.listarOrdenes();

                    todas = new List<FilaOrden>();
                    foreach (DataRow fila in tabla.Rows)
                    {
                        todas.Add(new FilaOrden(fila));
                    }

                    actualizarResumen();
                    reconstruirFiltroSupervisor();
                    aplicarFiltros(idSeleccionar, forzarDetalle);
                }
                catch (Exception ex)
                {
                    mostrarError(ex);
                }
            }

            /// <summary>
            /// Actualiza resumen.
            /// </summary>
            private void actualizarResumen()
            {
                txtTotal.Text = todas.Count.ToString();
                txtPendientes.Text = todas.Count(f => f.Estado == "Pendiente").ToString();
                txtEnProceso.Text = todas.Count(f => f.Estado == "En proceso").ToString();
                txtCompletadas.Text = todas.Count(f => f.Estado == "Completada").ToString();
                txtCanceladas.Text = todas.Count(f => f.Estado == "Cancelada").ToString();
            }

            // El filtro de supervisor solo ofrece los supervisores que tienen órdenes.
            /// <summary>
            /// Reconstruye filtro supervisor.
            /// </summary>
            private void reconstruirFiltroSupervisor()
            {
                int previo = 0;
                if (cmbFiltroSupervisor.SelectedItem is OpcionFiltro actual)
                    previo = actual.id;

                bool estabaListo = listo;
                listo = false;

                cmbFiltroSupervisor.Items.Clear();
                cmbFiltroSupervisor.Items.Add(new OpcionFiltro(0, "Todos"));

                List<FilaOrden> supervisores = todas
                    .Where(f => f.SupervisorId > 0)
                    .GroupBy(f => f.SupervisorId)
                    .Select(g => g.First())
                    .OrderBy(f => f.Supervisor)
                    .ToList();

                foreach (FilaOrden f in supervisores)
                {
                    cmbFiltroSupervisor.Items.Add(new OpcionFiltro(f.SupervisorId, f.Supervisor));
                }

                cmbFiltroSupervisor.SelectedIndex = 0;
                foreach (OpcionFiltro item in cmbFiltroSupervisor.Items)
                {
                    if (item.id == previo)
                    {
                        cmbFiltroSupervisor.SelectedItem = item;
                        break;
                    }
                }

                listo = estabaListo;
            }

            // ---------- Filtros ----------

            /// <summary>
            /// Indica si un texto contiene a otro sin distinguir mayúsculas.
            /// </summary>
            /// <param name="origen">Texto donde se busca.</param>
            /// <param name="texto">Texto a usar.</param>
            /// <returns>true si el texto contiene al otro; false en caso contrario.</returns>
            private static bool contiene(string origen, string texto)
            {
                // Sin importar mayúsculas ni tildes.
                return CultureInfo.InvariantCulture.CompareInfo.IndexOf(
                    origen, texto, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;
            }

            /// <summary>
            /// Aplica filtros.
            /// </summary>
            /// <param name="idSeleccionar">Id seleccionar.</param>
            /// <param name="forzarDetalle">Forzar detalle.</param>
            private void aplicarFiltros(string idSeleccionar, bool forzarDetalle)
            {
                if (!listo)
                    return;

                string texto = txtBuscar.Text.Trim();
                int estadoId = cmbFiltroEstado.SelectedItem is OpcionFiltro estado ? estado.id : 0;
                int supervisorId = cmbFiltroSupervisor.SelectedItem is OpcionFiltro supervisor ? supervisor.id : 0;
                DateTime? desde = dpDesde.SelectedDate;
                DateTime? hasta = dpHasta.SelectedDate;

                List<FilaOrden> lista = new List<FilaOrden>();
                string aviso = "";

                if (desde.HasValue && hasta.HasValue && desde.Value.Date > hasta.Value.Date)
                {
                    aviso = "La fecha \"desde\" no puede ser posterior a la fecha \"hasta\".";
                }
                else
                {
                    foreach (FilaOrden f in todas)
                    {
                        if (estadoId > 0 && f.EstadoId != estadoId)
                            continue;
                        if (supervisorId > 0 && f.SupervisorId != supervisorId)
                            continue;

                        if (desde.HasValue || hasta.HasValue)
                        {
                            if (!f.FechaServicio.HasValue)
                                continue;
                            if (desde.HasValue && f.FechaServicio.Value.Date < desde.Value.Date)
                                continue;
                            if (hasta.HasValue && f.FechaServicio.Value.Date > hasta.Value.Date)
                                continue;
                        }

                        if (texto != "" &&
                            !contiene(f.OrdenId, texto) && !contiene(f.Rtn, texto) &&
                            !contiene(f.Empresa, texto) && !contiene(f.Contacto, texto))
                            continue;

                        lista.Add(f);
                    }
                }

                if (idSeleccionar == "" && dgOrdenes.SelectedItem is FilaOrden actual)
                    idSeleccionar = actual.OrdenId;

                reconstruyendo = true;
                dgOrdenes.ItemsSource = lista;
                FilaOrden? seleccionada = lista.FirstOrDefault(f => f.OrdenId == idSeleccionar);
                dgOrdenes.SelectedItem = seleccionada;
                reconstruyendo = false;

                if (aviso != "")
                    txtConteo.Text = aviso;
                else
                    txtConteo.Text = "Mostrando " + lista.Count + " de " + todas.Count + " órdenes";

                if (seleccionada == null)
                {
                    limpiarDetalle();
                }
                else
                {
                    dgOrdenes.ScrollIntoView(seleccionada);

                    if (forzarDetalle || seleccionada.OrdenId != detalleMostrado)
                        mostrarDetalle(seleccionada);
                    else
                        actualizarAcciones(seleccionada);
                }
            }

            /// <summary>
            /// Atiende el cambio de texto de buscar.
            /// </summary>
            /// <param name="sender">Control que originó el evento.</param>
            /// <param name="e">Datos del evento.</param>
            private void buscarTextoCambiado(object sender, TextChangedEventArgs e)
            {
                aplicarFiltros("", false);
            }

            /// <summary>
            /// Atiende el cambio de filtro.
            /// </summary>
            /// <param name="sender">Control que originó el evento.</param>
            /// <param name="e">Datos del evento.</param>
            private void filtroCambiado(object sender, SelectionChangedEventArgs e)
            {
                aplicarFiltros("", false);
            }

            /// <summary>
            /// Atiende el clic de limpiar filtros.
            /// </summary>
            /// <param name="sender">Control que originó el evento.</param>
            /// <param name="e">Datos del evento.</param>
            private void limpiarFiltrosClic(object sender, RoutedEventArgs e)
            {
                listo = false;
                txtBuscar.Text = "";
                cmbFiltroEstado.SelectedIndex = 0;
                cmbFiltroSupervisor.SelectedIndex = 0;
                dpDesde.SelectedDate = null;
                dpHasta.SelectedDate = null;
                listo = true;

                aplicarFiltros("", false);
                txtBuscar.Focus();
            }

            // ---------- Detalle ----------

            /// <summary>
            /// Atiende el cambio de selección de órdenes.
            /// </summary>
            /// <param name="sender">Control que originó el evento.</param>
            /// <param name="e">Datos del evento.</param>
            private void ordenesSeleccionCambiada(object sender, SelectionChangedEventArgs e)
            {
                if (reconstruyendo)
                    return;

                if (dgOrdenes.SelectedItem is FilaOrden fila)
                    mostrarDetalle(fila);
                else
                    limpiarDetalle();
            }

            /// <summary>
            /// Limpia detalle.
            /// </summary>
            private void limpiarDetalle()
            {
                detalleMostrado = "";
                facturaIdActual = 0;
                panelDetalle.Visibility = Visibility.Collapsed;
                pillEstado.Visibility = Visibility.Collapsed;
                txtSinSeleccion.Visibility = Visibility.Visible;

                actualizarAcciones(null);
            }

            /// <summary>
            /// Muestra detalle.
            /// </summary>
            /// <param name="fila">Fila con los datos.</param>
            private void mostrarDetalle(FilaOrden fila)
            {
                try
                {
                    DataTable cabecera = gestionBD.obtenerDetalle(fila.OrdenId);

                    if (cabecera.Rows.Count == 0)
                    {
                        MensajeCodelex.mostrar(this, "Gestión de órdenes", "La orden ya no existe. Se actualizará la lista.", false);
                        cargarOrdenes("", true);
                        return;
                    }

                    DataRow d = cabecera.Rows[0];

                    dOrden.Text = fila.OrdenId;
                    dCliente.Text = textoCliente(d);
                    dLugar.Text = textoLugar(d);

                    string vehiculo = ("" + d["vehiculo"]).Trim();
                    dVehiculo.Text = vehiculo == "" ? "Sin vehículo asignado" : vehiculo;

                    string observaciones = ("" + d["observacion"]).Trim();
                    dObservaciones.Text = observaciones == "" ? "Sin observaciones" : observaciones;

                    dConformidad.Text = textoConformidad(d);
                    dFactura.Text = textoFactura(d);

                    llenarListas(fila.OrdenId);

                    txtPillEstado.Text = fila.Estado;
                    txtPillEstado.Foreground = fila.EstadoTexto;
                    pillEstado.Background = fila.EstadoFondo;
                    pillEstado.Visibility = Visibility.Visible;

                    txtSinSeleccion.Visibility = Visibility.Collapsed;
                    panelDetalle.Visibility = Visibility.Visible;

                    detalleMostrado = fila.OrdenId;
                    facturaIdActual = d["factura_id"] is DBNull ? 0 : Convert.ToInt32(d["factura_id"]);
                    actualizarAcciones(fila);
            }
                catch (Exception ex)
                {
                    mostrarError(ex);
                }
            }

            /// <summary>
            /// Llena listas.
            /// </summary>
            /// <param name="ordenId">Identificador de orden.</param>
            private void llenarListas(string ordenId)
            {
                DataTable tabla = gestionBD.listarDetalle(ordenId);

                List<string> actividades = new List<string>();
                List<string> empleados = new List<string>();
                List<string> productos = new List<string>();

                foreach (DataRow fila in tabla.Rows)
                {
                    int seccion = Convert.ToInt32(fila["seccion"]);
                    string texto = "" + fila["texto"];

                    if (seccion == 1)
                        actividades.Add(texto);
                    else if (seccion == 2)
                        empleados.Add(texto);
                    else
                        productos.Add(texto);   // productos (3) y equipos (4)
                }

                if (actividades.Count == 0)
                    actividades.Add("Sin actividades registradas");
                if (empleados.Count == 0)
                    empleados.Add("Sin empleados asignados");
                if (productos.Count == 0)
                    productos.Add("Sin productos ni equipos");

                lstActividades.ItemsSource = actividades;
                lstEmpleados.ItemsSource = empleados;
                lstProductos.ItemsSource = productos;
            }

            /// <summary>
            /// Une lineas.
            /// </summary>
            /// <param name="lineas">Lineas.</param>
            /// <returns>Texto resultante.</returns>
            private static string unirLineas(List<string> lineas)
            {
                return string.Join(Environment.NewLine, lineas.Where(l => l.Trim() != ""));
            }

            /// <summary>
            /// Arma el texto del cliente que se muestra en el detalle de la orden.
            /// </summary>
            /// <param name="d">Fila con los datos de la orden.</param>
            /// <returns>Texto resultante.</returns>
            private static string textoCliente(DataRow d)
            {
                string empresa = ("" + d["nombre_empresa"]).Trim();
                string contacto = ("" + d["contacto"]).Trim();
                string rtn = ("" + d["identificacion_cliente"]).Trim();

                List<string> lineas = new List<string>();

                if (empresa != "")
                {
                    lineas.Add(empresa);
                    if (contacto != "")
                        lineas.Add("Contacto: " + contacto);
                }
                else if (contacto != "")
                {
                    lineas.Add(contacto);
                }

                if (rtn != "")
                    lineas.Add("RTN: " + rtn);

                return unirLineas(lineas);
            }

            /// <summary>
            /// Arma el texto del lugar del servicio que se muestra en el detalle de la orden.
            /// </summary>
            /// <param name="d">Fila con los datos de la orden.</param>
            /// <returns>Texto resultante.</returns>
            private static string textoLugar(DataRow d)
            {
                string locacion = ("" + d["locacion"]).Trim();
                string tipo = ("" + d["tipo_locacion"]).Trim();
                string direccion = ("" + d["direccion"]).Trim();
                string piso = ("" + d["piso_area"]).Trim();
                string turno = ("" + d["turno"]).Trim();

                List<string> lineas = new List<string>();

                if (locacion != "")
                    lineas.Add(tipo == "" ? locacion : locacion + " (" + tipo + ")");
                if (direccion != "")
                    lineas.Add(direccion);
                if (piso != "")
                    lineas.Add("Piso o área: " + piso);
                if (turno != "")
                    lineas.Add("Turno: " + turno);

                return lineas.Count == 0 ? "Sin locación" : unirLineas(lineas);
            }

            // Igual que el reporte de órdenes: vacío = sin evaluar, 1 = conforme, 0 = no conforme.
            /// <summary>
            /// Arma el texto de la conformidad del cliente que se muestra en el detalle de la orden.
            /// </summary>
            /// <param name="d">Fila con los datos de la orden.</param>
            /// <returns>Texto resultante.</returns>
            private static string textoConformidad(DataRow d)
            {
                if ("" + d["estado"] != "Completada")
                    return "Se evalúa cuando la orden esté completada";

                if (d["conformidad_cliente"] is bool conforme)
                    return conforme ? "Conforme" : "No conforme";

                return "Sin evaluar";
            }

            /// <summary>
            /// Arma el texto de la factura que se muestra en el detalle de la orden.
            /// </summary>
            /// <param name="d">Fila con los datos de la orden.</param>
            /// <returns>Texto resultante.</returns>
            private static string textoFactura(DataRow d)
            {
                if (d["factura_id"] is DBNull)
                    return "Sin factura";

                List<string> lineas = new List<string>();
                lineas.Add("Factura N.° " + Facturacion.numeroFactura(Convert.ToInt32(d["factura_id"])) + " - " + d["estado_pago"]);

                if (d["precio_facturado"] is not DBNull)
                    lineas.Add("Monto: L " + Convert.ToDecimal(d["precio_facturado"]).ToString("N2", CultureInfo.InvariantCulture));

                if (d["fecha_factura"] is DateTime fecha)
                    lineas.Add("Fecha: " + fecha.ToString("dd/MM/yyyy"));

                return unirLineas(lineas);
            }

            // ---------- Cambio de estado ----------

            /// <summary>
            /// Actualiza acciones.
            /// </summary>
            /// <param name="fila">Fila con los datos.</param>
            private void actualizarAcciones(FilaOrden? fila)
            {
                if (fila == null)
                {
                    cmbCambiarEstado.SelectedIndex = -1;
                    cmbCambiarEstado.IsEnabled = false;
                    cmbCambiarEstado.ToolTip = null;
                    btnAplicarEstado.IsEnabled = false;
                    btnCancelarOrden.IsEnabled = false;
                    btnCrearFactura.Visibility = Visibility.Collapsed;
                    btnVerFactura.Visibility = Visibility.Collapsed;
                    return;
                }

                // Crear factura: orden completada sin factura. Ver factura: orden completada con factura.
                bool completada = fila.Estado == "Completada";
                btnCrearFactura.Visibility = completada && facturaIdActual == 0 ? Visibility.Visible : Visibility.Collapsed;
                btnVerFactura.Visibility = completada && facturaIdActual > 0 ? Visibility.Visible : Visibility.Collapsed;

                cmbCambiarEstado.IsEnabled = !fila.EsFinal;
                cmbCambiarEstado.ToolTip = fila.EsFinal
                    ? "Una orden completada o cancelada ya no puede cambiar de estado."
                    : null;
                cmbCambiarEstado.SelectedItem = cmbCambiarEstado.Items
                    .OfType<OpcionFiltro>()
                    .FirstOrDefault(o => o.id == fila.EstadoId);

                btnCancelarOrden.IsEnabled = !fila.EsFinal;
                actualizarBotonAplicar();
            }

            /// <summary>
            /// Actualiza boton aplicar.
            /// </summary>
            private void actualizarBotonAplicar()
            {
                FilaOrden? fila = dgOrdenes.SelectedItem as FilaOrden;
                OpcionFiltro? nuevo = cmbCambiarEstado.SelectedItem as OpcionFiltro;

                btnAplicarEstado.IsEnabled = fila != null && !fila.EsFinal
                                             && nuevo != null && nuevo.id != fila.EstadoId;
            }

            /// <summary>
            /// Atiende el cambio de selección de cambiar estado.
            /// </summary>
            /// <param name="sender">Control que originó el evento.</param>
            /// <param name="e">Datos del evento.</param>
            private void cambiarEstadoSeleccionCambiada(object sender, SelectionChangedEventArgs e)
            {
                actualizarBotonAplicar();
            }

            /// <summary>
            /// Atiende el clic de aplicar estado.
            /// </summary>
            /// <param name="sender">Control que originó el evento.</param>
            /// <param name="e">Datos del evento.</param>
            private void aplicarEstadoClic(object sender, RoutedEventArgs e)
            {
                if (dgOrdenes.SelectedItem is not FilaOrden fila || cmbCambiarEstado.SelectedItem is not OpcionFiltro nuevo)
                    return;

                if (nuevo.id == fila.EstadoId)
                    return;

                string pregunta = "¿Cambiar la orden " + fila.OrdenId + " de \"" + fila.Estado + "\" a \"" + nuevo.texto + "\"?";

                if (nuevo.texto == "Cancelada")
                    pregunta += "\n\nLa orden se conserva, pero ya no podrá cambiar de estado.";
                else if (nuevo.texto == "Completada")
                    pregunta += "\n\nLos equipos asignados quedarán libres y la orden ya no podrá cambiar de estado.";

                if (MessageBox.Show(pregunta, "Gestión de órdenes", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;

                cambiarEstado(fila, nuevo.id);
            }

            /// <summary>
            /// Atiende el clic de cancelar orden.
            /// </summary>
            /// <param name="sender">Control que originó el evento.</param>
            /// <param name="e">Datos del evento.</param>
            private void cancelarOrdenClic(object sender, RoutedEventArgs e)
            {
                if (dgOrdenes.SelectedItem is not FilaOrden fila)
                    return;

                int canceladaId = 0;
                foreach (OpcionFiltro item in cmbCambiarEstado.Items)
                {
                    if (item.texto == "Cancelada")
                        canceladaId = item.id;
                }

                if (canceladaId == 0)
                {
                    MensajeCodelex.mostrar(this, "Gestión de órdenes", "No existe el estado \"Cancelada\" en la base de datos.", false);
                    return;
                }

                string pregunta = "¿Desea cancelar la orden " + fila.OrdenId + "?\n\n" +
                                  "La orden no se borra: queda con estado Cancelada y ya no podrá cambiar de estado.";

                if (MessageBox.Show(pregunta, "Gestión de órdenes", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;

                cambiarEstado(fila, canceladaId);
            }

            /// <summary>
            /// Cambia estado.
            /// </summary>
            /// <param name="fila">Fila con los datos.</param>
            /// <param name="estadoId">Identificador del estado.</param>
            private void cambiarEstado(FilaOrden fila, int estadoId)
            {
                try
                {
                    string mensaje;
                    bool guardado = gestionBD.cambiarEstado(fila.OrdenId, estadoId, out mensaje);

                    MensajeCodelex.mostrar(this, "Gestión de órdenes", mensaje, guardado);

                    // Se recarga también si falló: casi siempre es porque la lista estaba desactualizada.
                    cargarOrdenes(fila.OrdenId, true);
                }
                catch (Exception ex)
                {
                    mostrarError(ex);
                }
            }

        // ---------- Botones ----------

            /// <summary>
            /// Atiende el clic de crear factura.
            /// </summary>
            /// <param name="sender">Control que originó el evento.</param>
            /// <param name="e">Datos del evento.</param>
            private void crearFacturaClic(object sender, RoutedEventArgs e)
            {
                if (dgOrdenes.SelectedItem is not FilaOrden fila)
                    return;

                Factura ventana = new Factura(fila.OrdenId);
                ventana.Owner = this;
                ventana.ShowDialog();

                // Al volver se relee la orden: si ya se facturó, el botón cambia a "Ver factura".
                cargarOrdenes(fila.OrdenId, true);
            }

            /// <summary>
            /// Atiende el clic de ver factura.
            /// </summary>
            /// <param name="sender">Control que originó el evento.</param>
            /// <param name="e">Datos del evento.</param>
            private void verFacturaClic(object sender, RoutedEventArgs e)
            {
                if (facturaIdActual <= 0)
                    return;

                FacturaDetalle ventana = new FacturaDetalle(facturaIdActual);
                ventana.Owner = this;

                if (ventana.ShowDialog() == true && dgOrdenes.SelectedItem is FilaOrden fila)
                    cargarOrdenes(fila.OrdenId, true);
            }

            /// <summary>
            /// Valida y guarda los cambios del registro seleccionado.
            /// </summary>
            /// <param name="sender">Control que originó el evento.</param>
            /// <param name="e">Datos del evento.</param>
            private void actualizarClic(object sender, RoutedEventArgs e)
                {
                    cargarOrdenes("", true);
                }

            /// <summary>
            /// Atiende el clic de nueva orden.
            /// </summary>
            /// <param name="sender">Control que originó el evento.</param>
            /// <param name="e">Datos del evento.</param>
            private void nuevaOrdenClic(object sender, RoutedEventArgs e)
            {
                DataRow? cotizacion = SeleccionarCotizacion.elegir(this, "");
                if (cotizacion == null)
                    return;

                IngresarOrden ventana = new IngresarOrden(cotizacion);
                ventana.Owner = this;
                ventana.ShowDialog();

                cargarOrdenes("", false);
            }

            // ---------- Ventana ----------

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

            /// <summary>
            /// Minimiza la ventana.
            /// </summary>
            /// <param name="sender">Control que originó el evento.</param>
            /// <param name="e">Datos del evento.</param>
            private void minimizarClic(object sender, RoutedEventArgs e)
            {
                WindowState = WindowState.Minimized;
            }

            /// <summary>
            /// Cierra la ventana.
            /// </summary>
            /// <param name="sender">Control que originó el evento.</param>
            /// <param name="e">Datos del evento.</param>
            private void salirClic(object sender, RoutedEventArgs e)
            {
                Close();
            }

            /// <summary>
            /// Muestra un mensaje con el error ocurrido.
            /// </summary>
            /// <param name="ex">Excepción que se produjo.</param>
            private void mostrarError(Exception ex)
            {
                MensajeCodelex.mostrar(this, "Gestión de órdenes", ex.Message, false);
            }
        }
}
