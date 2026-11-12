using Codelex.AccesoDatos;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class ElegirEmpleado : Window
    {
        UsuarioBD usuarioBD = new UsuarioBD();

        public DataRowView? empleadoElegido = null;

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        public ElegirEmpleado()
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
            cargarEmpleados();
            txtBuscar.Focus();
        }

        /// <summary>
        /// Carga empleados.
        /// </summary>
        private void cargarEmpleados()
        {
            try
            {
                DataTable tabla = usuarioBD.buscarEmpleadosSinUsuario(txtBuscar.Text);
                dgEmpleados.ItemsSource = tabla.DefaultView;
                txtResultado.Text = "Empleados disponibles (" + tabla.Rows.Count + ")";
            }
            catch (Exception ex)
            {
                MensajeCodelex.mostrar(this, "Buscar empleado", ex.Message, false);
            }
        }

        /// <summary>
        /// Confirma el empleado seleccionado y cierra el cuadro.
        /// </summary>
        private void elegir()
        {
            if (dgEmpleados.SelectedItem == null)
            {
                MensajeCodelex.mostrar(this, "Buscar empleado", "Seleccione un empleado de la tabla.", true);
                return;
            }

            empleadoElegido = (DataRowView)dgEmpleados.SelectedItem;
            DialogResult = true;
        }

        /// <summary>
        /// Busca con el texto escrito.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void buscarClic(object sender, RoutedEventArgs e)
        {
            cargarEmpleados();
        }

        /// <summary>
        /// Ejecuta la búsqueda al presionar Enter.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void buscarTeclaPresionada(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                cargarEmpleados();
        }

        /// <summary>
        /// Confirma el elemento seleccionado.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void seleccionarClic(object sender, RoutedEventArgs e)
        {
            elegir();
        }

        /// <summary>
        /// Atiende el doble clic en empleados.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void empleadosDobleClic(object sender, MouseButtonEventArgs e)
        {
            elegir();
        }

        /// <summary>
        /// Cancela lo que se está haciendo y cierra o limpia el formulario.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void cancelarClic(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
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
    }
}
