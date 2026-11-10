using Codelex.AccesoDatos;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class NuevoProveedor : Window
    {
        InventarioBD inventarioBD = new InventarioBD();

        public int proveedorId = 0;

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        public NuevoProveedor()
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
            txtNombre.Text = "";
            txtTelefono.Text = "";
            txtCorreo.Text = "";
            txtNombre.Focus();
        }

        /// <summary>
        /// Impide escribir caracteres que no sean números.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void soloNumeros(object sender, TextCompositionEventArgs e)
        {
            if (!inventarioBD.soloNumeros(e.Text))
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
