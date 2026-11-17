using Codelex.AccesoDatos;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class NuevoProveedor : VentanaCodelex
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
        /// Valida y guarda los datos del formulario.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void guardarClic(object sender, RoutedEventArgs e)
        {
            try
            {
                string mensaje;
                int idNuevo;

                bool guardado = inventarioBD.insertarProveedor(txtNombre.Text, txtTelefono.Text, txtCorreo.Text,
                                                               out mensaje, out idNuevo);

                MensajeCodelex.mostrar(this, "Inventario", mensaje, guardado);

                if (guardado)
                {
                    proveedorId = idNuevo;
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

    }
}
