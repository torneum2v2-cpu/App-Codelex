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

        private void ventanaCargada(object sender, RoutedEventArgs e)
        {
            lista = inventarioBD.crearLista();
            dgLista.ItemsSource = lista.DefaultView;
            dpFecha.SelectedDate = DateTime.Today;

            try
            {
                cargarProveedores(0);
                llenarCombo(cmbEmpleado, inventarioBD.listarEmpleados(), "empleado_id", "nombre_completo");
                cargarArticulos();
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }

            actualizarTotal();
        }

        private void cargarProveedores(int seleccionar)
        {
            llenarCombo(cmbProveedor, inventarioBD.listarProveedores(), "proveedor_id", "nombre_proveedor");

            foreach (ItemLista item in cmbProveedor.Items)
            {
                if (item.id == seleccionar)
                    cmbProveedor.SelectedItem = item;
            }
        }

        private void llenarCombo(ComboBox combo, DataTable tabla, string columnaId, string columnaTexto)
        {
            combo.Items.Clear();

            foreach (DataRow fila in tabla.Rows)
            {
                combo.Items.Add(new ItemLista(Convert.ToInt32(fila[columnaId]), "" + fila[columnaTexto], ""));
            }
        }

        private int idSeleccionado(ComboBox combo)
        {
            ItemLista? item = combo.SelectedItem as ItemLista;

            if (item == null)
                return 0;

            return item.id;
        }

        private void cargarArticulos()
        {
            dgProductos.ItemsSource = inventarioBD.listarProductos(textoBusqueda).DefaultView;

            tablaEquipos = inventarioBD.listarEquipos(textoBusqueda);

            if (equiposNuevos != null)
            {
                foreach (DataRow fila in equiposNuevos.Rows)
                {
                    tablaEquipos.ImportRow(fila);
                }
            }

            dgEquipos.ItemsSource = tablaEquipos.DefaultView;
        }

        private void actualizarTotal()
        {
            txtTotal.Text = "Total a ingresar: " + inventarioBD.totalUnidades(lista) + " unidades";
        }

        private void seleccionar(DataGrid tabla, string columna, string valor)
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

        private void pestanaMarcado(object sender, RoutedEventArgs e)
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

        private void buscarTexto()
        {
            textoBusqueda = txtBuscar.Text.Trim();

            try
            {
                cargarArticulos();
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
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

        private void soloNumeros(object sender, TextCompositionEventArgs e)
        {
            if (!inventarioBD.soloNumeros(e.Text))
                e.Handled = true;
        }

        private void soloPrecio(object sender, TextCompositionEventArgs e)
        {
            if (!inventarioBD.caracteresDePrecio(e.Text))
                e.Handled = true;
        }

        private void agregarAListaClic(object sender, RoutedEventArgs e)
        {
            string mensaje;
            bool agregado;

            if (rbProductos.IsChecked == true)
            {
                DataRowView? producto = dgProductos.SelectedItem as DataRowView;

                if (producto == null)
                {
                    MensajeCodelex.mostrar(this, "Inventario", "Seleccione un producto de la tabla.", false);
                    return;
                }

                agregado = inventarioBD.agregarProductoALista(lista, Convert.ToInt32(producto["producto_id"]),
                    "" + producto["nombre_producto"], "" + producto["tipo_producto"],
                    txtCantidad.Text, txtPrecio.Text, out mensaje);
            }
            else
            {
                DataRowView? equipo = dgEquipos.SelectedItem as DataRowView;

                if (equipo == null)
                {
                    MensajeCodelex.mostrar(this, "Inventario", "Seleccione un equipo de la tabla.", false);
                    return;
                }

                agregado = inventarioBD.agregarEquipoALista(lista, "" + equipo["nombre_equipo"], "" + equipo["tipo_equipo"],
                    "" + equipo["marca_equipo"], "" + equipo["modelo_equipo"],
                    txtCantidad.Text, txtPrecio.Text, out mensaje);
            }

            if (!agregado)
            {
                MensajeCodelex.mostrar(this, "Inventario", mensaje, false);
                return;
            }

            txtCantidad.Text = "";
            txtPrecio.Text = "";
            actualizarTotal();
        }

        private void nuevoArticuloClic(object sender, RoutedEventArgs e)
        {
            NuevoProductoEquipo ventana = new NuevoProductoEquipo();
            ventana.Owner = this;
            ventana.ShowDialog();

            if (!ventana.registrado)
                return;

            try
            {
                if (ventana.esEquipo)
                {
                    if (equiposNuevos == null)
                        equiposNuevos = tablaEquipos.Clone();

                    equiposNuevos.Rows.Add(ventana.nombre, ventana.tipo, ventana.marca, ventana.modelo, 0);

                    cargarArticulos();
                    rbEquipos.IsChecked = true;
                    dgEquipos.SelectedItem = dgEquipos.Items[dgEquipos.Items.Count - 1];
                    dgEquipos.ScrollIntoView(dgEquipos.SelectedItem);
                }
                else
                {
                    cargarArticulos();
                    rbProductos.IsChecked = true;
                    seleccionar(dgProductos, "producto_id", "" + ventana.productoId);
                }

                txtCantidad.Focus();
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void nuevoProveedorClic(object sender, RoutedEventArgs e)
        {
            NuevoProveedor ventana = new NuevoProveedor();
            ventana.Owner = this;
            ventana.ShowDialog();

            if (ventana.proveedorId == 0)
                return;

            try
            {
                cargarProveedores(ventana.proveedorId);
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void quitarClic(object sender, RoutedEventArgs e)
        {
            DataRowView? fila = dgLista.SelectedItem as DataRowView;

            if (fila == null)
            {
                MensajeCodelex.mostrar(this, "Inventario", "Seleccione un elemento de la lista a ingresar.", false);
                return;
            }

            lista.Rows.Remove(fila.Row);
            actualizarTotal();
        }

        private void vaciarClic(object sender, RoutedEventArgs e)
        {
            lista.Rows.Clear();
            actualizarTotal();
        }

        private void guardarClic(object sender, RoutedEventArgs e)
        {
            try
            {
                string mensaje;

                bool registrado = inventarioBD.registrarIngreso(idSeleccionado(cmbProveedor), idSeleccionado(cmbEmpleado),
                    txtFactura.Text, dpFecha.SelectedDate, lista, out mensaje);

                MensajeCodelex.mostrar(this, "Inventario", mensaje, registrado);

                if (registrado)
                {
                    guardado = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void ventanaCerrando(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (guardado || lista.Rows.Count == 0)
                return;

            MessageBoxResult respuesta = MessageBox.Show("La lista a ingresar no se ha guardado. ¿Desea salir de todos modos?",
                "Inventario", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (respuesta != MessageBoxResult.Yes)
                e.Cancel = true;
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

        private void mostrarError(Exception ex)
        {
            MensajeCodelex.mostrar(this, "Inventario", ex.Message, false);
        }
    }
}
