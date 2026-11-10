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

        public SeleccionarCotizacion(string rtn)
        {
            InitializeComponent();
            rtnCliente = rtn;
        }

        public static DataRow? elegir(Window dueno, string rtn)
        {
            SeleccionarCotizacion ventana = new SeleccionarCotizacion(rtn);
            ventana.Owner = dueno;

            if (ventana.ShowDialog() == true && ventana.cotizacionElegida != null)
                return ventana.cotizacionElegida.Row;

            return null;
        }

        private void ventanaCargada(object sender, RoutedEventArgs e)
        {
            cargarCotizaciones();
            txtBuscar.Focus();
        }

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

        private void agregarCotizacionClic(object sender, RoutedEventArgs e)
        {
            Cotizacion ventana = new Cotizacion();
            ventana.Owner = this;
            ventana.ShowDialog();

            cargarCotizaciones();
        }

        private void buscarClic(object sender, RoutedEventArgs e)
        {
            cargarCotizaciones();
        }

        private void buscarTeclaPresionada(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                cargarCotizaciones();
        }

        private void seleccionarClic(object sender, RoutedEventArgs e)
        {
            elegir();
        }

        private void cotizacionesDobleClic(object sender, MouseButtonEventArgs e)
        {
            elegir();
        }

        private void cancelarClic(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void arrastrarVentana(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
