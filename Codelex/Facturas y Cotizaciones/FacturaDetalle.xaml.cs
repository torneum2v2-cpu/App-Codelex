using Codelex.Clases_BDD;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Codelex.Facturas_y_Cotizaciones
{
    /// <summary>
    /// Lógica de interacción para FacturaDetalle.xaml
    /// Muestra (solo lectura) una factura ya emitida.
    /// </summary>
    public partial class FacturaDetalle : Window
    {
        Facturacion facturaBD = new Facturacion();
        int facturaId;

        public FacturaDetalle(int facturaId)
        {
            InitializeComponent();

            this.facturaId = facturaId;
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CargarFactura();
            }
            catch (Exception ex)
            {
                MensajeCodelex.Mostrar(this, "Factura", ex.Message, false);
            }
        }

        private void CargarFactura()
        {
            DataTable cabecera = facturaBD.ObtenerFactura(facturaId);

            if (cabecera.Rows.Count == 0)
            {
                MensajeCodelex.Mostrar(this, "Factura", "No se encontró la factura.", false);
                Close();
                return;
            }

            DataRow f = cabecera.Rows[0];

            string numero = "FAC-" + facturaId.ToString().PadLeft(6, '0');
            lblTitulo.Text = "Factura " + numero;

            dNumero.Text = numero;
            dFecha.Text = Fecha(f["fechaFactura"]);
            dOrden.Text = "" + f["orden_id"];
            dFormaPago.Text = Vacio("" + f["nombreFormaPago"]);
            dTipoPago.Text = ("" + f["tipo_pago"]) == "Credito" ? "Crédito" : "Contado";

            string estadoPago = "" + f["estado_pago"];
            dEstadoPago.Text = estadoPago;
            if (estadoPago == "Pagada")
            {
                pillPago.Background = Pincel("#E3F7F4");
                dEstadoPago.Foreground = Pincel("#14837A");
            }
            else
            {
                pillPago.Background = Pincel("#FFF3D6");
                dEstadoPago.Foreground = Pincel("#9A6B00");
            }

            dRtn.Text = Vacio("" + f["identificacionCliente"]);
            dEmpresa.Text = Vacio("" + f["nombreEmpresa"]);
            dContacto.Text = Vacio((("" + f["nombreCliente"]) + " " + ("" + f["apellidoCliente"])).Trim());
            dTelefono.Text = Vacio("" + f["telefonoCliente"]);
            dCorreo.Text = Vacio("" + f["correoCliente"]);

            dLocacion.Text = Vacio("" + f["nombreLocacion"]);
            dTipoLocacion.Text = Vacio("" + f["tipoLocacion"]);
            dDireccion.Text = Vacio("" + f["direccion"]);

            // Detalle y totales
            DataTable lineas = facturaBD.DetalleFactura(facturaId);
            dgLineas.ItemsSource = lineas.DefaultView;

            decimal subtotal = 0;
            foreach (DataRow linea in lineas.Rows)
            {
                subtotal += Convert.ToDecimal(linea["subtotal"]);
            }

            // El total es el que quedó guardado en la factura; el ISV es la diferencia.
            decimal total = Convert.ToDecimal(f["precio_facturado"]);
            decimal isv = total - subtotal;

            lblSubtotal.Text = facturaBD.Dinero(subtotal);
            lblIsv.Text = facturaBD.Dinero(isv);
            lblTotal.Text = facturaBD.Dinero(total);

            // Cuotas (solo si la factura fue a crédito)
            if (("" + f["tipo_pago"]) == "Credito")
            {
                DataTable cuotas = facturaBD.CuotasFactura(facturaId);
                dgCuotas.ItemsSource = cuotas.DefaultView;
                lblTituloCuotas.Text = "Cuotas (" + cuotas.Rows.Count + ")";
                pnlCuotas.Visibility = Visibility.Visible;
            }
            else
            {
                pnlCuotas.Visibility = Visibility.Collapsed;
            }
        }

        private static string Fecha(object valor)
        {
            if (valor is DateTime fecha)
                return fecha.ToString("dd/MM/yyyy");

            return "—";
        }

        private static string Vacio(string texto)
        {
            return texto.Trim() == "" ? "—" : texto;
        }

        private static Brush Pincel(string color)
        {
            SolidColorBrush pincel = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            pincel.Freeze();
            return pincel;
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
    }
}
