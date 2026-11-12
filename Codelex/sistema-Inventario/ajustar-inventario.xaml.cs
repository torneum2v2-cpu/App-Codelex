using Codelex.AccesoDatos;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Codelex
{
    public partial class AjustarInventario : Window
    {
        InventarioBD inventarioBD = new InventarioBD();

        int productoActual = 0;
        int stockActual = 0;

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        public AjustarInventario()
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
            try
            {
                foreach (string tipo in inventarioBD.tiposAjuste)
                {
                    cmbTipo.Items.Add(tipo);
                }

                cargarProductos();
                cargarAjustes();
                txtBuscar.Focus();
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        /// <summary>
        /// Carga la lista de productos según el texto de búsqueda.
        /// </summary>
        private void cargarProductos()
        {
            DataTable tabla = inventarioBD.listarProductos(txtBuscar.Text);
            dgProductos.ItemsSource = tabla.DefaultView;
        }

        /// <summary>
        /// Carga los últimos ajustes (del producto elegido o de todos).
        /// </summary>
        private void cargarAjustes()
        {
            DataTable tabla = inventarioBD.listarAjustes(productoActual);
            dgAjustes.ItemsSource = tabla.DefaultView;
        }

        /// <summary>
        /// Limpia los campos del ajuste.
        /// </summary>
        private void limpiarFormulario()
        {
            cmbTipo.SelectedIndex = -1;
            txtCantidad.Text = "";
            txtMotivo.Text = "";
            lblCantidad.Text = "Cantidad *";
        }

        /// <summary>
        /// Atiende el cambio de texto de buscar.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void buscarTextoCambiado(object sender, TextChangedEventArgs e)
        {
            try
            {
                cargarProductos();
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        /// <summary>
        /// Atiende el cambio de selección de productos.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void productosSeleccionCambiada(object sender, SelectionChangedEventArgs e)
        {
            if (dgProductos.SelectedItem == null)
                return;

            DataRowView fila = (DataRowView)dgProductos.SelectedItem;

            productoActual = Convert.ToInt32(fila["producto_id"]);
            stockActual = Convert.ToInt32(fila["stock"]);

            lblProducto.Text = "Producto: " + fila["nombre_producto"] + "  |  Stock actual: " + stockActual;

            try
            {
                cargarAjustes();
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        /// <summary>
        /// Atiende el cambio de tipo de ajuste: cambia el texto de la cantidad.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void tipoSeleccionCambiado(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTipo.SelectedItem == null)
                return;

            string tipo = inventarioBD.valorTipoAjuste("" + cmbTipo.SelectedItem);

            if (tipo == "Conteo")
                lblCantidad.Text = "Stock contado *";
            else
                lblCantidad.Text = "Cantidad a descontar *";
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
        /// Limpia los datos del formulario.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void limpiarClic(object sender, RoutedEventArgs e)
        {
            limpiarFormulario();
        }

        /// <summary>
        /// Valida y guarda el ajuste, pidiendo confirmación antes.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void guardarClic(object sender, RoutedEventArgs e)
        {
            try
            {
                string tipo = inventarioBD.valorTipoAjuste("" + cmbTipo.SelectedItem);

                int cantidad;
                string error = inventarioBD.validarAjuste(productoActual, tipo, txtCantidad.Text, txtMotivo.Text, out cantidad);
                if (error != "")
                {
                    MensajeCodelex.mostrar(this, "Ajustar inventario", error, false);
                    return;
                }

                string pregunta = "¿Registrar este ajuste? El stock del producto cambiará y el ajuste no se puede deshacer.";
                if (!MensajeCodelex.preguntar(this, "Ajustar inventario", pregunta))
                    return;

                string mensaje;
                bool guardado = inventarioBD.ajustarInventario(productoActual, tipo, txtCantidad.Text, txtMotivo.Text, out mensaje);

                MensajeCodelex.mostrar(this, "Ajustar inventario", mensaje, guardado);

                if (guardado)
                {
                    limpiarFormulario();
                    cargarProductos();
                    cargarAjustes();
                }
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
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
            MensajeCodelex.mostrar(this, "Ajustar inventario", ex.Message, false);
        }
    }
}
