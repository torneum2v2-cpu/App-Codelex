using Codelex.AccesoDatos;
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

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        public AgregarInventario()
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

        /// <summary>
        /// Carga proveedores.
        /// </summary>
        /// <param name="seleccionar">Seleccionar.</param>
        private void cargarProveedores(int seleccionar)
        {
            llenarCombo(cmbProveedor, inventarioBD.listarProveedores(), "proveedor_id", "nombre_proveedor");

            foreach (ItemLista item in cmbProveedor.Items)
            {
                if (item.id == seleccionar)
                    cmbProveedor.SelectedItem = item;
            }
        }

        /// <summary>
        /// Llena combo.
        /// </summary>
        /// <param name="combo">Lista desplegable.</param>
        /// <param name="tabla">Tabla de datos.</param>
        /// <param name="columnaId">Columna con el identificador.</param>
        /// <param name="columnaTexto">Columna texto.</param>
        private void llenarCombo(ComboBox combo, DataTable tabla, string columnaId, string columnaTexto)
        {
            combo.Items.Clear();

            foreach (DataRow fila in tabla.Rows)
            {
                combo.Items.Add(new ItemLista(Convert.ToInt32(fila[columnaId]), "" + fila[columnaTexto], ""));
            }
        }

        /// <summary>
        /// Obtiene el identificador del elemento elegido en la lista desplegable.
        /// </summary>
        /// <param name="combo">Lista desplegable.</param>
        /// <returns>Identificador elegido, o 0 si no hay selección.</returns>
        private int idSeleccionado(ComboBox combo)
        {
            ItemLista? item = combo.SelectedItem as ItemLista;

            if (item == null)
                return 0;

            return item.id;
        }

        /// <summary>
        /// Carga artículos.
        /// </summary>
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

        /// <summary>
        /// Actualiza total.
        /// </summary>
        private void actualizarTotal()
        {
            txtTotal.Text = "Total a ingresar: " + inventarioBD.totalUnidades(lista) + " unidades";
        }

        /// <summary>
        /// Selecciona en la tabla la fila cuyo valor coincide con el indicado.
        /// </summary>
        /// <param name="tabla">Tabla de datos.</param>
        /// <param name="columna">Nombre de la columna.</param>
        /// <param name="valor">Valor a usar.</param>
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

        /// <summary>
        /// Atiende el cambio de marca de pestaña.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
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

        /// <summary>
        /// Busca texto.
        /// </summary>
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
        /// Impide escribir caracteres que no sean números.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void soloNumeros(object sender, TextCompositionEventArgs e)
        {
            if (!inventarioBD.soloNumeros(e.Text))
                e.Handled = true;
        }

        /// <summary>
        /// Impide escribir caracteres que no sean números o punto decimal.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void soloPrecio(object sender, TextCompositionEventArgs e)
        {
            if (!inventarioBD.caracteresDePrecio(e.Text))
                e.Handled = true;
        }

        /// <summary>
        /// Atiende el clic de agregar a lista.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
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

        /// <summary>
        /// Atiende el clic de nuevo artículo.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
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

        /// <summary>
        /// Atiende el clic de nuevo proveedor.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
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

        /// <summary>
        /// Atiende el clic de quitar.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
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

        /// <summary>
        /// Atiende el clic de vaciar.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void vaciarClic(object sender, RoutedEventArgs e)
        {
            lista.Rows.Clear();
            actualizarTotal();
        }

        /// <summary>
        /// Valida y guarda los datos del formulario.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
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

        /// <summary>
        /// Libera lo necesario justo antes de cerrar la ventana.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void ventanaCerrando(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (guardado || lista.Rows.Count == 0)
                return;

            MessageBoxResult respuesta = MessageBox.Show("La lista a ingresar no se ha guardado. ¿Desea salir de todos modos?",
                "Inventario", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (respuesta != MessageBoxResult.Yes)
                e.Cancel = true;
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

        /// <summary>
        /// Muestra un mensaje con el error ocurrido.
        /// </summary>
        /// <param name="ex">Excepción que se produjo.</param>
        private void mostrarError(Exception ex)
        {
            MensajeCodelex.mostrar(this, "Inventario", ex.Message, false);
        }
    }
}
