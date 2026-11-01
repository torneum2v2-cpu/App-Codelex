using Codelex.Clases_BDD;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        ObservableCollection<LineaDocumento> lineas = new ObservableCollection<LineaDocumento>();

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
            txtConcepto.MaxLength = 150;
            txtCantidad.MaxLength = 4;
            txtPrecio.MaxLength = 10;
            txtObservaciones.MaxLength = 200;

            dgLineas.ItemsSource = lineas;

            cmbLocacion.SelectionChanged += cmbLocacion_SelectionChanged;
            cmbServicioRapido.SelectionChanged += cmbServicioRapido_SelectionChanged;
            txtCantidad.PreviewTextInput += SoloNumeros;
            txtPrecio.PreviewTextInput += SoloDecimal;
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CargarServicios();
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
            cmbServicioRapido.Items.Clear();

            DataTable servicios = facturaBD.ListarServicios();
            foreach (DataRow fila in servicios.Rows)
            {
                cmbServicioRapido.Items.Add(new ItemLista(Convert.ToInt32(fila["servicio_id"]), "" + fila["nombreServicio"], ""));
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

            lineas.Clear();
            LimpiarEntradaLinea();
            txtObservaciones.Text = "";

            LimpiarCliente();

            if (txtBuscarCliente.Text != "")
                txtBuscarCliente.Text = "";

            CargarClientes();
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
            dgClientes.SelectedItem = null;
        }

        private void LimpiarEntradaLinea()
        {
            cmbServicioRapido.SelectedIndex = -1;
            txtConcepto.Text = "";
            txtCantidad.Text = "";
            txtPrecio.Text = "";
        }

        // ---------- Eventos de cliente ----------

        private void SoloNumeros(object sender, TextCompositionEventArgs e)
        {
            if (!clienteBD.SoloNumeros(e.Text))
                e.Handled = true;
        }

        private void SoloDecimal(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != '.')
                {
                    e.Handled = true;
                    return;
                }
            }
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

        // ---------- Detalle ----------

        private void cmbServicioRapido_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemLista item = cmbServicioRapido.SelectedItem as ItemLista;

            if (item == null)
                return;

            txtConcepto.Text = item.Texto;
        }

        private void btnAgregarLinea_Click(object sender, RoutedEventArgs e)
        {
            int cantidad;
            decimal precio;

            string mensaje = facturaBD.ValidarLinea(txtConcepto.Text, txtCantidad.Text, txtPrecio.Text,
                                                    out cantidad, out precio);
            if (mensaje != "")
            {
                Aviso(mensaje);
                return;
            }

            LineaDocumento nueva = new LineaDocumento();
            nueva.Origen = "Servicio";
            nueva.Concepto = txtConcepto.Text.Trim();
            nueva.Cantidad = cantidad;
            nueva.Precio = precio;

            lineas.Add(nueva);

            LimpiarEntradaLinea();
            RecalcularTotales();
        }

        private void btnQuitarLinea_Click(object sender, RoutedEventArgs e)
        {
            LineaDocumento linea = dgLineas.SelectedItem as LineaDocumento;

            if (linea == null)
            {
                Aviso("Seleccione la línea que desea quitar.");
                return;
            }

            lineas.Remove(linea);
            RecalcularTotales();
        }

        private void RecalcularTotales()
        {
            decimal subtotal = 0;

            foreach (LineaDocumento linea in lineas)
            {
                subtotal += linea.Subtotal;
            }

            decimal isv = facturaBD.CalcularIsv(subtotal);
            decimal total = subtotal + isv;

            lblSubtotal.Text = facturaBD.Dinero(subtotal);
            lblIsv.Text = facturaBD.Dinero(isv);
            lblTotal.Text = facturaBD.Dinero(total);
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

            foreach (LineaDocumento linea in lineas)
            {
                datos.LineasConcepto.Add(linea.Concepto);
                datos.LineasCantidad.Add(linea.Cantidad);
                datos.LineasPrecio.Add(linea.Precio);
                datos.LineasAdicional.Add(false);
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