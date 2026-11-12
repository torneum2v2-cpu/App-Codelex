using Codelex.AccesoDatos;
using Codelex.AccesoDatos;
using System.Data;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class Usuarios : Window
    {
        UsuarioBD usuarioBD = new UsuarioBD();

        int usuarioActual = 0;
        int empleadoActual = 0;
        bool usuarioActivo = true;

        string textoBusqueda = "";
        string buscarPor = "";
        string filtroEstado = "TODOS";

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        public Usuarios()
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
            cargarRoles();
            limpiarFormulario();
            cargarUsuarios();
        }

        /// <summary>
        /// Carga roles.
        /// </summary>
        private void cargarRoles()
        {
            try
            {
                DataTable roles = usuarioBD.listarRoles();

                if (Sesion.Nivel != NivelAcceso.Avanzado)
                    roles.DefaultView.RowFilter = "nombreRol NOT LIKE '%Avanzado%'";

                cmbRol.ItemsSource = roles.DefaultView;
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        /// <summary>
        /// Carga usuarios.
        /// </summary>
        private void cargarUsuarios()
        {
            try
            {
                DataTable tabla = usuarioBD.buscarUsuarios(textoBusqueda, buscarPor, filtroEstado);
                dgUsuarios.ItemsSource = tabla.DefaultView;
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        /// <summary>
        /// Atiende el clic de usuarios.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void usuariosClic(object sender, MouseButtonEventArgs e)
        {
            if (dgUsuarios.SelectedItem == null)
                return;

            cargarUsuario((DataRowView)dgUsuarios.SelectedItem);
        }

        /// <summary>
        /// Limpia formulario.
        /// </summary>
        private void limpiarFormulario()
        {
            usuarioActual = 0;
            empleadoActual = 0;
            usuarioActivo = true;

            txtNombre.Text = "";
            txtApellido.Text = "";
            txtCorreo.Text = "";
            txtTelefono.Text = "";
            txtUsuario.Text = "";
            cmbRol.SelectedIndex = -1;
            pbContrasena.Clear();
            pbConfirmar.Clear();

            actualizarBotones();
        }

        /// <summary>
        /// Actualiza botones.
        /// </summary>
        private void actualizarBotones()
        {
            bool editando = usuarioActual > 0;

            btnCrear.IsEnabled = !editando;
            btnActualizar.IsEnabled = editando;
            btnBuscarEmpleado.IsEnabled = !editando;
            btnCambiarEstado.IsEnabled = editando;

            btnCambiarEstado.Content = usuarioActivo ? "Desactivar" : "Activar";

            if (editando)
                txtEstado.Text = usuarioActivo ? "Activo" : "Inactivo";
            else if (empleadoActual > 0)
                txtEstado.Text = "Activo";
            else
                txtEstado.Text = "—";
        }

        /// <summary>
        /// Carga empleado.
        /// </summary>
        /// <param name="fila">Fila con los datos.</param>
        private void cargarEmpleado(DataRowView fila)
        {
            usuarioActual = 0;
            usuarioActivo = true;
            empleadoActual = Convert.ToInt32(fila["empleado_id"]);

            txtNombre.Text = "" + fila["nombre_empleado"];
            txtApellido.Text = "" + fila["apellido_empleado"];
            txtCorreo.Text = "" + fila["correo_empleado"];
            txtTelefono.Text = Validacion.formatoTelefono("" + fila["telefono_empleado"]);

            string sugerido = generarUsuario(txtNombre.Text, txtApellido.Text);
            try
            {
                sugerido = usuarioBD.usuarioDisponible(sugerido);
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
            txtUsuario.Text = sugerido;

            cmbRol.SelectedIndex = -1;
            pbContrasena.Clear();
            pbConfirmar.Clear();

            actualizarBotones();
        }

        /// <summary>
        /// Carga usuario.
        /// </summary>
        /// <param name="fila">Fila con los datos.</param>
        private void cargarUsuario(DataRowView fila)
        {
            usuarioActual = Convert.ToInt32(fila["usuario_id"]);
            empleadoActual = Convert.ToInt32(fila["empleado_id"]);
            usuarioActivo = Convert.ToBoolean(fila["estado_usuario"]);

            txtNombre.Text = "" + fila["nombre_empleado"];
            txtApellido.Text = "" + fila["apellido_empleado"];
            txtCorreo.Text = "" + fila["correo_empleado"];
            txtTelefono.Text = Validacion.formatoTelefono("" + fila["telefono_empleado"]);

            txtUsuario.Text = "" + fila["usuario"];

            if (fila["rol_id"] == DBNull.Value)
                cmbRol.SelectedIndex = -1;
            else
                cmbRol.SelectedValue = Convert.ToInt32(fila["rol_id"]);

            pbContrasena.Clear();
            pbConfirmar.Clear();

            actualizarBotones();
        }

        /// <summary>
        /// Genera usuario.
        /// </summary>
        /// <param name="nombre">Nombre.</param>
        /// <param name="apellido">Apellido.</param>
        /// <returns>Texto resultante.</returns>
        private string generarUsuario(string nombre, string apellido)
        {
            string primerApellido = apellido.Trim();
            int espacio = primerApellido.IndexOf(' ');
            if (espacio > 0)
                primerApellido = primerApellido.Substring(0, espacio);

            string inicio = soloLetras(nombre);
            if (inicio.Length > 2)
                inicio = inicio.Substring(0, 2);

            return inicio + soloLetras(primerApellido);
        }

        /// <summary>
        /// Permite solo letras.
        /// </summary>
        /// <param name="texto">Texto a usar.</param>
        /// <returns>Texto resultante.</returns>
        private string soloLetras(string texto)
        {
            string descompuesto = texto.Normalize(NormalizationForm.FormD);
            StringBuilder resultado = new StringBuilder();

            foreach (char c in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                    continue;

                if (char.IsLetter(c))
                    resultado.Append(char.ToLower(c));
            }

            return resultado.ToString();
        }

        /// <summary>
        /// Valida formulario.
        /// </summary>
        /// <param name="esNuevo">Es nuevo.</param>
        /// <returns>Texto resultante.</returns>
        private string validarFormulario(bool esNuevo)
        {
            if (empleadoActual == 0)
                return "Elija un empleado con el botón Buscar empleado.";

            if (esNuevo && Validacion.validarCorreo(txtCorreo.Text, true, 50) != "")
                return "El correo del empleado está vacío o no tiene un formato válido. " +
                       "Corríjalo en el módulo de Empleados antes de continuar.";

            string problema = usuarioBD.validarAcceso(txtUsuario.Text, pbContrasena.Password, esNuevo);
            if (problema != "")
                return problema;

            if (cmbRol.SelectedValue == null)
                return "Seleccione un rol.";

            if (pbContrasena.Password != pbConfirmar.Password)
                return "Las contraseñas no coinciden.";

            return "";
        }

        /// <summary>
        /// Limpia el formulario para registrar uno nuevo.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void nuevoClic(object sender, RoutedEventArgs e)
        {
            limpiarFormulario();
            dgUsuarios.SelectedItem = null;
        }

        /// <summary>
        /// Atiende el clic de buscar empleado.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void buscarEmpleadoClic(object sender, RoutedEventArgs e)
        {
            ElegirEmpleado ventana = new ElegirEmpleado();
            ventana.Owner = this;

            if (ventana.ShowDialog() == true && ventana.empleadoElegido != null)
                cargarEmpleado(ventana.empleadoElegido);
        }

        /// <summary>
        /// Valida y guarda el registro nuevo.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void crearClic(object sender, RoutedEventArgs e)
        {
            string error = validarFormulario(true);
            if (error != "")
            {
                MensajeCodelex.mostrar(this, "Usuarios", error, false);
                return;
            }

            try
            {
                string mensaje;

                bool guardado = usuarioBD.crearUsuario(
                    empleadoActual, txtUsuario.Text, pbContrasena.Password,
                    Convert.ToInt32(cmbRol.SelectedValue), out mensaje);

                mostrarMensaje(guardado, mensaje);

                if (guardado)
                {
                    cargarUsuarios();
                    limpiarFormulario();
                }
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        /// <summary>
        /// Valida y guarda los cambios del registro seleccionado.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void actualizarClic(object sender, RoutedEventArgs e)
        {
            if (usuarioActual == 0)
                return;

            string error = validarFormulario(false);
            if (error != "")
            {
                MensajeCodelex.mostrar(this, "Usuarios", error, false);
                return;
            }

            try
            {
                string mensaje;

                bool guardado = usuarioBD.actualizarUsuario(
                    usuarioActual, txtUsuario.Text, pbContrasena.Password,
                    Convert.ToInt32(cmbRol.SelectedValue), out mensaje);

                mostrarMensaje(guardado, mensaje);

                if (guardado)
                {
                    pbContrasena.Clear();
                    pbConfirmar.Clear();
                    cargarUsuarios();
                }
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        /// <summary>
        /// Activa o desactiva el registro seleccionado.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void cambiarEstadoClic(object sender, RoutedEventArgs e)
        {
            if (usuarioActual == 0)
                return;

            bool nuevoEstado = !usuarioActivo;

            string pregunta;
            if (nuevoEstado)
                pregunta = "¿Desea activar el usuario " + txtUsuario.Text + "?";
            else
                pregunta = "¿Desea desactivar el usuario " + txtUsuario.Text + "?";

            if (!MensajeCodelex.preguntar(this, "Usuarios", pregunta))
                return;

            try
            {
                string mensaje;
                bool guardado = usuarioBD.cambiarEstadoUsuario(usuarioActual, nuevoEstado, out mensaje);

                mostrarMensaje(guardado, mensaje);

                if (guardado)
                {
                    usuarioActivo = nuevoEstado;
                    actualizarBotones();
                    cargarUsuarios();
                }
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        /// <summary>
        /// Atiende el clic de por nombre.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void porNombreClic(object sender, RoutedEventArgs e)
        {
            textoBusqueda = txtBuscar.Text.Trim();
            buscarPor = "NOMBRE";
            cargarUsuarios();
        }

        /// <summary>
        /// Atiende el clic de por usuario.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void porUsuarioClic(object sender, RoutedEventArgs e)
        {
            textoBusqueda = txtBuscar.Text.Trim();
            buscarPor = "USUARIO";
            cargarUsuarios();
        }

        /// <summary>
        /// Atiende el clic de por correo.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void porCorreoClic(object sender, RoutedEventArgs e)
        {
            textoBusqueda = txtBuscar.Text.Trim();
            buscarPor = "CORREO";
            cargarUsuarios();
        }

        /// <summary>
        /// Atiende el clic de por rol.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void porRolClic(object sender, RoutedEventArgs e)
        {
            textoBusqueda = txtBuscar.Text.Trim();
            buscarPor = "ROL";
            cargarUsuarios();
        }

        /// <summary>
        /// Muestra solo los registros activos.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void activosClic(object sender, RoutedEventArgs e)
        {
            filtroEstado = "ACTIVOS";
            cargarUsuarios();
        }

        /// <summary>
        /// Muestra solo los registros inactivos.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void inactivosClic(object sender, RoutedEventArgs e)
        {
            filtroEstado = "INACTIVOS";
            cargarUsuarios();
        }

        /// <summary>
        /// Quita los filtros y muestra todos los registros.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void mostrarTodosClic(object sender, RoutedEventArgs e)
        {
            txtBuscar.Text = "";
            textoBusqueda = "";
            buscarPor = "";
            filtroEstado = "TODOS";
            cargarUsuarios();
        }

        /// <summary>
        /// Ejecuta la búsqueda al presionar Enter.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void buscarTeclaPresionada(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                textoBusqueda = txtBuscar.Text.Trim();
                buscarPor = "";
                cargarUsuarios();
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
        /// Muestra mensaje.
        /// </summary>
        /// <param name="correcto">Correcto.</param>
        /// <param name="mensaje">Mensaje con el resultado.</param>
        private void mostrarMensaje(bool correcto, string mensaje)
        {
            if (correcto)
                MensajeCodelex.mostrar(this, "Usuarios", mensaje, true);
            else
                MensajeCodelex.mostrar(this, "Usuarios", mensaje, false);
        }

        /// <summary>
        /// Muestra un mensaje con el error ocurrido.
        /// </summary>
        /// <param name="ex">Excepción que se produjo.</param>
        private void mostrarError(Exception ex)
        {
            MensajeCodelex.mostrar(this, "Usuarios", ex.Message, false);
        }
    }
}