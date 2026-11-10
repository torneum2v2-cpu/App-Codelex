using Codelex.Clases_BDD;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Codelex.Facturas_y_Cotizaciones
{
    /// <summary>
    /// Lógica de interacción para Cotizacion.xaml
    /// </summary>
    public partial class Cotizacion : Window
    {
        Cliente clienteBD = new Cliente();
        Facturacion facturaBD = new Facturacion();

        DataTable locacionesCliente = new DataTable();
        DataTable actividades = new DataTable();
        ObservableCollection<LineaActividad> lineas = new ObservableCollection<LineaActividad>();

        public Cotizacion()
        {
            InitializeComponent();

            cmbTipoLocacion.IsEnabled = false;

            TxtIdentidad.MaxLength = 30;
            TxtEmpresa.MaxLength = 50;
            TxtContacto.MaxLength = 50;
            TxtApellido.MaxLength = 50;
            TxtTelefono.MaxLength = 50;
            TxtCorreo.MaxLength = 50;
            txtDireccion.MaxLength = 80;
            txtObservaciones.MaxLength = 200;

            dgLineas.ItemsSource = lineas;

            cmbLocacion.SelectionChanged += cmbLocacion_SelectionChanged;
            cmbServicio.SelectionChanged += cmbServicio_SelectionChanged;
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CargarServicios();
                actividades = facturaBD.ListarActividades();
                NuevaCotizacion();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        // ---------- Carga de datos ----------

        private void CargarServicios()
        {
            cmbServicio.Items.Clear();

            DataTable servicios = facturaBD.ListarServicios();
            foreach (DataRow fila in servicios.Rows)
            {
                cmbServicio.Items.Add(new ItemLista(Convert.ToInt32(fila["servicio_id"]), "" + fila["nombreServicio"], ""));
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
                Aviso("Este cliente no tiene locaciones registradas. Escriba la dirección del servicio.");
        }

        private int IdSeleccionado(ComboBox combo)
        {
            ItemLista item = combo.SelectedItem as ItemLista;

            if (item == null)
                return 0;

            return item.Id;
        }

        // ---------- Cotización nueva ----------

        private void NuevaCotizacion()
        {
            txtN_documento.Text = "(automático)";
            dpFecha.SelectedDate = DateTime.Today;
            dpValidaHasta.SelectedDate = DateTime.Today.AddDays(15);

            cmbServicio.SelectedIndex = -1;
            lineas.Clear();
            ActualizarOtrasActividades();
            txtObservaciones.Text = "";

            LimpiarCliente();
            RecalcularTotales();
        }

        private void LimpiarCliente()
        {
            TxtIdentidad.Text = "";
            TxtEmpresa.Text = "";
            TxtContacto.Text = "";
            TxtApellido.Text = "";
            TxtTelefono.Text = "";
            TxtCorreo.Text = "";
            txtDireccion.Text = "";

            cmbLocacion.Items.Clear();
        }

        // ---------- Eventos de cliente ----------

        private void BuscarCliente_Click(object sender, RoutedEventArgs e)
        {
            ElegirCliente ventana = new ElegirCliente();
            ventana.Owner = this;

            if (ventana.ShowDialog() == true && ventana.ClienteElegido != null)
                LlenarCliente(ventana.ClienteElegido.Row);
        }

        private void LlenarCliente(DataRow fila)
        {
            string rtn = "" + fila["identificacionCliente"];

            TxtIdentidad.Text = rtn;
            TxtEmpresa.Text = "" + fila["nombreEmpresa"];
            TxtContacto.Text = "" + fila["nombreCliente"];
            TxtApellido.Text = "" + fila["apellidoCliente"];
            TxtTelefono.Text = "" + fila["telefonoCliente"];
            TxtCorreo.Text = "" + fila["correoCliente"];

            try
            {
                CargarLocaciones(rtn);
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

            int locacionId = IdSeleccionado(cmbLocacion);
            if (locacionId == 0)
                return;

            foreach (DataRow fila in locacionesCliente.Rows)
            {
                if (Convert.ToInt32(fila["locacion_id"]) == locacionId)
                {
                    cmbTipoLocacion.Items.Add("" + fila["tipoLocacion"]);
                    cmbTipoLocacion.SelectedIndex = 0;
                    txtDireccion.Text = "" + fila["direccion"];
                }
            }
        }

        // ---------- Actividades ----------

        private void cmbServicio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int servicioId = IdSeleccionado(cmbServicio);

            if (servicioId == 0)
                return;

            lineas.Clear();

            foreach (DataRow fila in actividades.Rows)
            {
                if (Convert.ToInt32(fila["servicio_id"]) == servicioId)
                    lineas.Add(NuevaLinea(fila));
            }

            ActualizarOtrasActividades();
            RecalcularTotales();
        }

        private LineaActividad NuevaLinea(DataRow fila)
        {
            LineaActividad linea = new LineaActividad();
            linea.ActividadId = Convert.ToInt32(fila["actividad_id"]);
            linea.Nombre = "" + fila["nombre"];
            linea.Unidad = "" + fila["unidad"];
            return linea;
        }

        private bool YaEstaEnLaLista(int actividadId)
        {
            foreach (LineaActividad linea in lineas)
            {
                if (linea.ActividadId == actividadId)
                    return true;
            }

            return false;
        }

        private void ActualizarOtrasActividades()
        {
            cmbOtraActividad.Items.Clear();

            foreach (DataRow fila in actividades.Rows)
            {
                int id = Convert.ToInt32(fila["actividad_id"]);

                if (!YaEstaEnLaLista(id))
                    cmbOtraActividad.Items.Add(new ItemLista(id, "" + fila["nombre"] + " (" + fila["unidad"] + ")", ""));
            }
        }

        private void btnAgregarLinea_Click(object sender, RoutedEventArgs e)
        {
            int actividadId = IdSeleccionado(cmbOtraActividad);

            if (actividadId == 0)
            {
                Aviso("Seleccione la actividad que desea agregar.");
                return;
            }

            foreach (DataRow fila in actividades.Rows)
            {
                if (Convert.ToInt32(fila["actividad_id"]) == actividadId)
                    lineas.Add(NuevaLinea(fila));
            }

            ActualizarOtrasActividades();
            RecalcularTotales();
        }

        private void btnQuitarLinea_Click(object sender, RoutedEventArgs e)
        {
            LineaActividad linea = dgLineas.SelectedItem as LineaActividad;

            if (linea == null)
            {
                Aviso("Seleccione la actividad que desea quitar.");
                return;
            }

            lineas.Remove(linea);
            ActualizarOtrasActividades();
            RecalcularTotales();
        }

        private void Medida_LostFocus(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(RecalcularTotales), DispatcherPriority.Background);
        }

        private void RecalcularTotales()
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
                DataTable precios = facturaBD.CotizarActividades(ids, medidas, out mensaje);

                foreach (DataRow fila in precios.Rows)
                {
                    foreach (LineaActividad linea in lineas)
                    {
                        if (linea.ActividadId == Convert.ToInt32(fila["actividad_id"]))
                        {
                            linea.Precio = Convert.ToDecimal(fila["precio"]);
                            linea.Personas = Convert.ToInt32(fila["personas"]);
                        }
                    }

                    subtotal += Convert.ToDecimal(fila["precio"]);

                    if (Convert.ToInt32(fila["personas"]) > personas)
                        personas = Convert.ToInt32(fila["personas"]);
                }

                if (mensaje != "")
                    Aviso(mensaje);
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }

            decimal isv = facturaBD.CalcularIsv(subtotal);
            decimal total = subtotal + isv;

            lblSubtotal.Text = facturaBD.Dinero(subtotal);
            lblIsv.Text = facturaBD.Dinero(isv);
            lblTotal.Text = facturaBD.Dinero(total);
            lblPersonas.Text = "" + personas;
        }

        // ---------- Botones ----------

        private void btnNuevoDocumento_Click(object sender, RoutedEventArgs e)
        {
            NuevaCotizacion();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            DatosDocumento datos = new DatosDocumento();

            datos.Rtn = TxtIdentidad.Text;
            datos.Empresa = TxtEmpresa.Text;
            datos.Contacto = TxtContacto.Text;
            datos.Apellido = TxtApellido.Text;
            datos.Telefono = TxtTelefono.Text;
            datos.Correo = TxtCorreo.Text;
            datos.LocacionId = IdSeleccionado(cmbLocacion);
            datos.Direccion = txtDireccion.Text;
            datos.Fecha = dpFecha.SelectedDate;
            datos.ValidaHasta = dpValidaHasta.SelectedDate;
            datos.Observaciones = txtObservaciones.Text;

            datos.ServicioId = IdSeleccionado(cmbServicio);

            foreach (LineaActividad linea in lineas)
            {
                datos.ActividadesId.Add(linea.ActividadId);
                datos.ActividadesNombre.Add(linea.Nombre);
                datos.ActividadesMedida.Add(linea.Medida);
            }

            try
            {
                string mensaje;
                string id;

                bool guardado = facturaBD.GuardarCotizacion(datos, out mensaje, out id);

                MensajeCodelex.Mostrar(this, "Cotizaciones", mensaje, guardado);

                if (guardado)
                    NuevaCotizacion();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            bool hayDatos = TxtContacto.Text.Trim() != "" || TxtEmpresa.Text.Trim() != "" || lineas.Count > 0;

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
            MensajeCodelex.Mostrar(this, "Cotizaciones", mensaje, false);
        }

        private void MostrarError(Exception ex)
        {
            MensajeCodelex.Mostrar(this, "Cotizaciones", ex.Message, false);
        }
    }
}