using Codelex.AccesoDatos;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class RegistrarVehiculo : Window
    {
        Vehiculo vehiculoBD = new Vehiculo();

        public RegistrarVehiculo()
        {
            InitializeComponent();
        }

        private void ventanaCargada(object sender, RoutedEventArgs e)
        {
            limpiarFormulario();
        }

        private void limpiarFormulario()
        {
            txtPlaca.Text = "";
            txtMarca.Text = "";
            txtModelo.Text = "";
            txtPlaca.Focus();
        }

        private void soloPlaca(object sender, TextCompositionEventArgs e)
        {
            if (!vehiculoBD.caracteresDePlaca(e.Text))
                e.Handled = true;
        }

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

        private void limpiarClic(object sender, RoutedEventArgs e)
        {
            limpiarFormulario();
        }

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
            Close();
        }
    }
}
