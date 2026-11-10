using Codelex.AccesoDatos;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Codelex
{
    public partial class ActividadEditar : Window
    {
        ServicioBD servicioBD = new ServicioBD();
        InventarioBD inventarioBD = new InventarioBD();
        DatosActividad datos;

        int servicioInicial;
        DataRow? actividad;

        public bool guardada = false;

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        /// <param name="servicioInicial">Servicio que aparece elegido al abrir.</param>
        /// <param name="actividad">Fila de la actividad a editar, o null si es nueva.</param>
        public ActividadEditar(int servicioInicial, DataRow? actividad)
        {
            InitializeComponent();

            this.servicioInicial = servicioInicial;
            this.actividad = actividad;

            datos = servicioBD.crearDatosActividad();
            dgRangos.ItemsSource = datos.rangos.DefaultView;
            dgProductos.ItemsSource = datos.productos.DefaultView;
            dgEquipos.ItemsSource = datos.equipos.DefaultView;
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
                cargarServicios();
                cargarProductos(0);
                cargarEquipos("");

                if (actividad != null)
                    cargarActividad();

                txtActividad.Focus();
            }
            catch (Exception ex)
            {
                MensajeCodelex.mostrar(this, "Servicios", ex.Message, false);
            }
        }

        /// <summary>
        /// Llena la lista de servicios y deja elegido el servicio inicial.
        /// </summary>
        private void cargarServicios()
        {
            cmbServicio.Items.Clear();

            foreach (DataRow fila in servicioBD.listarServicios().Rows)
            {
                ItemLista item = new ItemLista(Convert.ToInt32(fila["servicio_id"]), "" + fila["nombre_servicio"], "");
                cmbServicio.Items.Add(item);

                if (item.id == servicioInicial)
                    cmbServicio.SelectedItem = item;
            }
        }

        /// <summary>
        /// Llena la lista de productos registrados y elige el indicado.
        /// </summary>
        /// <param name="seleccionar">Identificador del producto a elegir, 0 para ninguno.</param>
        private void cargarProductos(int seleccionar)
        {
            cmbProducto.Items.Clear();

            foreach (DataRow fila in servicioBD.listarProductosCatalogo().Rows)
            {
                string texto = "" + fila["nombre_producto"];
                if ("" + fila["tipo_producto"] != "")
                    texto = texto + " (" + fila["tipo_producto"] + ")";

                ItemLista item = new ItemLista(Convert.ToInt32(fila["producto_id"]), texto, "" + fila["tipo_producto"]);
                cmbProducto.Items.Add(item);

                if (item.id == seleccionar)
                    cmbProducto.SelectedItem = item;
            }
        }

        /// <summary>
        /// Llena la lista de equipos registrados y elige el indicado.
        /// </summary>
        /// <param name="seleccionar">Nombre del equipo a elegir, vacío para ninguno.</param>
        private void cargarEquipos(string seleccionar)
        {
            cmbEquipo.Items.Clear();

            foreach (DataRow fila in servicioBD.listarEquiposCatalogo().Rows)
                cmbEquipo.Items.Add("" + fila["nombre_equipo"]);

            if (seleccionar != "")
                cmbEquipo.SelectedItem = seleccionar;
        }

        /// <summary>
        /// Carga los datos de la actividad que se va a editar.
        /// </summary>
        private void cargarActividad()
        {
            if (actividad == null)
                return;

            int actividadId = Convert.ToInt32(actividad["actividad_id"]);
            datos.actividadId = actividadId;

            txtTituloActividad.Text = "Editar actividad";
            txtActividad.Text = "" + actividad["descripcion"];
            txtUnidad.Text = "" + actividad["unidad"];

            decimal rendimiento = Convert.ToDecimal(actividad["rendimiento"]);
            if (rendimiento > 0)
                txtRendimiento.Text = rendimiento.ToString("0.##", CultureInfo.InvariantCulture);

            foreach (DataRow fila in servicioBD.listarRangos(actividadId).Rows)
                datos.rangos.Rows.Add(fila["desde"], fila["hasta"], fila["cantidad_personas"]);

            foreach (DataRow fila in servicioBD.listarProductosActividad(actividadId).Rows)
                datos.productos.Rows.Add(fila["producto_id"], fila["nombre_producto"], fila["tipo_producto"], fila["cantidad_por_unidad"]);

            foreach (DataRow fila in servicioBD.listarEquiposActividad(actividadId).Rows)
                datos.equipos.Rows.Add(fila["nombre_equipo"]);
        }

        /// <summary>
        /// Obtiene el identificador del elemento elegido en una lista desplegable.
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

        // ---------- Rangos ----------

        /// <summary>
        /// Agrega el rango de personas escrito a la tabla.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void agregarRangoClic(object sender, RoutedEventArgs e)
        {
            string mensaje;

            if (!servicioBD.agregarRango(datos.rangos, txtDesde.Text, txtHasta.Text, txtPersonas.Text, out mensaje))
            {
                MensajeCodelex.mostrar(this, "Servicios", mensaje, false);
                return;
            }

            txtDesde.Text = "";
            txtHasta.Text = "";
            txtPersonas.Text = "";
            txtDesde.Focus();
        }

        /// <summary>
        /// Quita de la tabla el rango elegido.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void quitarRangoClic(object sender, RoutedEventArgs e)
        {
            quitarFila(dgRangos, datos.rangos, "un rango");
        }

        // ---------- Productos ----------

        /// <summary>
        /// Agrega a la actividad el producto elegido con su cantidad por unidad.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void agregarProductoClic(object sender, RoutedEventArgs e)
        {
            ItemLista? item = cmbProducto.SelectedItem as ItemLista;
            string mensaje;

            if (item == null)
            {
                MensajeCodelex.mostrar(this, "Servicios", "Elija un producto.", false);
                return;
            }

            if (!servicioBD.agregarProducto(datos.productos, item.id, nombreSinTipo(item), item.tipo, txtCantidadProducto.Text, out mensaje))
            {
                MensajeCodelex.mostrar(this, "Servicios", mensaje, false);
                return;
            }

            cmbProducto.SelectedItem = null;
            txtCantidadProducto.Text = "";
        }

        /// <summary>
        /// Obtiene el nombre del producto sin el tipo que se muestra entre paréntesis.
        /// </summary>
        /// <param name="item">Elemento de la lista de productos.</param>
        /// <returns>Nombre del producto.</returns>
        private string nombreSinTipo(ItemLista item)
        {
            if (item.tipo == "")
                return item.texto;

            return item.texto.Substring(0, item.texto.Length - item.tipo.Length - 3);
        }

        /// <summary>
        /// Abre la ventana para crear un producto nuevo que se guarda junto con la actividad.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void nuevoProductoClic(object sender, RoutedEventArgs e)
        {
            NuevoProductoEquipo ventana = new NuevoProductoEquipo("producto");
            ventana.Owner = this;
            ventana.ShowDialog();

            if (!ventana.registrado)
                return;

            string mensaje;

            if (!servicioBD.agregarProducto(datos.productos, 0, ventana.nombre, ventana.tipo, ventana.cantidad, out mensaje))
                MensajeCodelex.mostrar(this, "Servicios", mensaje, false);
        }

        /// <summary>
        /// Quita de la actividad el producto elegido.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void quitarProductoClic(object sender, RoutedEventArgs e)
        {
            quitarFila(dgProductos, datos.productos, "un producto");
        }

        // ---------- Equipos ----------

        /// <summary>
        /// Agrega a la actividad el equipo elegido.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void agregarEquipoClic(object sender, RoutedEventArgs e)
        {
            string mensaje;
            string nombre = "";

            if (cmbEquipo.SelectedItem != null)
                nombre = "" + cmbEquipo.SelectedItem;

            if (!servicioBD.agregarEquipo(datos.equipos, nombre, out mensaje))
            {
                MensajeCodelex.mostrar(this, "Servicios", mensaje, false);
                return;
            }

            cmbEquipo.SelectedItem = null;
        }

        /// <summary>
        /// Abre la ventana para registrar un equipo nuevo y lo agrega a la actividad.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void nuevoEquipoClic(object sender, RoutedEventArgs e)
        {
            NuevoProductoEquipo ventana = new NuevoProductoEquipo("equipo");
            ventana.Owner = this;
            ventana.ShowDialog();

            if (!ventana.registrado)
                return;

            string mensaje;

            if (!servicioBD.agregarEquipo(datos.equipos, ventana.nombre, out mensaje))
                MensajeCodelex.mostrar(this, "Servicios", mensaje, false);
        }

        /// <summary>
        /// Quita de la actividad el equipo elegido.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void quitarEquipoClic(object sender, RoutedEventArgs e)
        {
            quitarFila(dgEquipos, datos.equipos, "un equipo");
        }

        /// <summary>
        /// Quita de una tabla la fila elegida en la cuadrícula.
        /// </summary>
        /// <param name="cuadricula">Cuadrícula que muestra la tabla.</param>
        /// <param name="tabla">Tabla de datos.</param>
        /// <param name="texto">Texto para el mensaje, por ejemplo "un rango".</param>
        private void quitarFila(DataGrid cuadricula, DataTable tabla, string texto)
        {
            DataRowView? fila = cuadricula.SelectedItem as DataRowView;

            if (fila == null)
            {
                MensajeCodelex.mostrar(this, "Servicios", "Seleccione " + texto + " de la tabla.", false);
                return;
            }

            tabla.Rows.Remove(fila.Row);
        }

        // ---------- Guardar ----------

        /// <summary>
        /// Valida y guarda la actividad con sus rangos, productos y equipos.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void guardarClic(object sender, RoutedEventArgs e)
        {
            try
            {
                decimal rendimiento = 0;

                if (txtRendimiento.Text.Trim() != "" && !servicioBD.leerDecimal(txtRendimiento.Text, out rendimiento))
                {
                    MensajeCodelex.mostrar(this, "Servicios", "El rendimiento no es un número válido.", false);
                    return;
                }

                datos.servicioId = idSeleccionado(cmbServicio);
                datos.descripcion = txtActividad.Text;
                datos.unidad = txtUnidad.Text;
                datos.rendimiento = rendimiento;

                string mensaje;
                int idGuardado;

                bool guardado = servicioBD.guardarActividad(datos, out mensaje, out idGuardado);

                MensajeCodelex.mostrar(this, "Servicios", mensaje, guardado);

                if (guardado)
                {
                    guardada = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MensajeCodelex.mostrar(this, "Servicios", ex.Message, false);
            }
        }

        // ---------- Entrada de datos ----------

        /// <summary>
        /// Impide escribir caracteres que no sean números.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void soloNumeros(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !inventarioBD.soloNumeros(e.Text);
        }

        /// <summary>
        /// Impide escribir caracteres que no sean números o punto decimal.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void soloDecimales(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !inventarioBD.caracteresDePrecio(e.Text);
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
