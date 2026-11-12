using Codelex.AccesoDatos;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Codelex.FacturasCotizaciones
{
    /// <summary>
    /// Lógica de interacción para factura.xaml
    /// Todo viene de la orden y su cotización; solo se elige cómo se paga.
    /// </summary>
    public partial class Factura : Window
    {
        Facturacion facturaBD = new Facturacion();

        string ordenId = "";
        decimal total = 0;

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        /// <param name="ordenId">Identificador de orden.</param>
        public Factura(string ordenId)
        {
            InitializeComponent();

            this.ordenId = ordenId;

            cmbTipoPago.SelectionChanged += pagoSeleccionCambiada;
            cmbCuotas.SelectionChanged += pagoSeleccionCambiada;
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
                cargarListas();

                if (!cargarOrden())
                {
                    Close();
                    return;
                }

                actualizarValorCuota();
            }
            catch (Exception ex)
            {
                mostrarError(ex);
                Close();
            }
        }

        /// <summary>
        /// Carga listas.
        /// </summary>
        private void cargarListas()
        {
            foreach (string tipo in facturaBD.tiposPago())
            {
                cmbTipoPago.Items.Add(tipo);
            }

            DataTable formas = facturaBD.listarFormasPago();
            foreach (DataRow fila in formas.Rows)
            {
                cmbMetodoPago.Items.Add(new ItemLista(Convert.ToInt32(fila["forma_pago_id"]), "" + fila["nombre_forma_pago"], ""));
            }

            foreach (int cuotas in facturaBD.opcionesCuotas())
            {
                cmbCuotas.Items.Add(cuotas);
            }

            cmbTipoPago.SelectedIndex = 0;
            cmbCuotas.SelectedIndex = 4;
        }

        /// <summary>
        /// Carga orden.
        /// </summary>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        private bool cargarOrden()
        {
            DataTable orden = facturaBD.obtenerOrdenFactura(ordenId);

            string problema = facturaBD.validarOrdenFacturable(orden);
            if (problema != "")
            {
                aviso(problema);
                return false;
            }

            DataRow o = orden.Rows[0];

            dFecha.Text = DateTime.Today.ToString("dd/MM/yyyy");
            dOrden.Text = ordenId;
            dCotizacion.Text = vacio("" + o["cotizacion_id"]);
            dServicio.Text = vacio("" + o["nombre_servicio"]);
            dFechaServicio.Text = "—";
            if (o["fecha_servicio"] is DateTime fechaServicio)
                dFechaServicio.Text = fechaServicio.ToString("dd/MM/yyyy");

            dRtn.Text = vacio("" + o["identificacion_cliente"]);
            dEmpresa.Text = vacio("" + o["nombre_empresa"]);
            dContacto.Text = vacio((("" + o["nombre_cliente"]) + " " + ("" + o["apellido_cliente"])).Trim());
            dTelefono.Text = vacio(Validacion.formatoTelefono("" + o["telefono_cliente"]));
            dCorreo.Text = vacio("" + o["correo_cliente"]);

            dLocacion.Text = vacio("" + o["nombre_locacion"]);
            dTipoLocacion.Text = vacio("" + o["tipo_locacion"]);
            dDireccion.Text = vacio("" + o["direccion"]);

            DataTable lineas = facturaBD.detalleOrden(ordenId);
            dgLineas.ItemsSource = lineas.DefaultView;

            decimal subtotal = 0;
            foreach (DataRow linea in lineas.Rows)
            {
                subtotal += Convert.ToDecimal(linea["subtotal"]);
            }

            decimal isv = facturaBD.calcularIsv(subtotal);
            total = subtotal + isv;

            lblSubtotal.Text = facturaBD.dinero(subtotal);
            lblIsv.Text = facturaBD.dinero(isv);
            lblTotal.Text = facturaBD.dinero(total);

            return true;
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

        // ---------- Forma de pago ----------

        /// <summary>
        /// Atiende el cambio de selección de pago.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void pagoSeleccionCambiada(object sender, SelectionChangedEventArgs e)
        {
            actualizarValorCuota();
        }

        /// <summary>
        /// Actualiza valor cuota.
        /// </summary>
        private void actualizarValorCuota()
        {
            if (cmbTipoPago.SelectedItem == null || cmbCuotas.SelectedItem == null)
                return;

            bool credito = ("" + cmbTipoPago.SelectedItem) == Facturacion.CREDITO;
            cmbCuotas.IsEnabled = credito;

            if (!credito)
            {
                lblValorCuota.Text = "1 pago de " + facturaBD.dinero(total);
                return;
            }

            int cuotas = Convert.ToInt32(cmbCuotas.SelectedItem);
            lblValorCuota.Text = cuotas + " x " + facturaBD.dinero(facturaBD.calcularCuota(total, cuotas));
        }

        /// <summary>
        /// Valida y guarda los datos del formulario.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void guardarClic(object sender, RoutedEventArgs e)
        {
            ItemLista metodo = cmbMetodoPago.SelectedItem as ItemLista;

            DatosFactura datos = new DatosFactura();
            datos.ordenId = ordenId;
            datos.tipoPago = "" + cmbTipoPago.SelectedItem;

            if (metodo != null)
                datos.formaPagoId = metodo.id;

            if (cmbCuotas.SelectedItem != null)
                datos.cuotas = Convert.ToInt32(cmbCuotas.SelectedItem);

            try
            {
                string mensaje;
                int facturaId;

                bool guardada = facturaBD.guardarFactura(datos, out mensaje, out facturaId);

                MensajeCodelex.mostrar(this, "Facturas", mensaje, guardada);

                if (guardada)
                    Close();
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
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
            MensajeCodelex.mostrar(this, "Facturas", mensaje, false);
        }

        /// <summary>
        /// Muestra un mensaje con el error ocurrido.
        /// </summary>
        /// <param name="ex">Excepción que se produjo.</param>
        private void mostrarError(Exception ex)
        {
            MensajeCodelex.mostrar(this, "Facturas", ex.Message, false);
        }
    }
}
