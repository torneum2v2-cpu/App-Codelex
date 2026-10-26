using Codelex.Clases_BDD;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            txtPlaca.Text = "";
            txtMarca.Text = "";
            txtModelo.Text = "";
            txtPlaca.Focus();
        }

        private void SoloPlaca(object sender, TextCompositionEventArgs e)
        {
            if (!vehiculoBD.CaracteresDePlaca(e.Text))
                e.Handled = true;
        }

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string mensaje;

                bool guardado = vehiculoBD.InsertarVehiculo(txtPlaca.Text, txtMarca.Text, txtModelo.Text, out mensaje);

                MensajeCodelex.Mostrar(this, "Vehículos", mensaje, guardado);

                if (guardado)
                {
                    LimpiarFormulario();
                }
            }
            catch (Exception ex)
            {
                MensajeCodelex.Mostrar(this, "Vehículos", ex.Message, false);
            }
        }

        private void Limpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

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
    }
}
