using Codelex.AccesoDatos;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class Inventario : Window
    {
        InventarioBD inventarioBD = new InventarioBD();

        string textoBusqueda = "";
        string filtroTipo = "";

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        public Inventario()
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
            cargarInventario();
            txtBuscar.Focus();
        }

        /// <summary>
        /// Carga inventario.
        /// </summary>
        private void cargarInventario()
        {
            try
            {
                DataTable tabla = inventarioBD.buscarInventario(textoBusqueda, filtroTipo);
                dgInventario.ItemsSource = tabla.DefaultView;
                txtValorTotal.Text = "Valor del inventario: L " + inventarioBD.valorTotal(tabla).ToString("N2");
            }
            catch (Exception ex)
            {
                MensajeCodelex.mostrar(this, "Inventario", ex.Message, false);
            }
        }

        /// <summary>
        /// Busca texto.
        /// </summary>
        private void buscarTexto()
        {
            textoBusqueda = txtBuscar.Text.Trim();
            cargarInventario();
        }

        /// <summary>
        /// Busca con el texto escrito.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void buscarClic(object sender, RoutedEventArgs e)
        {
            buscarTexto();
        }

        /// <summary>
        /// Ejecuta la búsqueda al presionar Enter.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void buscarTeclaPresionada(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                buscarTexto();
        }

        /// <summary>
        /// Quita los filtros y muestra todos los registros.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void mostrarTodosClic(object sender, RoutedEventArgs e)
        {
            txtBuscar.Text = "";
            textoBusqueda = "";
            filtroTipo = "";
            cargarInventario();
        }

        /// <summary>
        /// Atiende el clic de productos.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void productosClic(object sender, RoutedEventArgs e)
        {
            filtroTipo = "PRODUCTO";
            cargarInventario();
        }

        /// <summary>
        /// Atiende el clic de equipos.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void equiposClic(object sender, RoutedEventArgs e)
        {
            filtroTipo = "EQUIPO";
            cargarInventario();
        }

        /// <summary>
        /// Atiende el clic de agregar.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void agregarClic(object sender, RoutedEventArgs e)
        {
            AgregarInventario ventana = new AgregarInventario();
            ventana.Owner = this;
            ventana.ShowDialog();

            cargarInventario();
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
