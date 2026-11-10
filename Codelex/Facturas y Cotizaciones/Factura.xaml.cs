using Codelex.Clases_BDD;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Codelex.Facturas_y_Cotizaciones
{
    /// <summary>
    /// Lógica de interacción para Factura.xaml
    /// Todo viene de la orden y su cotización; solo se elige cómo se paga.
    /// </summary>
    public partial class Factura : Window
    {
        Facturacion facturaBD = new Facturacion();

        string ordenId = "";
        decimal total = 0;

        public Factura(string ordenId)
        {
            InitializeComponent();

            this.ordenId = ordenId;

            cmbTipoPago.SelectionChanged += cmbPago_SelectionChanged;
            cmbCuotas.SelectionChanged += cmbPago_SelectionChanged;
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CargarListas();

                if (!CargarOrden())
                {
                    Close();
                    return;
                }

                ActualizarValorCuota();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
                Close();
            }
        }

        private void CargarListas()
        {
            foreach (string tipo in facturaBD.TiposPago())
            {
                cmbTipoPago.Items.Add(tipo);
            }

            DataTable formas = facturaBD.ListarFormasPago();
            foreach (DataRow fila in formas.Rows)
            {
                cmbMetodoPago.Items.Add(new ItemLista(Convert.ToInt32(fila["forma_pago_id"]), "" + fila["nombreFormaPago"], ""));
            }

            foreach (int cuotas in facturaBD.OpcionesCuotas())
            {
                cmbCuotas.Items.Add(cuotas);
            }

            cmbTipoPago.SelectedIndex = 0;
            cmbCuotas.SelectedIndex = 4;
        }

        private bool CargarOrden()
        {
            DataTable orden = facturaBD.ObtenerOrdenFactura(ordenId);

            string problema = facturaBD.ValidarOrdenFacturable(orden);
            if (problema != "")
            {
                Aviso(problema);
                return false;
            }

            DataRow o = orden.Rows[0];

            dFecha.Text = DateTime.Today.ToString("dd/MM/yyyy");
            dOrden.Text = ordenId;
            dCotizacion.Text = Vacio("" + o["cotizacion_id"]);
            dServicio.Text = Vacio("" + o["nombreServicio"]);
            dFechaServicio.Text = "—";
            if (o["fecha_servicio"] is DateTime fechaServicio)
                dFechaServicio.Text = fechaServicio.ToString("dd/MM/yyyy");

            dRtn.Text = Vacio("" + o["identificacionCliente"]);
            dEmpresa.Text = Vacio("" + o["nombreEmpresa"]);
            dContacto.Text = Vacio((("" + o["nombreCliente"]) + " " + ("" + o["apellidoCliente"])).Trim());
            dTelefono.Text = Vacio("" + o["telefonoCliente"]);
            dCorreo.Text = Vacio("" + o["correoCliente"]);

            dLocacion.Text = Vacio("" + o["nombreLocacion"]);
            dTipoLocacion.Text = Vacio("" + o["tipoLocacion"]);
            dDireccion.Text = Vacio("" + o["direccion"]);

            DataTable lineas = facturaBD.DetalleOrden(ordenId);
            dgLineas.ItemsSource = lineas.DefaultView;

            decimal subtotal = 0;
            foreach (DataRow linea in lineas.Rows)
            {
                subtotal += Convert.ToDecimal(linea["subtotal"]);
            }

            decimal isv = facturaBD.CalcularIsv(subtotal);
            total = subtotal + isv;

            lblSubtotal.Text = facturaBD.Dinero(subtotal);
            lblIsv.Text = facturaBD.Dinero(isv);
            lblTotal.Text = facturaBD.Dinero(total);

            return true;
        }

        private static string Vacio(string texto)
        {
            if (texto.Trim() == "")
                return "—";

            return texto;
        }

        // ---------- Forma de pago ----------

        private void cmbPago_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarValorCuota();
        }

        private void ActualizarValorCuota()
        {
            if (cmbTipoPago.SelectedItem == null || cmbCuotas.SelectedItem == null)
                return;

            bool credito = ("" + cmbTipoPago.SelectedItem) == Facturacion.Credito;
            cmbCuotas.IsEnabled = credito;

            if (!credito)
            {
                lblValorCuota.Text = "1 pago de " + facturaBD.Dinero(total);
                return;
            }

            int cuotas = Convert.ToInt32(cmbCuotas.SelectedItem);
            lblValorCuota.Text = cuotas + " x " + facturaBD.Dinero(facturaBD.CalcularCuota(total, cuotas));
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            ItemLista metodo = cmbMetodoPago.SelectedItem as ItemLista;

            DatosFactura datos = new DatosFactura();
            datos.OrdenId = ordenId;
            datos.TipoPago = "" + cmbTipoPago.SelectedItem;

            if (metodo != null)
                datos.FormaPagoId = metodo.Id;

            if (cmbCuotas.SelectedItem != null)
                datos.Cuotas = Convert.ToInt32(cmbCuotas.SelectedItem);

            try
            {
                string mensaje;
                int facturaId;

                bool guardada = facturaBD.GuardarFactura(datos, out mensaje, out facturaId);

                MensajeCodelex.Mostrar(this, "Facturas", mensaje, guardada);

                if (guardada)
                    Close();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
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
