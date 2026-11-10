using Codelex.AccesoDatos;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class NuevoProveedor : Window
    {
        InventarioBD inventarioBD = new InventarioBD();

        public int proveedorId = 0;

        public NuevoProveedor()
        {
            InitializeComponent();
        }

        private void ventanaCargada(object sender, RoutedEventArgs e)
        {
            limpiarFormulario();
        }

        private void limpiarFormulario()
        {
            txtNombre.Text = "";
            txtTelefono.Text = "";
            txtCorreo.Text = "";
            txtNombre.Focus();
        }

        private void soloNumeros(object sender, TextCompositionEventArgs e)
        {
            if (!inventarioBD.soloNumeros(e.Text))
                e.Handled = true;
        }

        private void guardarClic(object sender, RoutedEventArgs e)
        {
            try
            {
                string mensaje;
                int proveedorId;

                bool guardado = inventarioBD.insertarProveedor(txtNombre.Text, txtTelefono.Text, txtCorreo.Text,
                                                               out mensaje, out proveedorId);

                MensajeCodelex.mostrar(this, "Inventario", mensaje, guardado);

                if (guardado)
                {
                    proveedorId = proveedorId;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MensajeCodelex.mostrar(this, "Inventario", ex.Message, false);
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

        private void salirClic(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
