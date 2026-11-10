using Codelex.AccesoDatos;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class NuevoProductoEquipo : Window
    {
        InventarioBD inventarioBD = new InventarioBD();
        ServicioBD servicioBD = new ServicioBD();

        // "" = producto o equipo con sus actividades; "producto" o "equipo" = se abre desde una actividad.
        string tipoFijo = "";
        DataTable actividades = new DataTable();

        public bool registrado = false;
        public bool esEquipo = false;
        public int productoId = 0;
        public string nombre = "";
        public string tipo = "";
        public string marca = "";
        public string modelo = "";
        public string cantidad = "";

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        /// <param name="tipoFijo">Vacío para elegir producto o equipo; "producto" o "equipo" cuando se abre desde una actividad.</param>
        public NuevoProductoEquipo(string tipoFijo = "")
        {
            InitializeComponent();
            this.tipoFijo = tipoFijo;
        }

        /// <summary>
        /// Prepara la ventana y carga los datos iniciales cuando termina de abrirse.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void ventanaCargada(object sender, RoutedEventArgs e)
        {
            if (tipoFijo == "producto")
            {
                rbProducto.IsChecked = true;
                spTipoArticulo.Visibility = Visibility.Collapsed;
                spActividades.Visibility = Visibility.Collapsed;
                spCantidad.Visibility = Visibility.Visible;
            }
            else if (tipoFijo == "equipo")
            {
                rbEquipo.IsChecked = true;
                spTipoArticulo.Visibility = Visibility.Collapsed;
                spActividades.Visibility = Visibility.Collapsed;
            }
            else
            {
                cargarActividades();
            }

            mostrarCampos();
            limpiarFormulario();
        }

        /// <summary>
        /// Carga las actividades que se pueden marcar para el producto o equipo nuevo.
        /// </summary>
        private void cargarActividades()
        {
            try
            {
                actividades = servicioBD.crearActividadesParaElegir();
                dgActividades.ItemsSource = actividades.DefaultView;

                if (actividades.Rows.Count == 0)
                    txtAyudaActividades.Text = "Todavía no hay actividades. Cree primero un servicio con sus actividades en el módulo Servicios.";
            }
            catch (Exception ex)
            {
                MensajeCodelex.mostrar(this, "Inventario", ex.Message, false);
            }
        }

        /// <summary>
        /// Atiende el cambio de marca de tipo.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void tipoMarcado(object sender, RoutedEventArgs e)
        {
            if (spMarca == null || spModelo == null)
                return;

            mostrarCampos();
        }

        /// <summary>
        /// Muestra u oculta los campos según se registre un producto o un equipo.
        /// </summary>
        private void mostrarCampos()
        {
            if (rbEquipo.IsChecked == true)
            {
                lblTipo.Text = "Tipo de equipo *";
                txtTipo.MaxLength = 50;
                colCantidad.Visibility = Visibility.Collapsed;
                txtAyudaActividades.Text = "Marque cada actividad que usa el equipo. Debe elegir al menos una.";

                if (tipoFijo == "equipo")
                {
                    spTipo.Visibility = Visibility.Collapsed;
                    spMarca.Visibility = Visibility.Collapsed;
                    spModelo.Visibility = Visibility.Collapsed;
                    txtNota.Text = "Solo el nombre del equipo. Las unidades se compran luego en Inventario > Agregar al inventario.";
                }
                else
                {
                    spMarca.Visibility = Visibility.Visible;
                    spModelo.Visibility = Visibility.Visible;
                    txtNota.Text = "Los equipos se guardan por unidad: el equipo nuevo aparece en la pestaña Equipos y sus unidades se crean al guardar el ingreso.";
                }
            }
            else
            {
                lblTipo.Text = "Tipo de producto *";
                txtTipo.MaxLength = 20;
                spMarca.Visibility = Visibility.Collapsed;
                spModelo.Visibility = Visibility.Collapsed;
                colCantidad.Visibility = Visibility.Visible;
                txtAyudaActividades.Text = "Marque cada actividad que usa el producto y cuánto se gasta por unidad (por ejemplo 0.002 por m²). Debe elegir al menos una.";
                txtNota.Text = "El producto se registra con existencia 0. Luego agréguelo a la lista para ingresar lo que se compró.";
            }

            if (actividades.Rows.Count == 0 && spActividades.Visibility == Visibility.Visible)
                txtAyudaActividades.Text = "Todavía no hay actividades. Cree primero un servicio con sus actividades en el módulo Servicios.";
        }

        /// <summary>
        /// Limpia formulario.
        /// </summary>
        private void limpiarFormulario()
        {
            txtNombre.Text = "";
            txtTipo.Text = "";
            txtMarca.Text = "";
            txtModelo.Text = "";
            txtCantidad.Text = "";

            foreach (DataRow fila in actividades.Rows)
            {
                fila["usar"] = false;
                fila["cantidad"] = "1";
            }

            txtNombre.Focus();
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
                if (tipoFijo != "")
                {
                    guardarDesdeActividad();
                    return;
                }

                string mensaje;
                bool conCantidad = rbEquipo.IsChecked != true;
                DataTable vinculos = servicioBD.crearVinculos();

                if (!conCantidad)
                {
                    mensaje = inventarioBD.validarEquipo(txtNombre.Text, txtTipo.Text, txtMarca.Text, txtModelo.Text);
                    if (mensaje != "")
                    {
                        MensajeCodelex.mostrar(this, "Inventario", mensaje, false);
                        return;
                    }
                }

                mensaje = servicioBD.tomarVinculos(actividades, conCantidad, vinculos);
                if (mensaje != "")
                {
                    MensajeCodelex.mostrar(this, "Inventario", mensaje, false);
                    return;
                }

                if (conCantidad)
                {
                    int idNuevo;
                    bool guardado = servicioBD.insertarProductoConActividades(txtNombre.Text, txtTipo.Text, vinculos, out mensaje, out idNuevo);

                    MensajeCodelex.mostrar(this, "Inventario", mensaje, guardado);

                    if (!guardado)
                        return;

                    productoId = idNuevo;
                }
                else
                {
                    bool ligado = servicioBD.vincularEquipo(txtNombre.Text, vinculos, out mensaje);

                    MensajeCodelex.mostrar(this, "Inventario", mensaje, ligado);

                    if (!ligado)
                        return;

                    esEquipo = true;
                    marca = txtMarca.Text.Trim();
                    modelo = txtModelo.Text.Trim();
                }

                nombre = txtNombre.Text.Trim();
                tipo = txtTipo.Text.Trim();
                registrado = true;
                Close();
            }
            catch (Exception ex)
            {
                MensajeCodelex.mostrar(this, "Inventario", ex.Message, false);
            }
        }

        /// <summary>
        /// Valida los datos cuando la ventana se abrió desde una actividad; ahí el artículo se guarda junto con la actividad.
        /// </summary>
        private void guardarDesdeActividad()
        {
            string mensaje;

            if (tipoFijo == "producto")
            {
                mensaje = inventarioBD.validarProducto(txtNombre.Text, txtTipo.Text);
                if (mensaje != "")
                {
                    MensajeCodelex.mostrar(this, "Servicios", mensaje, false);
                    return;
                }

                decimal valor;
                if (!servicioBD.leerDecimal(txtCantidad.Text, out valor) || valor <= 0)
                {
                    MensajeCodelex.mostrar(this, "Servicios", "Escriba la cantidad por unidad (mayor que cero).", false);
                    return;
                }

                tipo = txtTipo.Text.Trim();
                cantidad = txtCantidad.Text.Trim();
            }
            else
            {
                if (txtNombre.Text.Trim() == "")
                {
                    MensajeCodelex.mostrar(this, "Servicios", "El nombre del equipo es obligatorio.", false);
                    return;
                }

                if (txtNombre.Text.Trim().Length > 50)
                {
                    MensajeCodelex.mostrar(this, "Servicios", "El nombre no puede tener más de 50 caracteres.", false);
                    return;
                }

                esEquipo = true;
            }

            nombre = txtNombre.Text.Trim();
            registrado = true;
            Close();
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
