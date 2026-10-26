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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarInventario();
            txtBuscar.Focus();
        }

        private void CargarInventario()
        {
            try
            {
                DataTable tabla = inventarioBD.BuscarInventario(textoBusqueda, filtroTipo);
                dgInventario.ItemsSource = tabla.DefaultView;
                txtValorTotal.Text = "Valor del inventario: L " + inventarioBD.ValorTotal(tabla).ToString("N2");
            }
            catch (Exception ex)
            {
                MensajeCodelex.Mostrar(this, "Inventario", ex.Message, false);
            }
        }

        private void BuscarTexto()
        {
            textoBusqueda = txtBuscar.Text.Trim();
            CargarInventario();
        }

        private void Buscar_Click(object sender, RoutedEventArgs e)
        {
            BuscarTexto();
        }

        private void Buscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                BuscarTexto();
        }

        private void MostrarTodos_Click(object sender, RoutedEventArgs e)
        {
            txtBuscar.Text = "";
            textoBusqueda = "";
            filtroTipo = "";
            CargarInventario();
        }

        private void Productos_Click(object sender, RoutedEventArgs e)
        {
            filtroTipo = "PRODUCTO";
            CargarInventario();
        }

        private void Equipos_Click(object sender, RoutedEventArgs e)
        {
            filtroTipo = "EQUIPO";
            CargarInventario();
        }

        private void Agregar_Click(object sender, RoutedEventArgs e)
        {
            AgregarInventario ventana = new AgregarInventario();
            ventana.Owner = this;
            ventana.ShowDialog();

            CargarInventario();
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimizar_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
