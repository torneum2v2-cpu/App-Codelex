using Codelex.Clases_BDD;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace Codelex.Facturas_y_Cotizaciones
{
    /// <summary>
    /// Lógica de interacción para RegistrarAbono.xaml
    /// El abono cubre primero las cuotas más antiguas; aquí se ve cómo quedarían antes de confirmar.
    /// </summary>
    public partial class RegistrarAbono : Window
    {
        Facturacion facturaBD = new Facturacion();

        int facturaId;
        decimal saldo;

        public RegistrarAbono(int facturaId, decimal saldo)
        {
            InitializeComponent();

            this.facturaId = facturaId;
            this.saldo = saldo;

            txtMonto.MaxLength = 12;
            lblSaldo.Text = facturaBD.Dinero(saldo);
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable formas = facturaBD.ListarFormasPago();
                foreach (DataRow fila in formas.Rows)
                {
                    cmbMetodoPago.Items.Add(new ItemLista(Convert.ToInt32(fila["forma_pago_id"]), "" + fila["nombreFormaPago"], ""));
                }

                cmbMetodoPago.SelectedIndex = 0;
                txtMonto.Focus();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void btnSimular_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string mensaje;
                DataTable cuotas = facturaBD.SimularAbono(facturaId, txtMonto.Text, saldo, out mensaje);

                dgSimulacion.ItemsSource = cuotas.DefaultView;

                if (mensaje != "")
                    Aviso(mensaje);
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void txtMonto_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                btnSimular_Click(sender, e);
        }

        private void btnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            ItemLista metodo = cmbMetodoPago.SelectedItem as ItemLista;
            int formaPagoId = 0;

            if (metodo != null)
                formaPagoId = metodo.Id;

            try
            {
                string mensaje;
                bool registrado = facturaBD.RegistrarAbono(facturaId, txtMonto.Text, saldo, formaPagoId, out mensaje);

                MensajeCodelex.Mostrar(this, "Abonos", mensaje, registrado);

                if (registrado)
                {
                    DialogResult = true;
                    Close();
                }
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
            DialogResult = false;
            Close();
        }

        private void Aviso(string mensaje)
        {
            MensajeCodelex.Mostrar(this, "Abonos", mensaje, false);
        }

        private void MostrarError(Exception ex)
        {
            MensajeCodelex.Mostrar(this, "Abonos", ex.Message, false);
        }
    }
}
