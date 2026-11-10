using Codelex.Clases_BDD;
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

        public Inventario()
        {
            InitializeComponent();
        }

        private void ventanaCargada(object sender, RoutedEventArgs e)
        {
            cargarInventario();
            txtBuscar.Focus();
        }

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

        private void buscarTexto()
        {
            textoBusqueda = txtBuscar.Text.Trim();
            cargarInventario();
        }

        private void buscarClic(object sender, RoutedEventArgs e)
        {
            buscarTexto();
        }

        private void buscarTeclaPresionada(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                buscarTexto();
        }

        private void mostrarTodosClic(object sender, RoutedEventArgs e)
        {
            txtBuscar.Text = "";
            textoBusqueda = "";
            filtroTipo = "";
            cargarInventario();
        }

        private void productosClic(object sender, RoutedEventArgs e)
        {
            filtroTipo = "PRODUCTO";
            cargarInventario();
        }

        private void equiposClic(object sender, RoutedEventArgs e)
        {
            filtroTipo = "EQUIPO";
            cargarInventario();
        }

        private void agregarClic(object sender, RoutedEventArgs e)
        {
            AgregarInventario ventana = new AgregarInventario();
            ventana.Owner = this;
            ventana.ShowDialog();

            cargarInventario();
        }

        private void arrastrarVentana(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void minimizarClic(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void salirClic(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
