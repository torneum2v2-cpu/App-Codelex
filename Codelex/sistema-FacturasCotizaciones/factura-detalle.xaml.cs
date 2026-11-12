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
        DataRow? cabeceraActual = null;
        DataTable lineasActuales = new DataTable();
        int abonoPorDeshacer = 0;

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        /// <param name="facturaId">Identificador de factura.</param>
        public FacturaDetalle(int facturaId)
        {
            InitializeComponent();

            this.facturaId = facturaId;
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
                cargarFactura();
            }
            catch (Exception ex)
            {
                MensajeCodelex.mostrar(this, "Factura", ex.Message, false);
            }
        }

        /// <summary>
        /// Carga factura.
        /// </summary>
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

            cabeceraActual = f;

            string estadoPago = "" + f["estado_pago"];
            dEstadoPago.Text = estadoPago;
            if (estadoPago == "Pagada")
            {
                pillPago.Background = pincel("#E3F7F4");
                dEstadoPago.Foreground = pincel("#14837A");
            }
            else if (estadoPago == "Anulada")
            {
                pillPago.Background = pincel("#FDE4E4");
                dEstadoPago.Foreground = pincel("#B3261E");
            }
            else
            {
                pillPago.Background = pincel("#FFF3D6");
                dEstadoPago.Foreground = pincel("#9A6B00");
            }

            dRtn.Text = vacio("" + f["identificacion_cliente"]);
            dEmpresa.Text = vacio("" + f["nombre_empresa"]);
            dContacto.Text = vacio((("" + f["nombre_cliente"]) + " " + ("" + f["apellido_cliente"])).Trim());
            dTelefono.Text = vacio(Validacion.formatoTelefono("" + f["telefono_cliente"]));
            dCorreo.Text = vacio("" + f["correo_cliente"]);

            dLocacion.Text = vacio("" + f["nombre_locacion"]);
            dTipoLocacion.Text = vacio("" + f["tipo_locacion"]);
            dDireccion.Text = vacio("" + f["direccion"]);

            // Detalle y totales
            DataTable lineas = facturaBD.detalleFactura(facturaId);
            lineasActuales = lineas;
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

                // El único abono que se puede deshacer es el último que sigue vigente.
                abonoPorDeshacer = 0;
                foreach (DataRow abono in abonos.Rows)
                {
                    if (!Convert.ToBoolean(abono["anulado"]))
                        abonoPorDeshacer = Convert.ToInt32(abono["abono_id"]);
                }

                btnDeshacer.Visibility = Visibility.Collapsed;
                pnlDeshacer.Visibility = Visibility.Collapsed;
                if (abonoPorDeshacer > 0 && estadoPago != "Anulada")
                    btnDeshacer.Visibility = Visibility.Visible;

                btnAbono.Visibility = Visibility.Collapsed;
                btnSaldar.Visibility = Visibility.Collapsed;
                if (saldoActual > 0 && estadoPago != "Anulada")
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

            // Una factura anulada ya no se puede anular ni pagar; se muestra el motivo.
            pnlAnular.Visibility = Visibility.Collapsed;
            if (estadoPago == "Anulada")
            {
                btnAnular.Visibility = Visibility.Collapsed;
                lblMotivoAnulada.Text = "Factura anulada. Motivo: " + f["motivo_anulacion"];
                lblMotivoAnulada.Visibility = Visibility.Visible;
            }
            else
            {
                btnAnular.Visibility = Visibility.Visible;
                lblMotivoAnulada.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Atiende el clic de imprimir: arma el documento con los datos de la factura y abre el cuadro de impresión.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void imprimirClic(object sender, RoutedEventArgs e)
        {
            if (cabeceraActual == null)
                return;

            try
            {
                List<string> encabezado = new List<string>();
                encabezado.Add("Fecha: " + dFecha.Text + "     Orden: " + dOrden.Text);
                encabezado.Add("Forma de pago: " + dFormaPago.Text + "     Tipo: " + dTipoPago.Text + "     Estado: " + dEstadoPago.Text);
                encabezado.Add("Cliente: " + dEmpresa.Text + " (" + dRtn.Text + ")");
                encabezado.Add("Contacto: " + dContacto.Text + "     Teléfono: " + dTelefono.Text);
                encabezado.Add("Locación: " + dLocacion.Text + " - " + dDireccion.Text);

                DataTable lineas = ImpresionDocumento.crearLineas();
                foreach (DataRow fila in lineasActuales.Rows)
                {
                    ImpresionDocumento.agregarLinea(lineas, "" + fila["concepto"], "" + fila["cantidad"],
                        facturaBD.dinero(Convert.ToDecimal(fila["precio_unitario"])),
                        facturaBD.dinero(Convert.ToDecimal(fila["subtotal"])));
                }

                List<string> totales = new List<string>();
                totales.Add("Subtotal: " + lblSubtotal.Text);
                totales.Add("ISV: " + lblIsv.Text);
                totales.Add("Total: " + lblTotal.Text);

                ImpresionDocumento.imprimir(this, "Factura", dNumero.Text, encabezado, lineas, totales);
            }
            catch (Exception ex)
            {
                MensajeCodelex.mostrar(this, "Factura", ex.Message, false);
            }
        }

        /// <summary>
        /// Atiende el clic de deshacer abono: muestra el cuadro para escribir el motivo.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void deshacerClic(object sender, RoutedEventArgs e)
        {
            txtMotivoAbono.Text = "";
            pnlDeshacer.Visibility = Visibility.Visible;
            txtMotivoAbono.Focus();
        }

        /// <summary>
        /// Cierra el cuadro del motivo sin deshacer el abono.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void cancelarDeshacerClic(object sender, RoutedEventArgs e)
        {
            pnlDeshacer.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Valida el motivo, pide confirmación y deshace el último abono.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void confirmarDeshacerClic(object sender, RoutedEventArgs e)
        {
            string error = Validacion.validarMotivo(txtMotivoAbono.Text);
            if (error != "")
            {
                MensajeCodelex.mostrar(this, "Factura", error, false);
                return;
            }

            string pregunta = "¿Deshacer el último abono de esta factura?\n\nEl saldo pendiente volverá a subir.";
            if (!MensajeCodelex.preguntar(this, "Factura", pregunta))
                return;

            try
            {
                string mensaje;
                bool deshecho = facturaBD.deshacerAbono(abonoPorDeshacer, txtMotivoAbono.Text, out mensaje);

                MensajeCodelex.mostrar(this, "Factura", mensaje, deshecho);

                if (deshecho)
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

        /// <summary>
        /// Atiende el clic de anular: muestra el cuadro para escribir el motivo.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void anularClic(object sender, RoutedEventArgs e)
        {
            txtMotivo.Text = "";
            pnlAnular.Visibility = Visibility.Visible;
            txtMotivo.Focus();
        }

        /// <summary>
        /// Cierra el cuadro del motivo sin anular la factura.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void cancelarAnulacionClic(object sender, RoutedEventArgs e)
        {
            pnlAnular.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Valida el motivo, pide confirmación y anula la factura.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void confirmarAnulacionClic(object sender, RoutedEventArgs e)
        {
            string error = facturaBD.validarAnulacion(txtMotivo.Text);
            if (error != "")
            {
                MensajeCodelex.mostrar(this, "Factura", error, false);
                return;
            }

            string pregunta = "¿Anular la factura " + dNumero.Text + "?\n\nEsta acción no se puede deshacer.";
            if (!MensajeCodelex.preguntar(this, "Factura", pregunta))
                return;

            try
            {
                string mensaje;
                bool anulada = facturaBD.anularFactura(facturaId, txtMotivo.Text, out mensaje);

                MensajeCodelex.mostrar(this, "Factura", mensaje, anulada);

                if (anulada)
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

        /// <summary>
        /// Atiende el clic de saldar.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
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

            if (!MensajeCodelex.preguntar(this, "Factura", pregunta))
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

        /// <summary>
        /// Atiende el clic de abono.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
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

        /// <summary>
        /// Convierte un valor de fecha en texto dd/MM/yyyy, o un guion si no hay fecha.
        /// </summary>
        /// <param name="valor">Valor a usar.</param>
        /// <returns>Fecha como dd/MM/yyyy, o un guion si no hay fecha.</returns>
        private static string fecha(object valor)
        {
            if (valor is DateTime fecha)
                return fecha.ToString("dd/MM/yyyy");

            return "—";
        }

        /// <summary>
        /// Devuelve un guion cuando el texto está vacío.
        /// </summary>
        /// <param name="texto">Texto a usar.</param>
        /// <returns>El mismo texto, o un guion si estaba vacío.</returns>
        private static string vacio(string texto)
        {
            if (texto.Trim() == "")
                return "—";

            return texto;
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
            DialogResult = huboCambios;
            Close();
        }
    }
}
