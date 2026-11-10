using Codelex.AccesoDatos;
using Codelex.FacturasCotizaciones;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class SeleccionarCotizacion : Window
    {
        Orden ordenBD = new Orden();

        string rtnCliente = "";

        public DataRowView? cotizacionElegida = null;

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        /// <param name="rtn">RTN o identidad del cliente (sin guiones).</param>
        public SeleccionarCotizacion(string rtn)
        {
            InitializeComponent();
            rtnCliente = rtn;
        }

        /// <summary>
        /// Abre el cuadro de cotizaciones y devuelve la que se eligió, o null si se cancela.
        /// </summary>
        /// <param name="dueno">Ventana dueña del cuadro.</param>
        /// <param name="rtn">RTN o identidad del cliente (sin guiones).</param>
        /// <returns>Fila de la cotización elegida, o null si se canceló.</returns>
        public static DataRow? elegir(Window dueno, string rtn)
        {
            SeleccionarCotizacion ventana = new SeleccionarCotizacion(rtn);
            ventana.Owner = dueno;

            if (ventana.ShowDialog() == true && ventana.cotizacionElegida != null)
                return ventana.cotizacionElegida.Row;

            return null;
        }

        /// <summary>
        /// Prepara la ventana y carga los datos iniciales cuando termina de abrirse.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void ventanaCargada(object sender, RoutedEventArgs e)
        {
            cargarCotizaciones();
            txtBuscar.Focus();
        }

        /// <summary>
        /// Carga cotizaciones.
        /// </summary>
        private void cargarCotizaciones()
        {
            try
            {
                DataTable tabla = ordenBD.listarCotizaciones(txtBuscar.Text, rtnCliente);
                dgCotizaciones.ItemsSource = tabla.DefaultView;
                txtResultado.Text = "Cotizaciones pendientes (" + tabla.Rows.Count + ")";
            }
            catch (Exception ex)
            {
                MensajeCodelex.mostrar(this, "Seleccionar cotización", ex.Message, false);
            }
        }

        /// <summary>
        /// Confirma la cotización seleccionada y cierra el cuadro.
        /// </summary>
        private void elegir()
        {
            if (dgCotizaciones.SelectedItem == null)
            {
                MensajeCodelex.mostrar(this, "Seleccionar cotización", "Seleccione una cotización de la tabla.", false);
                return;
            }

            DataRowView fila = (DataRowView)dgCotizaciones.SelectedItem;
            string motivo = "" + fila["motivo"];

            if (motivo != "")
            {
                MensajeCodelex.mostrar(this, "Seleccionar cotización", motivo, false);
                return;
            }

            cotizacionElegida = fila;
            DialogResult = true;
        }

        /// <summary>
        /// Atiende el clic de agregar cotización.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void agregarCotizacionClic(object sender, RoutedEventArgs e)
        {
            Cotizacion ventana = new Cotizacion();
            ventana.Owner = this;
            ventana.ShowDialog();

            cargarCotizaciones();
        }

        /// <summary>
        /// Busca con el texto escrito.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void buscarClic(object sender, RoutedEventArgs e)
        {
            cargarCotizaciones();
        }

        /// <summary>
        /// Ejecuta la búsqueda al presionar Enter.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void buscarTeclaPresionada(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                cargarCotizaciones();
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
        /// Atiende el doble clic en cotizaciones.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void cotizacionesDobleClic(object sender, MouseButtonEventArgs e)
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
