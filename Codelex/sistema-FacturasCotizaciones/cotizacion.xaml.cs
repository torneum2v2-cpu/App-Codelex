using Codelex.AccesoDatos;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Codelex.FacturasCotizaciones
{
    /// <summary>
    /// Lógica de interacción para cotizacion.xaml
    /// </summary>
    public partial class Cotizacion : Window
    {
        Cliente clienteBD = new Cliente();
        Facturacion facturaBD = new Facturacion();

        DataTable locacionesCliente = new DataTable();
        DataTable actividades = new DataTable();
        ObservableCollection<LineaActividad> lineas = new ObservableCollection<LineaActividad>();

        decimal subtotalCotizado = 0;
        int personasNecesarias = 0;

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        public Cotizacion()
        {
            InitializeComponent();

            cmbTipoLocacion.IsEnabled = false;

            txtIdentidad.MaxLength = 14;
            txtEmpresa.MaxLength = 50;
            txtContacto.MaxLength = 50;
            txtApellido.MaxLength = 50;
            txtTelefono.MaxLength = 50;
            txtCorreo.MaxLength = 50;
            txtDireccion.MaxLength = 80;
            txtObservaciones.MaxLength = 200;

            dgLineas.ItemsSource = lineas;

            cmbLocacion.SelectionChanged += locacionSeleccionCambiada;
            cmbServicio.SelectionChanged += servicioSeleccionCambiada;
            Loaded += ventanaCargada;
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
                cargarServicios();
                actividades = facturaBD.listarActividades();
                nuevaCotizacion();
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        // ---------- Carga de datos ----------

        /// <summary>
        /// Carga servicios.
        /// </summary>
        private void cargarServicios()
        {
            cmbServicio.Items.Clear();

            DataTable servicios = facturaBD.listarServicios();
            foreach (DataRow fila in servicios.Rows)
            {
                cmbServicio.Items.Add(new ItemLista(Convert.ToInt32(fila["servicio_id"]), "" + fila["nombre_servicio"], ""));
            }
        }

        /// <summary>
        /// Carga locaciones.
        /// </summary>
        /// <param name="rtn">RTN o identidad del cliente (sin guiones).</param>
        private void cargarLocaciones(string rtn)
        {
            cmbLocacion.Items.Clear();

            locacionesCliente = clienteBD.locacionesPorCliente(rtn);

            foreach (DataRow fila in locacionesCliente.Rows)
            {
                cmbLocacion.Items.Add(new ItemLista(Convert.ToInt32(fila["locacion_id"]), "" + fila["nombre_locacion"], ""));
            }

            if (locacionesCliente.Rows.Count == 0)
                aviso("Este cliente no tiene locaciones registradas. Escriba la dirección del servicio.");
        }

        /// <summary>
        /// Obtiene el identificador del elemento elegido en la lista desplegable.
        /// </summary>
        /// <param name="combo">Lista desplegable.</param>
        /// <returns>Identificador elegido, o 0 si no hay selección.</returns>
        private int idSeleccionado(ComboBox combo)
        {
            ItemLista item = combo.SelectedItem as ItemLista;

            if (item == null)
                return 0;

            return item.id;
        }

        // ---------- Cotización nueva ----------

        /// <summary>
        /// Deja el formulario listo para una cotización nueva.
        /// </summary>
        private void nuevaCotizacion()
        {
            txtNDocumento.Text = "(automático)";
            dpFecha.SelectedDate = DateTime.Today;
            dpValidaHasta.SelectedDate = DateTime.Today.AddDays(15);

            cmbServicio.SelectedIndex = -1;
            lineas.Clear();
            actualizarOtrasActividades();
            txtObservaciones.Text = "";
            txtDescuento.Text = "0";

            limpiarCliente();
            recalcularTotales();
        }

        /// <summary>
        /// Limpia cliente.
        /// </summary>
        private void limpiarCliente()
        {
            txtIdentidad.Text = "";
            txtEmpresa.Text = "";
            txtContacto.Text = "";
            txtApellido.Text = "";
            txtTelefono.Text = "";
            txtCorreo.Text = "";
            txtDireccion.Text = "";

            cmbLocacion.Items.Clear();
        }

        // ---------- Eventos de cliente ----------

        /// <summary>
        /// Atiende el clic de buscar cliente.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void buscarClienteClic(object sender, RoutedEventArgs e)
        {
            ElegirCliente ventana = new ElegirCliente();
            ventana.Owner = this;

            if (ventana.ShowDialog() == true && ventana.clienteElegido != null)
                llenarCliente(ventana.clienteElegido.Row);
        }

        /// <summary>
        /// Llena cliente.
        /// </summary>
        /// <param name="fila">Fila con los datos.</param>
        private void llenarCliente(DataRow fila)
        {
            string rtn = "" + fila["identificacion_cliente"];

            txtIdentidad.Text = rtn;
            txtEmpresa.Text = "" + fila["nombre_empresa"];
            txtContacto.Text = "" + fila["nombre_cliente"];
            txtApellido.Text = "" + fila["apellido_cliente"];
            txtTelefono.Text = "" + fila["telefono_cliente"];
            txtCorreo.Text = "" + fila["correo_cliente"];

            try
            {
                cargarLocaciones(rtn);
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        /// <summary>
        /// Borra los datos del cliente del formulario.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void limpiarClientes(object sender, RoutedEventArgs e)
        {
            limpiarCliente();
        }

        /// <summary>
        /// Atiende el cambio de selección de locación.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void locacionSeleccionCambiada(object sender, SelectionChangedEventArgs e)
        {
            cmbTipoLocacion.Items.Clear();

            int locacionId = idSeleccionado(cmbLocacion);
            if (locacionId == 0)
                return;

            foreach (DataRow fila in locacionesCliente.Rows)
            {
                if (Convert.ToInt32(fila["locacion_id"]) == locacionId)
                {
                    cmbTipoLocacion.Items.Add("" + fila["tipo_locacion"]);
                    cmbTipoLocacion.SelectedIndex = 0;
                    txtDireccion.Text = "" + fila["direccion"];
                }
            }
        }

        // ---------- Actividades ----------

        /// <summary>
        /// Atiende el cambio de selección de servicio.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void servicioSeleccionCambiada(object sender, SelectionChangedEventArgs e)
        {
            int servicioId = idSeleccionado(cmbServicio);

            if (servicioId == 0)
                return;

            lineas.Clear();

            foreach (DataRow fila in actividades.Rows)
            {
                if (Convert.ToInt32(fila["servicio_id"]) == servicioId)
                    lineas.Add(nuevaLinea(fila));
            }

            actualizarOtrasActividades();
            recalcularTotales();
        }

        /// <summary>
        /// Crea una línea de actividad a partir de una fila de la base de datos.
        /// </summary>
        /// <param name="fila">Fila con los datos.</param>
        /// <returns>Resultado de tipo LineaActividad.</returns>
        private LineaActividad nuevaLinea(DataRow fila)
        {
            LineaActividad linea = new LineaActividad();
            linea.ActividadId = Convert.ToInt32(fila["actividad_id"]);
            linea.Nombre = "" + fila["nombre"];
            linea.Unidad = "" + fila["unidad"];
            return linea;
        }

        /// <summary>
        /// Indica si ya esta en la lista.
        /// </summary>
        /// <param name="actividadId">Identificador de actividad.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        private bool yaEstaEnLaLista(int actividadId)
        {
            foreach (LineaActividad linea in lineas)
            {
                if (linea.ActividadId == actividadId)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Actualiza otras actividades.
        /// </summary>
        private void actualizarOtrasActividades()
        {
            cmbOtraActividad.Items.Clear();

            foreach (DataRow fila in actividades.Rows)
            {
                int id = Convert.ToInt32(fila["actividad_id"]);

                if (!yaEstaEnLaLista(id))
                    cmbOtraActividad.Items.Add(new ItemLista(id, "" + fila["nombre"] + " (" + fila["unidad"] + ")", ""));
            }
        }

        /// <summary>
        /// Atiende el clic de agregar linea.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void agregarLineaClic(object sender, RoutedEventArgs e)
        {
            int actividadId = idSeleccionado(cmbOtraActividad);

            if (actividadId == 0)
            {
                aviso("Seleccione la actividad que desea agregar.");
                return;
            }

            foreach (DataRow fila in actividades.Rows)
            {
                if (Convert.ToInt32(fila["actividad_id"]) == actividadId)
                    lineas.Add(nuevaLinea(fila));
            }

            actualizarOtrasActividades();
            recalcularTotales();
        }

        /// <summary>
        /// Atiende el clic de quitar linea.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void quitarLineaClic(object sender, RoutedEventArgs e)
        {
            LineaActividad linea = dgLineas.SelectedItem as LineaActividad;

            if (linea == null)
            {
                aviso("Seleccione la actividad que desea quitar.");
                return;
            }

            lineas.Remove(linea);
            actualizarOtrasActividades();
            recalcularTotales();
        }

        /// <summary>
        /// Atiende la pérdida de foco de medida.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void medidaFocoPerdido(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(recalcularTotales), DispatcherPriority.Background);
        }

        /// <summary>
        /// Recalcula totales.
        /// </summary>
        private void recalcularTotales()
        {
            List<int> ids = new List<int>();
            List<string> medidas = new List<string>();

            foreach (LineaActividad linea in lineas)
            {
                ids.Add(linea.ActividadId);
                medidas.Add(linea.Medida);
                linea.Precio = 0;
                linea.Personas = 0;
            }

            decimal subtotal = 0;
            int personas = 0;

            try
            {
                string mensaje;
                DataTable precios = facturaBD.cotizarActividades(ids, medidas, out mensaje);

                foreach (DataRow fila in precios.Rows)
                {
                    foreach (LineaActividad linea in lineas)
                    {
                        if (linea.ActividadId == Convert.ToInt32(fila["actividad_id"]))
                        {
                            linea.Precio = Convert.ToDecimal(fila["precio"]);
                            linea.Personas = Convert.ToInt32(fila["cantidad_personas"]);
                        }
                    }

                    subtotal += Convert.ToDecimal(fila["precio"]);

                    if (Convert.ToInt32(fila["cantidad_personas"]) > personas)
                        personas = Convert.ToInt32(fila["cantidad_personas"]);
                }

                if (mensaje != "")
                    aviso(mensaje);
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }

            subtotalCotizado = subtotal;
            personasNecesarias = personas;
            mostrarTotales();
        }

        /// <summary>
        /// Muestra subtotal, descuento, ISV y total con el porcentaje de descuento escrito.
        /// El ISV se calcula sobre el subtotal ya descontado.
        /// </summary>
        private void mostrarTotales()
        {
            decimal descuento = facturaBD.calcularDescuento(subtotalCotizado, facturaBD.leerDescuento(txtDescuento.Text));
            decimal baseImponible = subtotalCotizado - descuento;
            decimal isv = facturaBD.calcularIsv(baseImponible);
            decimal total = baseImponible + isv;

            lblSubtotal.Text = facturaBD.dinero(subtotalCotizado);
            lblDescuento.Text = "- " + facturaBD.dinero(descuento);
            lblIsv.Text = facturaBD.dinero(isv);
            lblTotal.Text = facturaBD.dinero(total);
            lblPersonas.Text = "" + personasNecesarias;
        }

        /// <summary>
        /// Recalcula los totales cuando cambia el porcentaje de descuento.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void descuentoCambiado(object sender, TextChangedEventArgs e)
        {
            if (lblTotal == null)
                return;

            mostrarTotales();
        }

        /// <summary>
        /// Impide escribir caracteres que no sean números o punto decimal.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void soloDecimales(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !facturaBD.caracteresDePrecio(e.Text);
        }

        // ---------- Botones ----------

        /// <summary>
        /// Atiende el clic de nuevo documento.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void nuevoDocumentoClic(object sender, RoutedEventArgs e)
        {
            nuevaCotizacion();
        }

        /// <summary>
        /// Valida y guarda los datos del formulario.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void guardarClic(object sender, RoutedEventArgs e)
        {
            DatosDocumento datos = new DatosDocumento();

            datos.rtn = txtIdentidad.Text;
            datos.empresa = txtEmpresa.Text;
            datos.contacto = txtContacto.Text;
            datos.apellido = txtApellido.Text;
            datos.telefono = txtTelefono.Text;
            datos.correo = txtCorreo.Text;
            datos.locacionId = idSeleccionado(cmbLocacion);
            datos.direccion = txtDireccion.Text;
            datos.fecha = dpFecha.SelectedDate;
            datos.validaHasta = dpValidaHasta.SelectedDate;
            datos.observaciones = txtObservaciones.Text;
            datos.descuento = txtDescuento.Text;

            datos.servicioId = idSeleccionado(cmbServicio);

            foreach (LineaActividad linea in lineas)
            {
                datos.actividadesId.Add(linea.ActividadId);
                datos.actividadesNombre.Add(linea.Nombre);
                datos.actividadesMedida.Add(linea.Medida);
            }

            try
            {
                string mensaje;
                string id;

                bool guardado = facturaBD.guardarCotizacion(datos, out mensaje, out id);

                MensajeCodelex.mostrar(this, "Cotizaciones", mensaje, guardado);

                if (guardado)
                    nuevaCotizacion();
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        /// <summary>
        /// Cancela lo que se está haciendo y cierra o limpia el formulario.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void cancelarClic(object sender, RoutedEventArgs e)
        {
            bool hayDatos = txtContacto.Text.Trim() != "" || txtEmpresa.Text.Trim() != "" || lineas.Count > 0;

            if (hayDatos)
            {
                MessageBoxResult respuesta = MessageBox.Show(
                    "¿Desea cancelar? Se perderán los datos ingresados.",
                    "Cotizaciones", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (respuesta != MessageBoxResult.Yes)
                    return;
            }

            Close();
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
        /// Muestra un mensaje de aviso al usuario.
        /// </summary>
        /// <param name="mensaje">Mensaje con el resultado.</param>
        private void aviso(string mensaje)
        {
            MensajeCodelex.mostrar(this, "Cotizaciones", mensaje, false);
        }

        /// <summary>
        /// Muestra un mensaje con el error ocurrido.
        /// </summary>
        /// <param name="ex">Excepción que se produjo.</param>
        private void mostrarError(Exception ex)
        {
            MensajeCodelex.mostrar(this, "Cotizaciones", ex.Message, false);
        }
    }
}