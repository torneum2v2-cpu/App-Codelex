using Codelex.Clases_BDD;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class NuevoProveedor : Window
    {
        InventarioBD inventarioBD = new InventarioBD();

        public int ProveedorId = 0;

        public NuevoProveedor()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            txtNombre.Text = "";
            txtTelefono.Text = "";
            txtCorreo.Text = "";
            txtNombre.Focus();
        }

        private void SoloNumeros(object sender, TextCompositionEventArgs e)
        {
            if (!inventarioBD.SoloNumeros(e.Text))
                e.Handled = true;
        }

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string mensaje;
                int proveedorId;

                bool guardado = inventarioBD.InsertarProveedor(txtNombre.Text, txtTelefono.Text, txtCorreo.Text,
                                                               out mensaje, out proveedorId);

                MensajeCodelex.Mostrar(this, "Inventario", mensaje, guardado);

                if (guardado)
                {
                    ProveedorId = proveedorId;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MensajeCodelex.Mostrar(this, "Inventario", ex.Message, false);
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

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
