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
        decimal saldoActual = 0;
        DataTable cuotasActuales = new DataTable();
        bool huboCambios = false;

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

            string numero = Facturacion.NumeroFactura(facturaId);
            lblTitulo.Text = "Factura " + numero;

            dNumero.Text = numero;
            dFecha.Text = Fecha(f["fecha_factura"]);
            dOrden.Text = "" + f["orden_id"];
            dFormaPago.Text = Vacio("" + f["nombre_forma_pago"]);
            dTipoPago.Text = Facturacion.Contado;
            if (("" + f["tipo_pago"]) == "Credito")
                dTipoPago.Text = Facturacion.Credito;

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

            dRtn.Text = Vacio("" + f["identificacion_cliente"]);
            dEmpresa.Text = Vacio("" + f["nombre_empresa"]);
            dContacto.Text = Vacio((("" + f["nombre_cliente"]) + " " + ("" + f["apellido_cliente"])).Trim());
            dTelefono.Text = Vacio("" + f["telefono_cliente"]);
            dCorreo.Text = Vacio("" + f["correo_cliente"]);

            dLocacion.Text = Vacio("" + f["nombre_locacion"]);
            dTipoLocacion.Text = Vacio("" + f["tipo_locacion"]);
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

            saldoActual = Convert.ToDecimal(f["saldo"]);
            lblPagado.Text = facturaBD.Dinero(Convert.ToDecimal(f["pagado"]));
            lblSaldo.Text = facturaBD.Dinero(saldoActual);

            // Cuotas y abonos (solo si la factura fue a crédito)
            if (("" + f["tipo_pago"]) == "Credito")
            {
                facturaBD.ActualizarCuotasVencidas();

                cuotasActuales = facturaBD.CuotasFactura(facturaId);
                dgCuotas.ItemsSource = cuotasActuales.DefaultView;
                lblTituloCuotas.Text = "Cuotas (" + cuotasActuales.Rows.Count + ")";

                DataTable abonos = facturaBD.ListarAbonos(facturaId);
                dgAbonos.ItemsSource = abonos.DefaultView;

                btnAbono.Visibility = Visibility.Collapsed;
                btnSaldar.Visibility = Visibility.Collapsed;
                if (saldoActual > 0)
                {
                    btnAbono.Visibility = Visibility.Visible;
                    btnSaldar.Visibility = Visibility.Visible;
                }

                pnlCuotas.Visibility = Visibility.Visible;
            }
            else
            {
                pnlCuotas.Visibility = Visibility.Collapsed;
            }
        }

        private void btnSaldar_Click(object sender, RoutedEventArgs e)
        {
            string mensaje;
            DataRow? cuota = facturaBD.ElegirCuota(cuotasActuales, dgCuotas.SelectedItem as DataRowView, out mensaje);

            if (cuota == null)
            {
                MensajeCodelex.Mostrar(this, "Factura", mensaje, false);
                return;
            }

            string pregunta = "¿Saldar la cuota " + cuota["numero_cuota"] + " por "
                              + facturaBD.Dinero(Convert.ToDecimal(cuota["saldo"])) + "?";

            MessageBoxResult respuesta = MessageBox.Show(pregunta, "Factura", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (respuesta != MessageBoxResult.Yes)
                return;

            try
            {
                bool saldada = facturaBD.SaldarCuota(Convert.ToInt32(cuota["cuota_id"]), out mensaje);

                MensajeCodelex.Mostrar(this, "Factura", mensaje, saldada);

                if (saldada)
                {
                    huboCambios = true;
                    CargarFactura();
                }
            }
            catch (Exception ex)
            {
                MensajeCodelex.Mostrar(this, "Factura", ex.Message, false);
            }
        }

        private void btnAbono_Click(object sender, RoutedEventArgs e)
        {
            RegistrarAbono ventana = new RegistrarAbono(facturaId, saldoActual);
            ventana.Owner = this;

            if (ventana.ShowDialog() == true)
            {
                huboCambios = true;

                try
                {
                    CargarFactura();
                }
                catch (Exception ex)
                {
                    MensajeCodelex.Mostrar(this, "Factura", ex.Message, false);
                }
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
            DialogResult = huboCambios;
            Close();
        }
    }
}
