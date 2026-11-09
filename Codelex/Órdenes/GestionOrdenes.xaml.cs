using Codelex.Clases_BDD;
using global::Codelex.Clases_BDD;
using global::Codelex.Facturas_y_Cotizaciones;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Codelex.Órdenes
{

        // Elemento de los combos de filtro y de cambio de estado.
        class OpcionFiltro
        {
            public int Id;
            public string Texto;

            public OpcionFiltro(int id, string texto)
            {
                Id = id;
                Texto = texto;
            }

            public override string ToString()
            {
                return Texto;
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

            public FilaOrden(DataRow fila)
            {
                OrdenId = "" + fila["orden_id"];
                Rtn = "" + fila["identificacionCliente"];
                Empresa = "" + fila["nombreEmpresa"];
                Contacto = "" + fila["contacto"];
                Servicio = "" + fila["servicio"];
                SupervisorId = Convert.ToInt32(fila["supervisor_id"]);
                Estado = "" + fila["estado"];
                EstadoId = Convert.ToInt32(fila["estado_orden_id"]);
                FechaSolicitud = Fecha(fila["fecha_solicitud"]);
                FechaServicio = Fecha(fila["fecha_servicio"]);

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
                        EstadoFondo = Pincel("#FFF3D6");
                        EstadoTexto = Pincel("#9A6B00");
                        break;
                    case "En proceso":
                        EstadoFondo = Pincel("#E3F0FA");
                        EstadoTexto = Pincel("#1F6FB2");
                        break;
                    case "Completada":
                        EstadoFondo = Pincel("#E3F7F4");
                        EstadoTexto = Pincel("#14837A");
                        break;
                    case "Cancelada":
                        EstadoFondo = Pincel("#FBE6E6");
                        EstadoTexto = Pincel("#B03A3A");
                        break;
                    default:
                        EstadoFondo = Pincel("#EEF2F2");
                        EstadoTexto = Pincel("#5B7C78");
                        break;
                }
            }

            private static DateTime? Fecha(object valor)
            {
                if (valor is DateTime fecha)
                    return fecha;

                return null;
            }

            private static Brush Pincel(string color)
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

        public GestionOrdenes()
            {
                InitializeComponent();
                cmbCambiarEstado.SelectionChanged += cmbCambiarEstado_SelectionChanged;
            }

            // ---------- Carga ----------

            private void Window_Loaded(object sender, RoutedEventArgs e)
            {
                try
                {
                    CargarEstados();
                    LimpiarDetalle();

                    listo = true;
                    CargarOrdenes("", true);
                }
                catch (Exception ex)
                {
                    MostrarError(ex);
                }
            }

            private void CargarEstados()
            {
                DataTable estados = ordenBD.ListarEstadosOrden();

                cmbFiltroEstado.Items.Clear();
                cmbFiltroEstado.Items.Add(new OpcionFiltro(0, "Todos"));
                cmbCambiarEstado.Items.Clear();

                foreach (DataRow fila in estados.Rows)
                {
                    int id = Convert.ToInt32(fila["estado_orden_id"]);
                    string nombre = "" + fila["nombreEstadoOrden"];

                    cmbFiltroEstado.Items.Add(new OpcionFiltro(id, nombre));
                    cmbCambiarEstado.Items.Add(new OpcionFiltro(id, nombre));
                }

                cmbFiltroEstado.SelectedIndex = 0;
            }

            // Lee las órdenes de la base y vuelve a aplicar los filtros.
            // forzarDetalle = true vuelve a leer el detalle de la orden seleccionada.
            private void CargarOrdenes(string idSeleccionar, bool forzarDetalle)
            {
                try
                {
                    DataTable tabla = gestionBD.ListarOrdenes();

                    todas = new List<FilaOrden>();
                    foreach (DataRow fila in tabla.Rows)
                    {
                        todas.Add(new FilaOrden(fila));
                    }

                    ActualizarResumen();
                    ReconstruirFiltroSupervisor();
                    AplicarFiltros(idSeleccionar, forzarDetalle);
                }
                catch (Exception ex)
                {
                    MostrarError(ex);
                }
            }

            private void ActualizarResumen()
            {
                txtTotal.Text = todas.Count.ToString();
                txtPendientes.Text = todas.Count(f => f.Estado == "Pendiente").ToString();
                txtEnProceso.Text = todas.Count(f => f.Estado == "En proceso").ToString();
                txtCompletadas.Text = todas.Count(f => f.Estado == "Completada").ToString();
                txtCanceladas.Text = todas.Count(f => f.Estado == "Cancelada").ToString();
            }

            // El filtro de supervisor solo ofrece los supervisores que tienen órdenes.
            private void ReconstruirFiltroSupervisor()
            {
                int previo = 0;
                if (cmbFiltroSupervisor.SelectedItem is OpcionFiltro actual)
                    previo = actual.Id;

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
                    if (item.Id == previo)
                    {
                        cmbFiltroSupervisor.SelectedItem = item;
                        break;
                    }
                }

                listo = estabaListo;
            }

            // ---------- Filtros ----------

            private static bool Contiene(string origen, string texto)
            {
                // Sin importar mayúsculas ni tildes.
                return CultureInfo.InvariantCulture.CompareInfo.IndexOf(
                    origen, texto, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;
            }

            private void AplicarFiltros(string idSeleccionar, bool forzarDetalle)
            {
                if (!listo)
                    return;

                string texto = txtBuscar.Text.Trim();
                int estadoId = cmbFiltroEstado.SelectedItem is OpcionFiltro estado ? estado.Id : 0;
                int supervisorId = cmbFiltroSupervisor.SelectedItem is OpcionFiltro supervisor ? supervisor.Id : 0;
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
                            !Contiene(f.OrdenId, texto) && !Contiene(f.Rtn, texto) &&
                            !Contiene(f.Empresa, texto) && !Contiene(f.Contacto, texto))
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
                    LimpiarDetalle();
                }
                else
                {
                    dgOrdenes.ScrollIntoView(seleccionada);

                    if (forzarDetalle || seleccionada.OrdenId != detalleMostrado)
                        MostrarDetalle(seleccionada);
                    else
                        ActualizarAcciones(seleccionada);
                }
            }

            private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
            {
                AplicarFiltros("", false);
            }

            private void Filtro_Changed(object sender, SelectionChangedEventArgs e)
            {
                AplicarFiltros("", false);
            }

            private void btnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
            {
                listo = false;
                txtBuscar.Text = "";
                cmbFiltroEstado.SelectedIndex = 0;
                cmbFiltroSupervisor.SelectedIndex = 0;
                dpDesde.SelectedDate = null;
                dpHasta.SelectedDate = null;
                listo = true;

                AplicarFiltros("", false);
                txtBuscar.Focus();
            }

            // ---------- Detalle ----------

            private void dgOrdenes_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (reconstruyendo)
                    return;

                if (dgOrdenes.SelectedItem is FilaOrden fila)
                    MostrarDetalle(fila);
                else
                    LimpiarDetalle();
            }

            private void LimpiarDetalle()
            {
                detalleMostrado = "";
                facturaIdActual = 0;
                panelDetalle.Visibility = Visibility.Collapsed;
                pillEstado.Visibility = Visibility.Collapsed;
                txtSinSeleccion.Visibility = Visibility.Visible;

                ActualizarAcciones(null);
            }

            private void MostrarDetalle(FilaOrden fila)
            {
                try
                {
                    DataTable cabecera = gestionBD.ObtenerDetalle(fila.OrdenId);

                    if (cabecera.Rows.Count == 0)
                    {
                        MensajeCodelex.Mostrar(this, "Gestión de órdenes", "La orden ya no existe. Se actualizará la lista.", false);
                        CargarOrdenes("", true);
                        return;
                    }

                    DataRow d = cabecera.Rows[0];

                    dOrden.Text = fila.OrdenId;
                    dCliente.Text = TextoCliente(d);
                    dLugar.Text = TextoLugar(d);

                    string vehiculo = ("" + d["vehiculo"]).Trim();
                    dVehiculo.Text = vehiculo == "" ? "Sin vehículo asignado" : vehiculo;

                    string observaciones = ("" + d["observaciones"]).Trim();
                    dObservaciones.Text = observaciones == "" ? "Sin observaciones" : observaciones;

                    dConformidad.Text = TextoConformidad(d);
                    dFactura.Text = TextoFactura(d);

                    LlenarListas(fila.OrdenId);

                    txtPillEstado.Text = fila.Estado;
                    txtPillEstado.Foreground = fila.EstadoTexto;
                    pillEstado.Background = fila.EstadoFondo;
                    pillEstado.Visibility = Visibility.Visible;

                    txtSinSeleccion.Visibility = Visibility.Collapsed;
                    panelDetalle.Visibility = Visibility.Visible;

                    detalleMostrado = fila.OrdenId;
                    facturaIdActual = d["factura_id"] is DBNull ? 0 : Convert.ToInt32(d["factura_id"]);
                    ActualizarAcciones(fila);
            }
                catch (Exception ex)
                {
                    MostrarError(ex);
                }
            }

            private void LlenarListas(string ordenId)
            {
                DataTable tabla = gestionBD.ListarDetalle(ordenId);

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

            private static string UnirLineas(List<string> lineas)
            {
                return string.Join(Environment.NewLine, lineas.Where(l => l.Trim() != ""));
            }

            private static string TextoCliente(DataRow d)
            {
                string empresa = ("" + d["nombreEmpresa"]).Trim();
                string contacto = ("" + d["contacto"]).Trim();
                string rtn = ("" + d["identificacionCliente"]).Trim();

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

                return UnirLineas(lineas);
            }

            private static string TextoLugar(DataRow d)
            {
                string locacion = ("" + d["locacion"]).Trim();
                string tipo = ("" + d["tipoLocacion"]).Trim();
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

                return lineas.Count == 0 ? "Sin locación" : UnirLineas(lineas);
            }

            // Igual que el reporte de órdenes: vacío = sin evaluar, 1 = conforme, 0 = no conforme.
            private static string TextoConformidad(DataRow d)
            {
                if ("" + d["estado"] != "Completada")
                    return "Se evalúa cuando la orden esté completada";

                if (d["conformidad_cliente"] is bool conforme)
                    return conforme ? "Conforme" : "No conforme";

                return "Sin evaluar";
            }

            private static string TextoFactura(DataRow d)
            {
                if (d["factura_id"] is DBNull)
                    return "Sin factura";

                List<string> lineas = new List<string>();
                lineas.Add("Factura N.° " + d["factura_id"] + " - " + d["estado_pago"]);

                if (d["precio_facturado"] is not DBNull)
                    lineas.Add("Monto: L " + Convert.ToDecimal(d["precio_facturado"]).ToString("N2", CultureInfo.InvariantCulture));

                if (d["fechaFactura"] is DateTime fecha)
                    lineas.Add("Fecha: " + fecha.ToString("dd/MM/yyyy"));

                return UnirLineas(lineas);
            }

            // ---------- Cambio de estado ----------

            private void ActualizarAcciones(FilaOrden? fila)
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
                    .FirstOrDefault(o => o.Id == fila.EstadoId);

                btnCancelarOrden.IsEnabled = !fila.EsFinal;
                ActualizarBotonAplicar();
            }

            private void ActualizarBotonAplicar()
            {
                FilaOrden? fila = dgOrdenes.SelectedItem as FilaOrden;
                OpcionFiltro? nuevo = cmbCambiarEstado.SelectedItem as OpcionFiltro;

                btnAplicarEstado.IsEnabled = fila != null && !fila.EsFinal
                                             && nuevo != null && nuevo.Id != fila.EstadoId;
            }

            private void cmbCambiarEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                ActualizarBotonAplicar();
            }

            private void btnAplicarEstado_Click(object sender, RoutedEventArgs e)
            {
                if (dgOrdenes.SelectedItem is not FilaOrden fila || cmbCambiarEstado.SelectedItem is not OpcionFiltro nuevo)
                    return;

                if (nuevo.Id == fila.EstadoId)
                    return;

                string pregunta = "¿Cambiar la orden " + fila.OrdenId + " de \"" + fila.Estado + "\" a \"" + nuevo.Texto + "\"?";

                if (nuevo.Texto == "Cancelada")
                    pregunta += "\n\nLa orden se conserva, pero ya no podrá cambiar de estado.";
                else if (nuevo.Texto == "Completada")
                    pregunta += "\n\nLos equipos asignados quedarán libres y la orden ya no podrá cambiar de estado.";

                if (MessageBox.Show(pregunta, "Gestión de órdenes", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;

                CambiarEstado(fila, nuevo.Id);
            }

            private void btnCancelarOrden_Click(object sender, RoutedEventArgs e)
            {
                if (dgOrdenes.SelectedItem is not FilaOrden fila)
                    return;

                int canceladaId = 0;
                foreach (OpcionFiltro item in cmbCambiarEstado.Items)
                {
                    if (item.Texto == "Cancelada")
                        canceladaId = item.Id;
                }

                if (canceladaId == 0)
                {
                    MensajeCodelex.Mostrar(this, "Gestión de órdenes", "No existe el estado \"Cancelada\" en la base de datos.", false);
                    return;
                }

                string pregunta = "¿Desea cancelar la orden " + fila.OrdenId + "?\n\n" +
                                  "La orden no se borra: queda con estado Cancelada y ya no podrá cambiar de estado.";

                if (MessageBox.Show(pregunta, "Gestión de órdenes", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;

                CambiarEstado(fila, canceladaId);
            }

            private void CambiarEstado(FilaOrden fila, int estadoId)
            {
                try
                {
                    string mensaje;
                    bool guardado = gestionBD.CambiarEstado(fila.OrdenId, estadoId, out mensaje);

                    MensajeCodelex.Mostrar(this, "Gestión de órdenes", mensaje, guardado);

                    // Se recarga también si falló: casi siempre es porque la lista estaba desactualizada.
                    CargarOrdenes(fila.OrdenId, true);
                }
                catch (Exception ex)
                {
                    MostrarError(ex);
                }
            }

        // ---------- Botones ----------

            private void btnCrearFactura_Click(object sender, RoutedEventArgs e)
            {
                if (dgOrdenes.SelectedItem is not FilaOrden fila)
                    return;

                Factura ventana = new Factura(fila.OrdenId);
                ventana.Owner = this;
                ventana.ShowDialog();

                // Al volver se relee la orden: si ya se facturó, el botón cambia a "Ver factura".
                CargarOrdenes(fila.OrdenId, true);
            }

            private void btnVerFactura_Click(object sender, RoutedEventArgs e)
            {
                if (facturaIdActual <= 0)
                    return;

                FacturaDetalle ventana = new FacturaDetalle(facturaIdActual);
                ventana.Owner = this;
                ventana.ShowDialog();
            }   

            private void btnActualizar_Click(object sender, RoutedEventArgs e)
                {
                    CargarOrdenes("", true);
                }

            private void btnNuevaOrden_Click(object sender, RoutedEventArgs e)
            {
                DataRow? cotizacion = SeleccionarCotizacion.Elegir(this, "");
                if (cotizacion == null)
                    return;

                IngresarOrden ventana = new IngresarOrden(cotizacion);
                ventana.Owner = this;
                ventana.ShowDialog();

                CargarOrdenes("", false);
            }

            // ---------- Ventana ----------

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

            private void MostrarError(Exception ex)
            {
                MensajeCodelex.Mostrar(this, "Gestión de órdenes", ex.Message, false);
            }
        }
}
