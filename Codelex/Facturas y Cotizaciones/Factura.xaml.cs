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
    /// Lógica de interacción para Factura.xaml
    /// </summary>
    public partial class Factura : Window
    {
        Cliente clienteBD = new Cliente();
        Facturacion facturaBD = new Facturacion();

        string ordenId = "";
        DataTable locacionesCliente = new DataTable();
        ObservableCollection<LineaDocumento> lineas = new ObservableCollection<LineaDocumento>();

        public Factura()
        {
            InitializeComponent();

            cmbTipoLocacion.IsEnabled = false;

            txtConcepto.MaxLength = 150;
            txtCantidad.MaxLength = 4;
            txtPrecio.MaxLength = 10;

            dgLineas.ItemsSource = lineas;

            cmbLocacion.SelectionChanged += cmbLocacion_SelectionChanged;
            cmbServicioRapido.SelectionChanged += cmbServicioRapido_SelectionChanged;
            cmbCuotas.SelectionChanged += cmbCuotas_SelectionChanged;
            txtCantidad.PreviewTextInput += SoloNumeros;
            txtPrecio.PreviewTextInput += SoloDecimal;
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CargarListas();
                NuevaFactura();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        // ---------- Carga de datos ----------

        private void CargarListas()
        {
            cmbServicioRapido.Items.Clear();

            DataTable servicios = facturaBD.ListarServicios();
            foreach (DataRow fila in servicios.Rows)
            {
                cmbServicioRapido.Items.Add(new ItemLista(Convert.ToInt32(fila["servicio_id"]), "" + fila["nombreServicio"], ""));
            }

            cmbMetodoPago.Items.Clear();

            DataTable formas = facturaBD.ListarFormasPago();
            foreach (DataRow fila in formas.Rows)
            {
                cmbMetodoPago.Items.Add(new ItemLista(Convert.ToInt32(fila["forma_pago_id"]), "" + fila["nombreFormaPago"], ""));
            }

            cmbCuotas.Items.Clear();
            cmbCuotas.Items.Add(new ItemLista(1, "Contado (1 pago)", ""));
            cmbCuotas.Items.Add(new ItemLista(6, "6 cuotas", ""));
            cmbCuotas.Items.Add(new ItemLista(12, "12 cuotas", ""));
            cmbCuotas.Items.Add(new ItemLista(18, "18 cuotas", ""));
            cmbCuotas.Items.Add(new ItemLista(24, "24 cuotas", ""));
        }

        private void CargarOrdenes()
        {
            try
            {
                DataTable tabla = facturaBD.ListarOrdenesFacturables(txtBuscarOrden.Text.Trim());
                dgOrdenes.ItemsSource = tabla.DefaultView;
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private int IdSeleccionado(ComboBox combo)
        {
            ItemLista item = combo.SelectedItem as ItemLista;

            if (item == null)
                return 0;

            return item.Id;
        }

        // ---------- Factura nueva ----------

        private void NuevaFactura()
        {
            txtN_documento.Text = "(automático)";
            dpFecha.SelectedDate = DateTime.Today;

            lineas.Clear();
            LimpiarEntradaLinea();

            cmbMetodoPago.SelectedIndex = cmbMetodoPago.Items.Count > 0 ? 0 : -1;
            cmbCuotas.SelectedIndex = 0;

            LimpiarOrden();

            if (txtBuscarOrden.Text != "")
                txtBuscarOrden.Text = "";

            CargarOrdenes();
            RecalcularTotales();
        }

        private void LimpiarOrden()
        {
            ordenId = "";
            txtOrdenSeleccionada.Text = "";

            TxtIdentidad.Text = "";
            TxtEmpresa.Text = "";
            TxtContacto.Text = "";
            TxtApellido.Text = "";
            TxtTelefono.Text = "";
            TxtCorreo.Text = "";
            txtDireccion.Text = "";

            cmbLocacion.Items.Clear();
            dgOrdenes.SelectedItem = null;
        }

        private void LimpiarEntradaLinea()
        {
            cmbServicioRapido.SelectedIndex = -1;
            txtConcepto.Text = "";
            txtCantidad.Text = "";
            txtPrecio.Text = "";
        }

        // ---------- Eventos de orden ----------

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

        private void txtBuscarOrden_TextChanged(object sender, TextChangedEventArgs e)
        {
            CargarOrdenes();
        }

        private void dgOrdenes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgOrdenes.SelectedItem == null)
                return;

            DataRowView fila = (DataRowView)dgOrdenes.SelectedItem;

            try
            {
                ordenId = "" + fila["orden_id"];
                txtOrdenSeleccionada.Text = ordenId;

                TxtIdentidad.Text = "" + fila["identificacionCliente"];
                TxtEmpresa.Text = "" + fila["nombreEmpresa"];
                TxtContacto.Text = "" + fila["nombreCliente"];
                TxtApellido.Text = "" + fila["apellidoCliente"];
                TxtTelefono.Text = "" + fila["telefonoCliente"];
                TxtCorreo.Text = "" + fila["correoCliente"];

                // La locación de la orden es la única disponible
                locacionesCliente = new DataTable();
                locacionesCliente.Columns.Add("locacion_id", typeof(int));
                locacionesCliente.Columns.Add("nombreLocacion", typeof(string));
                locacionesCliente.Columns.Add("tipoLocacion", typeof(string));
                locacionesCliente.Columns.Add("direccion", typeof(string));
                locacionesCliente.Rows.Add(Convert.ToInt32(fila["locacion_id"]), "" + fila["nombreLocacion"],
                                           "" + fila["tipoLocacion"], "" + fila["direccion"]);

                cmbLocacion.Items.Clear();
                cmbLocacion.Items.Add(new ItemLista(Convert.ToInt32(fila["locacion_id"]), "" + fila["nombreLocacion"], ""));
                cmbLocacion.SelectedIndex = 0;

                // Actividades de la orden con su precio
                lineas.Clear();

                DataTable detalle = facturaBD.DetalleOrden(ordenId);
                foreach (DataRow linea in detalle.Rows)
                {
                    LineaDocumento nueva = new LineaDocumento();
                    nueva.Origen = "Orden";
                    nueva.Concepto = "" + linea["concepto"];
                    nueva.Cantidad = Convert.ToInt32(linea["cantidad"]);
                    nueva.Precio = Convert.ToDecimal(linea["precio_unitario"]);
                    lineas.Add(nueva);
                }

                RecalcularTotales();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void btnLimpiarClientes(object sender, RoutedEventArgs e)
        {
            LimpiarOrden();
            lineas.Clear();
            RecalcularTotales();
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
            if (ordenId == "")
            {
                Aviso("Primero seleccione una orden de la lista inferior.");
                return;
            }

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
            nueva.Origen = "Adicional";
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

            if (linea.Origen == "Orden")
            {
                Aviso("Las líneas de la orden no se pueden quitar. Solo los adicionales.");
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

            int cuotas = IdSeleccionado(cmbCuotas);
            if (cuotas <= 0)
                cuotas = 1;

            lblValorCuota.Text = cuotas + " x " + facturaBD.Dinero(facturaBD.CalcularCuota(total, cuotas));
        }

        // ---------- Pago ----------

        private void cmbCuotas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RecalcularTotales();
        }

        // ---------- Botones ----------

        private void btnNuevoDocumento_Click(object sender, RoutedEventArgs e)
        {
            NuevaFactura();
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

            datos.OrdenId = ordenId;
            datos.FormaPagoId = IdSeleccionado(cmbMetodoPago);
            datos.Cuotas = IdSeleccionado(cmbCuotas);

            foreach (LineaDocumento linea in lineas)
            {
                datos.LineasConcepto.Add(linea.Concepto);
                datos.LineasCantidad.Add(linea.Cantidad);
                datos.LineasPrecio.Add(linea.Precio);
                datos.LineasAdicional.Add(linea.Origen == "Adicional");
            }

            try
            {
                string mensaje;
                string id;

                bool guardado = facturaBD.GuardarFactura(datos, out mensaje, out id);

                MensajeCodelex.Mostrar(this, "Facturas", mensaje, guardado);

                if (guardado)
                    NuevaFactura();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            bool hayDatos = ordenId != "" || lineas.Count > 0;

            if (hayDatos)
            {
                MessageBoxResult respuesta = MessageBox.Show(
                    "¿Desea cancelar? Se perderán los datos ingresados.",
                    "Facturas", MessageBoxButton.YesNo, MessageBoxImage.Question);

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
            MensajeCodelex.Mostrar(this, "Facturas", mensaje, false);
        }

        private void MostrarError(Exception ex)
        {
            MensajeCodelex.Mostrar(this, "Facturas", ex.Message, false);
        }
    }
}