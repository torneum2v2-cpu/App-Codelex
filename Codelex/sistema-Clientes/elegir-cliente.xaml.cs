using Codelex.AccesoDatos;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class ElegirCliente : Window
    {
        Cliente clienteBD = new Cliente();

        public DataRowView? clienteElegido = null;

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        public ElegirCliente()
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
            cargarClientes();
            txtBuscar.Focus();
        }

        /// <summary>
        /// Carga clientes.
        /// </summary>
        private void cargarClientes()
        {
            try
            {
                DataTable tabla = clienteBD.buscarClientes(txtBuscar.Text.Trim(), "", "ACTIVOS");
                dgClientes.ItemsSource = tabla.DefaultView;
                txtResultado.Text = "Clientes activos (" + tabla.Rows.Count + ")";
            }
            catch (Exception ex)
            {
                MensajeCodelex.mostrar(this, "Buscar cliente", ex.Message, false);
            }
        }

        /// <summary>
        /// Confirma el cliente seleccionado y cierra el cuadro.
        /// </summary>
        private void elegir()
        {
            if (dgClientes.SelectedItem == null)
            {
                MensajeCodelex.mostrar(this, "Buscar cliente", "Seleccione un cliente de la tabla.", false);
                return;
            }

            clienteElegido = (DataRowView)dgClientes.SelectedItem;
            DialogResult = true;
        }

        /// <summary>
        /// Busca con el texto escrito.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void buscarClic(object sender, RoutedEventArgs e)
        {
            cargarClientes();
        }

        /// <summary>
        /// Ejecuta la búsqueda al presionar Enter.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void buscarTeclaPresionada(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                cargarClientes();
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
        /// Atiende el doble clic en clientes.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void clientesDobleClic(object sender, MouseButtonEventArgs e)
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
