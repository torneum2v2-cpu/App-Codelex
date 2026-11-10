using Codelex.AccesoDatos;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class RegistrarVehiculo : Window
    {
        Vehiculo vehiculoBD = new Vehiculo();

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        public RegistrarVehiculo()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Prepara la ventana y carga los datos iniciales cuando termina de abrirse.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void ventanaCargada(object sender, RoutedEventArgs e)
        {
            limpiarFormulario();
        }

        /// <summary>
        /// Limpia formulario.
        /// </summary>
        private void limpiarFormulario()
        {
            txtPlaca.Text = "";
            txtMarca.Text = "";
            txtModelo.Text = "";
            txtPlaca.Focus();
        }

        /// <summary>
        /// Impide escribir caracteres que no se permiten en una placa.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void soloPlaca(object sender, TextCompositionEventArgs e)
        {
            if (!vehiculoBD.caracteresDePlaca(e.Text))
                e.Handled = true;
        }

        /// <summary>
        /// Valida y guarda los datos del formulario.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void guardarClic(object sender, RoutedEventArgs e)
        {
            try
            {
                string mensaje;

                bool guardado = vehiculoBD.insertarVehiculo(txtPlaca.Text, txtMarca.Text, txtModelo.Text, out mensaje);

                MensajeCodelex.mostrar(this, "Vehículos", mensaje, guardado);

                if (guardado)
                {
                    limpiarFormulario();
                }
            }
            catch (Exception ex)
            {
                MensajeCodelex.mostrar(this, "Vehículos", ex.Message, false);
            }
        }

        /// <summary>
        /// Limpia los datos del formulario.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void limpiarClic(object sender, RoutedEventArgs e)
        {
            limpiarFormulario();
        }

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
    }
}
