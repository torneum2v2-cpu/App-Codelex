using Codelex.AccesoDatos;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace Codelex.FacturasCotizaciones
{
    /// <summary>
    /// Lógica de interacción para registrar-abono.xaml
    /// El abono cubre primero las cuotas más antiguas; aquí se ve cómo quedarían antes de confirmar.
    /// </summary>
    public partial class RegistrarAbono : Window
    {
        Facturacion facturaBD = new Facturacion();

        int facturaId;
        decimal saldo;

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        /// <param name="facturaId">Identificador de factura.</param>
        /// <param name="saldo">Saldo pendiente en lempiras.</param>
        public RegistrarAbono(int facturaId, decimal saldo)
        {
            InitializeComponent();

            this.facturaId = facturaId;
            this.saldo = saldo;

            txtMonto.MaxLength = 12;
            lblSaldo.Text = facturaBD.dinero(saldo);
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
                txtMonto.Focus();
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        /// <summary>
        /// Atiende el clic de simular.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
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

        /// <summary>
        /// Atiende la tecla presionada en monto.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void montoTeclaPresionada(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                simularClic(sender, e);
        }

        /// <summary>
        /// Atiende el clic de registrar.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
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
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Muestra un mensaje de aviso al usuario.
        /// </summary>
        /// <param name="mensaje">Mensaje con el resultado.</param>
        private void aviso(string mensaje)
        {
            MensajeCodelex.mostrar(this, "Abonos", mensaje, false);
        }

        /// <summary>
        /// Muestra un mensaje con el error ocurrido.
        /// </summary>
        /// <param name="ex">Excepción que se produjo.</param>
        private void mostrarError(Exception ex)
        {
            MensajeCodelex.mostrar(this, "Abonos", ex.Message, false);
        }
    }
}
