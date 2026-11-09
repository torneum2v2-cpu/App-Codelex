using Codelex.Clases_BDD;
using Codelex.Facturas_y_Cotizaciones;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class SeleccionarCotizacion : Window
    {
        Orden ordenBD = new Orden();

        string rtnCliente = "";

        public DataRowView? CotizacionElegida = null;

        public SeleccionarCotizacion(string rtn)
        {
            InitializeComponent();
            rtnCliente = rtn;
        }

        public static DataRow? Elegir(Window dueno, string rtn)
        {
            SeleccionarCotizacion ventana = new SeleccionarCotizacion(rtn);
            ventana.Owner = dueno;

            if (ventana.ShowDialog() == true && ventana.CotizacionElegida != null)
                return ventana.CotizacionElegida.Row;

            return null;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarCotizaciones();
            txtBuscar.Focus();
        }

        private void CargarCotizaciones()
        {
            try
            {
                DataTable tabla = ordenBD.ListarCotizaciones(txtBuscar.Text, rtnCliente);
                dgCotizaciones.ItemsSource = tabla.DefaultView;
                txtResultado.Text = "Cotizaciones pendientes (" + tabla.Rows.Count + ")";
            }
            catch (Exception ex)
            {
                MensajeCodelex.Mostrar(this, "Seleccionar cotización", ex.Message, false);
            }
        }

        private void Elegir()
        {
            if (dgCotizaciones.SelectedItem == null)
            {
                MensajeCodelex.Mostrar(this, "Seleccionar cotización", "Seleccione una cotización de la tabla.", false);
                return;
            }

            DataRowView fila = (DataRowView)dgCotizaciones.SelectedItem;
            string motivo = "" + fila["motivo"];

            if (motivo != "")
            {
                MensajeCodelex.Mostrar(this, "Seleccionar cotización", motivo, false);
                return;
            }

            CotizacionElegida = fila;
            DialogResult = true;
        }

        private void AgregarCotizacion_Click(object sender, RoutedEventArgs e)
        {
            Cotizacion ventana = new Cotizacion();
            ventana.Owner = this;
            ventana.ShowDialog();

            CargarCotizaciones();
        }

        private void Buscar_Click(object sender, RoutedEventArgs e)
        {
            CargarCotizaciones();
        }

        private void Buscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                CargarCotizaciones();
        }

        private void Seleccionar_Click(object sender, RoutedEventArgs e)
        {
            Elegir();
        }

        private void dgCotizaciones_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            Elegir();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
