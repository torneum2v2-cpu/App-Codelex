using Codelex.AccesoDatos;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Codelex
{
    /// <summary>
    /// Actividad de la orden en la lista del control de calidad.
    /// </summary>
    public class ItemControl
    {
        public int ActividadId { get; set; } = 0;
        public string Descripcion { get; set; } = "";
        public bool Hecha { get; set; } = false;
        public string Detalle { get; set; } = "";
    }

    /// <summary>
    /// Control de calidad de una orden: se revisa cada actividad y solo cuando todas están marcadas se puede completar la orden.
    /// </summary>
    public partial class ControlCalidad : Window
    {
        GestionOrdenesBD gestionBD = new GestionOrdenesBD();
        ObservableCollection<ItemControl> items = new ObservableCollection<ItemControl>();
        DataTable actividades = new DataTable();
        string ordenId = "";

        /// <summary>
        /// Indica si la orden quedó completada al cerrar el control.
        /// </summary>
        public bool completada = false;

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        /// <param name="ordenId">Identificador de la orden que se va a revisar.</param>
        public ControlCalidad(string ordenId)
        {
            InitializeComponent();

            this.ordenId = ordenId;
            lblTitulo.Text = "Control de calidad - " + ordenId;
            lstControl.ItemsSource = items;
            Loaded += ventanaCargada;
        }

        /// <summary>
        /// Carga las actividades de la orden cuando termina de abrirse la ventana.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void ventanaCargada(object sender, RoutedEventArgs e)
        {
            try
            {
                cargarActividades();
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        /// <summary>
        /// Carga las actividades de la orden y actualiza el avance.
        /// </summary>
        private void cargarActividades()
        {
            actividades = gestionBD.listarActividades(ordenId);
            items.Clear();

            foreach (DataRow fila in actividades.Rows)
            {
                ItemControl item = new ItemControl();
                item.ActividadId = Convert.ToInt32(fila["actividad_id"]);
                item.Descripcion = "" + fila["descripcion"];
                item.Hecha = Convert.ToBoolean(fila["estado"]);

                if (item.Hecha && fila["fecha_marcado"] != DBNull.Value)
                {
                    DateTime fecha = Convert.ToDateTime(fila["fecha_marcado"]);
                    item.Detalle = "Revisada el " + fecha.ToString("dd/MM/yyyy HH:mm") + " por " + fila["marcado_por"];
                }
                else if (item.Hecha)
                    item.Detalle = "Revisada";
                else
                    item.Detalle = "Pendiente de revisar";

                items.Add(item);
            }

            actualizarAvance();
        }

        /// <summary>
        /// Muestra cuántas actividades van revisadas y activa el botón de aprobar solo si están todas.
        /// </summary>
        private void actualizarAvance()
        {
            int total = items.Count;
            int hechas = gestionBD.contarRevisadas(actividades);

            lblProgreso.Text = "Revisadas " + hechas + " de " + total;
            barraProgreso.Maximum = Math.Max(total, 1);
            barraProgreso.Value = hechas;

            string error = gestionBD.validarControl(actividades, txtNota.Text);
            btnAprobar.IsEnabled = error == "";

            if (total == 0)
                lblEstadoControl.Text = "Esta orden no tiene actividades para revisar.";
            else if (hechas == total)
                lblEstadoControl.Text = "Todo está revisado. Ya puede aprobar el control y completar la orden.";
            else
                lblEstadoControl.Text = "Faltan " + (total - hechas) + " actividades. Mientras falte alguna, la orden sigue en proceso.";
        }

        /// <summary>
        /// Guarda el cambio cuando se marca o desmarca una actividad.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void actividadClic(object sender, RoutedEventArgs e)
        {
            CheckBox casilla = (CheckBox)sender;
            ItemControl item = (ItemControl)casilla.DataContext;
            bool hecha = casilla.IsChecked == true;

            try
            {
                string mensaje;
                bool guardado = gestionBD.marcarActividad(ordenId, item.ActividadId, hecha, out mensaje);

                if (!guardado)
                    MensajeCodelex.mostrar(this, "Control de calidad", mensaje, false);

                cargarActividades();
            }
            catch (Exception ex)
            {
                mostrarError(ex);
                cargarActividades();
            }
        }

        /// <summary>
        /// Pide confirmación y completa la orden con el control de calidad aprobado.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void aprobarClic(object sender, RoutedEventArgs e)
        {
            string error = gestionBD.validarControl(actividades, txtNota.Text);
            if (error != "")
            {
                MensajeCodelex.mostrar(this, "Control de calidad", error, false);
                return;
            }

            string pregunta = "¿Aprobar el control de calidad y completar la orden " + ordenId + "?\n\n"
                              + "Los equipos asignados quedarán libres y la orden ya no podrá cambiar de estado.";
            if (!MensajeCodelex.preguntar(this, "Control de calidad", pregunta))
                return;

            try
            {
                string mensaje;
                bool aprobado = gestionBD.completarConControl(ordenId, actividades, txtNota.Text, out mensaje);

                MensajeCodelex.mostrar(this, "Control de calidad", mensaje, aprobado);

                if (aprobado)
                {
                    completada = true;
                    Close();
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
            MensajeCodelex.mostrar(this, "Control de calidad", ex.Message, false);
        }
    }
}
