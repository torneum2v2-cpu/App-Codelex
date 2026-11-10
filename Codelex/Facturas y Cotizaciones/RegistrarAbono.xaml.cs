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
            lblSaldo.Text = facturaBD.dinero(saldo);
            Loaded += ventanaCargada;
        }

        private void ventanaCargada(object sender, RoutedEventArgs e)
        {
            try
            {
                txtMonto.Focus();
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void simularClic(object sender, RoutedEventArgs e)
        {
            try
            {
                string mensaje;
                DataTable cuotas = facturaBD.simularAbono(facturaId, txtMonto.Text, saldo, out mensaje);

                dgSimulacion.ItemsSource = cuotas.DefaultView;

                if (mensaje != "")
                    aviso(mensaje);
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void montoTeclaPresionada(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                simularClic(sender, e);
        }

        private void registrarClic(object sender, RoutedEventArgs e)
        {
            try
            {
                string mensaje;
                bool registrado = facturaBD.registrarAbono(facturaId, txtMonto.Text, saldo, out mensaje);

                MensajeCodelex.mostrar(this, "Abonos", mensaje, registrado);

                if (registrado)
                {
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
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
            DialogResult = false;
            Close();
        }

        private void aviso(string mensaje)
        {
            MensajeCodelex.mostrar(this, "Abonos", mensaje, false);
        }

        private void mostrarError(Exception ex)
        {
            MensajeCodelex.mostrar(this, "Abonos", ex.Message, false);
        }
    }
}
