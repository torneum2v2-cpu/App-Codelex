using Codelex.AccesoDatos;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Codelex.FacturasCotizaciones
{
    /// <summary>
    /// Lógica de interacción para factura-detalle.xaml
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
            Loaded += ventanaCargada;
        }

        private void ventanaCargada(object sender, RoutedEventArgs e)
        {
            try
            {
                cargarFactura();
            }
            catch (Exception ex)
            {
                MensajeCodelex.mostrar(this, "Factura", ex.Message, false);
            }
        }

        private void cargarFactura()
        {
            DataTable cabecera = facturaBD.obtenerFactura(facturaId);

            if (cabecera.Rows.Count == 0)
            {
                MensajeCodelex.mostrar(this, "Factura", "No se encontró la factura.", false);
                Close();
                return;
            }

            DataRow f = cabecera.Rows[0];

            string numero = Facturacion.numeroFactura(facturaId);
            lblTitulo.Text = "Factura " + numero;

            dNumero.Text = numero;
            dFecha.Text = fecha(f["fecha_factura"]);
            dOrden.Text = "" + f["orden_id"];
            dFormaPago.Text = vacio("" + f["nombre_forma_pago"]);
            dTipoPago.Text = Facturacion.CONTADO;
            if (("" + f["tipo_pago"]) == "Credito")
                dTipoPago.Text = Facturacion.CREDITO;

            string estadoPago = "" + f["estado_pago"];
            dEstadoPago.Text = estadoPago;
            if (estadoPago == "Pagada")
            {
                pillPago.Background = pincel("#E3F7F4");
                dEstadoPago.Foreground = pincel("#14837A");
            }
            else
            {
                pillPago.Background = pincel("#FFF3D6");
                dEstadoPago.Foreground = pincel("#9A6B00");
            }

            dRtn.Text = vacio("" + f["identificacion_cliente"]);
            dEmpresa.Text = vacio("" + f["nombre_empresa"]);
            dContacto.Text = vacio((("" + f["nombre_cliente"]) + " " + ("" + f["apellido_cliente"])).Trim());
            dTelefono.Text = vacio("" + f["telefono_cliente"]);
            dCorreo.Text = vacio("" + f["correo_cliente"]);

            dLocacion.Text = vacio("" + f["nombre_locacion"]);
            dTipoLocacion.Text = vacio("" + f["tipo_locacion"]);
            dDireccion.Text = vacio("" + f["direccion"]);

            // Detalle y totales
            DataTable lineas = facturaBD.detalleFactura(facturaId);
            dgLineas.ItemsSource = lineas.DefaultView;

            decimal subtotal = 0;
            foreach (DataRow linea in lineas.Rows)
            {
                subtotal += Convert.ToDecimal(linea["subtotal"]);
            }

            // El total es el que quedó guardado en la factura; el ISV es la diferencia.
            decimal total = Convert.ToDecimal(f["precio_facturado"]);
            decimal isv = total - subtotal;

            lblSubtotal.Text = facturaBD.dinero(subtotal);
            lblIsv.Text = facturaBD.dinero(isv);
            lblTotal.Text = facturaBD.dinero(total);

            saldoActual = Convert.ToDecimal(f["saldo"]);
            lblPagado.Text = facturaBD.dinero(Convert.ToDecimal(f["pagado"]));
            lblSaldo.Text = facturaBD.dinero(saldoActual);

            // Cuotas y abonos (solo si la factura fue a crédito)
            if (("" + f["tipo_pago"]) == "Credito")
            {
                facturaBD.actualizarCuotasVencidas();

                cuotasActuales = facturaBD.cuotasFactura(facturaId);
                dgCuotas.ItemsSource = cuotasActuales.DefaultView;
                lblTituloCuotas.Text = "Cuotas (" + cuotasActuales.Rows.Count + ")";

                DataTable abonos = facturaBD.listarAbonos(facturaId);
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

        private void saldarClic(object sender, RoutedEventArgs e)
        {
            string mensaje;
            DataRow? cuota = facturaBD.elegirCuota(cuotasActuales, dgCuotas.SelectedItem as DataRowView, out mensaje);

            if (cuota == null)
            {
                MensajeCodelex.mostrar(this, "Factura", mensaje, false);
                return;
            }

            string pregunta = "¿Saldar la cuota " + cuota["numero_cuota"] + " por "
                              + facturaBD.dinero(Convert.ToDecimal(cuota["saldo"])) + "?";

            MessageBoxResult respuesta = MessageBox.Show(pregunta, "Factura", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (respuesta != MessageBoxResult.Yes)
                return;

            try
            {
                bool saldada = facturaBD.saldarCuota(Convert.ToInt32(cuota["cuota_id"]), out mensaje);

                MensajeCodelex.mostrar(this, "Factura", mensaje, saldada);

                if (saldada)
                {
                    huboCambios = true;
                    cargarFactura();
                }
            }
            catch (Exception ex)
            {
                MensajeCodelex.mostrar(this, "Factura", ex.Message, false);
            }
        }

        private void abonoClic(object sender, RoutedEventArgs e)
        {
            RegistrarAbono ventana = new RegistrarAbono(facturaId, saldoActual);
            ventana.Owner = this;

            if (ventana.ShowDialog() == true)
            {
                huboCambios = true;

                try
                {
                    cargarFactura();
                }
                catch (Exception ex)
                {
                    MensajeCodelex.mostrar(this, "Factura", ex.Message, false);
                }
            }
        }

        private static string fecha(object valor)
        {
            if (valor is DateTime fecha)
                return fecha.ToString("dd/MM/yyyy");

            return "—";
        }

        private static string vacio(string texto)
        {
            return texto.Trim() == "" ? "—" : texto;
        }

        private static Brush pincel(string color)
        {
            SolidColorBrush pincel = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            pincel.Freeze();
            return pincel;
        }

        // ---------- Ventana ----------

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
            DialogResult = huboCambios;
            Close();
        }
    }
}
