using Codelex.Clases_BDD;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Codelex
{
    public partial class AgregarInventario : Window
    {
        InventarioBD inventarioBD = new InventarioBD();

        DataTable lista = new DataTable();
        DataTable tablaEquipos = new DataTable();
        DataTable? equiposNuevos = null;
        string textoBusqueda = "";
        bool guardado = false;

        public AgregarInventario()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lista = inventarioBD.CrearLista();
            dgLista.ItemsSource = lista.DefaultView;
            dpFecha.SelectedDate = DateTime.Today;

            try
            {
                CargarProveedores(0);
                LlenarCombo(cmbEmpleado, inventarioBD.ListarEmpleados(), "empleado_id", "nombreCompleto");
                CargarArticulos();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }

            ActualizarTotal();
        }

        private void CargarProveedores(int seleccionar)
        {
            LlenarCombo(cmbProveedor, inventarioBD.ListarProveedores(), "proveedor_id", "nombreProveedor");

            foreach (ItemLista item in cmbProveedor.Items)
            {
                if (item.Id == seleccionar)
                    cmbProveedor.SelectedItem = item;
            }
        }

        private void LlenarCombo(ComboBox combo, DataTable tabla, string columnaId, string columnaTexto)
        {
            combo.Items.Clear();

            foreach (DataRow fila in tabla.Rows)
            {
                combo.Items.Add(new ItemLista(Convert.ToInt32(fila[columnaId]), "" + fila[columnaTexto], ""));
            }
        }

        private int IdSeleccionado(ComboBox combo)
        {
            ItemLista? item = combo.SelectedItem as ItemLista;

            if (item == null)
                return 0;

            return item.Id;
        }

        private void CargarArticulos()
        {
            dgProductos.ItemsSource = inventarioBD.ListarProductos(textoBusqueda).DefaultView;

            tablaEquipos = inventarioBD.ListarEquipos(textoBusqueda);

            if (equiposNuevos != null)
            {
                foreach (DataRow fila in equiposNuevos.Rows)
                {
                    tablaEquipos.ImportRow(fila);
                }
            }

            dgEquipos.ItemsSource = tablaEquipos.DefaultView;
        }

        private void ActualizarTotal()
        {
            txtTotal.Text = "Total a ingresar: " + inventarioBD.TotalUnidades(lista) + " unidades";
        }

        private void Seleccionar(DataGrid tabla, string columna, string valor)
        {
            foreach (object item in tabla.Items)
            {
                DataRowView fila = (DataRowView)item;

                if ("" + fila[columna] == valor)
                {
                    tabla.SelectedItem = fila;
                    tabla.ScrollIntoView(fila);
                }
            }
        }

        private void Pestana_Checked(object sender, RoutedEventArgs e)
        {
            if (dgProductos == null || dgEquipos == null)
                return;

            if (rbProductos.IsChecked == true)
            {
                dgProductos.Visibility = Visibility.Visible;
                dgEquipos.Visibility = Visibility.Collapsed;
            }
            else
            {
                dgProductos.Visibility = Visibility.Collapsed;
                dgEquipos.Visibility = Visibility.Visible;
            }
        }

        private void BuscarTexto()
        {
            textoBusqueda = txtBuscar.Text.Trim();

            try
            {
                CargarArticulos();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
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

        private void SoloNumeros(object sender, TextCompositionEventArgs e)
        {
            if (!inventarioBD.SoloNumeros(e.Text))
                e.Handled = true;
        }

        private void SoloPrecio(object sender, TextCompositionEventArgs e)
        {
            if (!inventarioBD.CaracteresDePrecio(e.Text))
                e.Handled = true;
        }

        private void AgregarALista_Click(object sender, RoutedEventArgs e)
        {
            string mensaje;
            bool agregado;

            if (rbProductos.IsChecked == true)
            {
                DataRowView? producto = dgProductos.SelectedItem as DataRowView;

                if (producto == null)
                {
                    MensajeCodelex.Mostrar(this, "Inventario", "Seleccione un producto de la tabla.", false);
                    return;
                }

                agregado = inventarioBD.AgregarProductoALista(lista, Convert.ToInt32(producto["producto_id"]),
                    "" + producto["nombreProducto"], "" + producto["tipoProducto"],
                    txtCantidad.Text, txtPrecio.Text, out mensaje);
            }
            else
            {
                DataRowView? equipo = dgEquipos.SelectedItem as DataRowView;

                if (equipo == null)
                {
                    MensajeCodelex.Mostrar(this, "Inventario", "Seleccione un equipo de la tabla.", false);
                    return;
                }

                agregado = inventarioBD.AgregarEquipoALista(lista, "" + equipo["nombreEquipo"], "" + equipo["tipoEquipo"],
                    "" + equipo["marcaEquipo"], "" + equipo["modeloEquipo"],
                    txtCantidad.Text, txtPrecio.Text, out mensaje);
            }

            if (!agregado)
            {
                MensajeCodelex.Mostrar(this, "Inventario", mensaje, false);
                return;
            }

            txtCantidad.Text = "";
            txtPrecio.Text = "";
            ActualizarTotal();
        }

        private void NuevoArticulo_Click(object sender, RoutedEventArgs e)
        {
            NuevoProductoEquipo ventana = new NuevoProductoEquipo();
            ventana.Owner = this;
            ventana.ShowDialog();

            if (!ventana.Registrado)
                return;

            try
            {
                if (ventana.EsEquipo)
                {
                    if (equiposNuevos == null)
                        equiposNuevos = tablaEquipos.Clone();

                    equiposNuevos.Rows.Add(ventana.Nombre, ventana.Tipo, ventana.Marca, ventana.Modelo, 0);

                    CargarArticulos();
                    rbEquipos.IsChecked = true;
                    dgEquipos.SelectedItem = dgEquipos.Items[dgEquipos.Items.Count - 1];
                    dgEquipos.ScrollIntoView(dgEquipos.SelectedItem);
                }
                else
                {
                    CargarArticulos();
                    rbProductos.IsChecked = true;
                    Seleccionar(dgProductos, "producto_id", "" + ventana.ProductoId);
                }

                txtCantidad.Focus();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void NuevoProveedor_Click(object sender, RoutedEventArgs e)
        {
            NuevoProveedor ventana = new NuevoProveedor();
            ventana.Owner = this;
            ventana.ShowDialog();

            if (ventana.ProveedorId == 0)
                return;

            try
            {
                CargarProveedores(ventana.ProveedorId);
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void Quitar_Click(object sender, RoutedEventArgs e)
        {
            DataRowView? fila = dgLista.SelectedItem as DataRowView;

            if (fila == null)
            {
                MensajeCodelex.Mostrar(this, "Inventario", "Seleccione un elemento de la lista a ingresar.", false);
                return;
            }

            lista.Rows.Remove(fila.Row);
            ActualizarTotal();
        }

        private void Vaciar_Click(object sender, RoutedEventArgs e)
        {
            lista.Rows.Clear();
            ActualizarTotal();
        }

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string mensaje;

                bool registrado = inventarioBD.RegistrarIngreso(IdSeleccionado(cmbProveedor), IdSeleccionado(cmbEmpleado),
                    txtFactura.Text, dpFecha.SelectedDate, lista, out mensaje);

                MensajeCodelex.Mostrar(this, "Inventario", mensaje, registrado);

                if (registrado)
                {
                    guardado = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (guardado || lista.Rows.Count == 0)
                return;

            MessageBoxResult respuesta = MessageBox.Show("La lista a ingresar no se ha guardado. ¿Desea salir de todos modos?",
                "Inventario", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (respuesta != MessageBoxResult.Yes)
                e.Cancel = true;
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

        private void MostrarError(Exception ex)
        {
            MensajeCodelex.Mostrar(this, "Inventario", ex.Message, false);
        }
    }
}
