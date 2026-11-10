using Codelex.AccesoDatos;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class Servicios : Window
    {
        ServicioBD servicioBD = new ServicioBD();

        int servicioActual = 0;
        bool cargando = false;

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        public Servicios()
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
            cargarServicios(0);
        }

        /// <summary>
        /// Carga la lista de servicios y deja elegido el que se indica.
        /// </summary>
        /// <param name="seleccionar">Identificador del servicio a elegir, 0 para ninguno.</param>
        private void cargarServicios(int seleccionar)
        {
            try
            {
                cargando = true;

                DataTable tabla = servicioBD.listarServicios();
                dgServicios.ItemsSource = tabla.DefaultView;

                servicioActual = 0;
                txtServicio.Text = "";

                foreach (DataRowView fila in dgServicios.Items)
                {
                    if (Convert.ToInt32(fila["servicio_id"]) == seleccionar)
                    {
                        dgServicios.SelectedItem = fila;
                        servicioActual = seleccionar;
                        txtServicio.Text = "" + fila["nombre_servicio"];
                    }
                }

                cargando = false;
                cargarActividades();
            }
            catch (Exception ex)
            {
                cargando = false;
                MensajeCodelex.mostrar(this, "Servicios", ex.Message, false);
            }
        }

        /// <summary>
        /// Carga las actividades del servicio elegido, o de todos si no hay uno elegido.
        /// </summary>
        private void cargarActividades()
        {
            try
            {
                DataTable tabla = servicioBD.listarActividades(servicioActual);
                dgActividades.ItemsSource = tabla.DefaultView;

                if (servicioActual == 0)
                    txtTituloActividades.Text = "Actividades de todos los servicios";
                else
                    txtTituloActividades.Text = "Actividades de " + txtServicio.Text;
            }
            catch (Exception ex)
            {
                MensajeCodelex.mostrar(this, "Servicios", ex.Message, false);
            }
        }

        /// <summary>
        /// Atiende el cambio de servicio elegido en la tabla.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void servicioSeleccionado(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cargando)
                return;

            DataRowView? fila = dgServicios.SelectedItem as DataRowView;

            if (fila == null)
            {
                servicioActual = 0;
                txtServicio.Text = "";
            }
            else
            {
                servicioActual = Convert.ToInt32(fila["servicio_id"]);
                txtServicio.Text = "" + fila["nombre_servicio"];
            }

            cargarActividades();
        }

        /// <summary>
        /// Limpia la selección para escribir un servicio nuevo.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void nuevoServicioClic(object sender, RoutedEventArgs e)
        {
            dgServicios.SelectedItem = null;
            servicioActual = 0;
            txtServicio.Text = "";
            cargarActividades();
            txtServicio.Focus();
        }

        /// <summary>
        /// Valida y guarda el servicio escrito.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void guardarServicioClic(object sender, RoutedEventArgs e)
        {
            try
            {
                string mensaje;
                int idGuardado;

                bool guardado = servicioBD.guardarServicio(servicioActual, txtServicio.Text, out mensaje, out idGuardado);

                MensajeCodelex.mostrar(this, "Servicios", mensaje, guardado);

                if (guardado)
                    cargarServicios(idGuardado);
            }
            catch (Exception ex)
            {
                MensajeCodelex.mostrar(this, "Servicios", ex.Message, false);
            }
        }

        /// <summary>
        /// Elimina el servicio elegido después de confirmar.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void eliminarServicioClic(object sender, RoutedEventArgs e)
        {
            if (servicioActual == 0)
            {
                MensajeCodelex.mostrar(this, "Servicios", "Seleccione un servicio de la tabla.", false);
                return;
            }

            string pregunta = "¿Eliminar el servicio " + txtServicio.Text + "?";
            MessageBoxResult respuesta = MessageBox.Show(pregunta, "Servicios", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (respuesta != MessageBoxResult.Yes)
                return;

            try
            {
                string mensaje;
                bool eliminado = servicioBD.eliminarServicio(servicioActual, out mensaje);

                MensajeCodelex.mostrar(this, "Servicios", mensaje, eliminado);

                if (eliminado)
                    cargarServicios(0);
            }
            catch (Exception ex)
            {
                MensajeCodelex.mostrar(this, "Servicios", ex.Message, false);
            }
        }

        /// <summary>
        /// Quita el filtro y muestra las actividades de todos los servicios.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void verTodasClic(object sender, RoutedEventArgs e)
        {
            nuevoServicioClic(sender, e);
        }

        /// <summary>
        /// Abre la ventana para crear una actividad en el servicio elegido.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void nuevaActividadClic(object sender, RoutedEventArgs e)
        {
            if (servicioActual == 0)
            {
                MensajeCodelex.mostrar(this, "Servicios", "Primero elija o cree el servicio de la actividad.", false);
                return;
            }

            abrirEditor(servicioActual, null);
        }

        /// <summary>
        /// Abre la ventana para editar la actividad elegida.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void editarActividadClic(object sender, RoutedEventArgs e)
        {
            DataRowView? fila = dgActividades.SelectedItem as DataRowView;

            if (fila == null)
            {
                MensajeCodelex.mostrar(this, "Servicios", "Seleccione una actividad de la tabla.", false);
                return;
            }

            abrirEditor(Convert.ToInt32(fila["servicio_id"]), fila.Row);
        }

        /// <summary>
        /// Abre el editor al hacer doble clic sobre una actividad.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void actividadDobleClic(object sender, MouseButtonEventArgs e)
        {
            if (dgActividades.SelectedItem != null)
                editarActividadClic(sender, e);
        }

        /// <summary>
        /// Muestra el editor de actividades y recarga las listas si se guardó algo.
        /// </summary>
        /// <param name="servicioId">Servicio con el que abre el editor.</param>
        /// <param name="actividad">Fila de la actividad a editar, o null si es nueva.</param>
        private void abrirEditor(int servicioId, DataRow? actividad)
        {
            ActividadEditar ventana = new ActividadEditar(servicioId, actividad);
            ventana.Owner = this;
            ventana.ShowDialog();

            if (ventana.guardada)
                cargarServicios(servicioActual);
        }

        /// <summary>
        /// Elimina la actividad elegida después de confirmar.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void eliminarActividadClic(object sender, RoutedEventArgs e)
        {
            DataRowView? fila = dgActividades.SelectedItem as DataRowView;

            if (fila == null)
            {
                MensajeCodelex.mostrar(this, "Servicios", "Seleccione una actividad de la tabla.", false);
                return;
            }

            string pregunta = "¿Eliminar la actividad " + fila["descripcion"] + "?";
            MessageBoxResult respuesta = MessageBox.Show(pregunta, "Servicios", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (respuesta != MessageBoxResult.Yes)
                return;

            try
            {
                string mensaje;
                bool eliminada = servicioBD.eliminarActividad(Convert.ToInt32(fila["actividad_id"]), out mensaje);

                MensajeCodelex.mostrar(this, "Servicios", mensaje, eliminada);

                if (eliminada)
                    cargarServicios(servicioActual);
            }
            catch (Exception ex)
            {
                MensajeCodelex.mostrar(this, "Servicios", ex.Message, false);
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
    }
}
