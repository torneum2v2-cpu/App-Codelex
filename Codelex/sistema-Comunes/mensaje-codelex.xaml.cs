using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Codelex
{
    public partial class MensajeCodelex : Window
    {
        private bool confirmado = false;

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        /// <param name="titulo">Título del cuadro.</param>
        /// <param name="mensaje">Mensaje con el resultado.</param>
        /// <param name="exito">true si es un mensaje de éxito; false si es un error.</param>
        public MensajeCodelex(string titulo, string mensaje, bool exito)
        {
            InitializeComponent();

            txtTitulo.Text = titulo;
            txtMensaje.Text = mensaje;

            if (exito)
            {
                pathCheck.Visibility = Visibility.Visible;
                pathX.Visibility = Visibility.Collapsed;
                elIcono.Stroke = (Brush)FindResource("Primary");
            }
            else
            {
                pathCheck.Visibility = Visibility.Collapsed;
                pathX.Visibility = Visibility.Visible;
                elIcono.Stroke = (Brush)FindResource("Danger");
            }
        }

        /// <summary>
        /// Muestra el cuadro de mensaje de Codelex (de éxito o de error).
        /// </summary>
        /// <param name="dueno">Ventana dueña del cuadro.</param>
        /// <param name="titulo">Título del cuadro.</param>
        /// <param name="mensaje">Mensaje con el resultado.</param>
        /// <param name="exito">true si es un mensaje de éxito; false si es un error.</param>
        public static void mostrar(Window dueno, string titulo, string mensaje, bool exito)
        {
            MensajeCodelex ventana = new MensajeCodelex(titulo, mensaje, exito);

            if (dueno != null)
            {
                ventana.Owner = dueno;
                ventana.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                ventana.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            ventana.ShowDialog();
        }

        /// <summary>
        /// Pregunta al usuario si desea continuar, con el mismo diseño de Codelex.
        /// </summary>
        /// <param name="dueno">Ventana dueña del cuadro.</param>
        /// <param name="titulo">Título del cuadro.</param>
        /// <param name="pregunta">Pregunta que se le hace al usuario.</param>
        /// <returns>true si el usuario contestó Sí; false si contestó No.</returns>
        public static bool preguntar(Window dueno, string titulo, string pregunta)
        {
            MensajeCodelex ventana = new MensajeCodelex(titulo, pregunta, true);

            ventana.pathCheck.Visibility = Visibility.Collapsed;
            ventana.pathPregunta.Visibility = Visibility.Visible;
            ventana.btnAceptar.Visibility = Visibility.Collapsed;
            ventana.gridPregunta.Visibility = Visibility.Visible;

            if (dueno != null)
            {
                ventana.Owner = dueno;
                ventana.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                ventana.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            ventana.ShowDialog();
            return ventana.confirmado;
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
        /// Atiende el clic de aceptar.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void aceptarClic(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Atiende el clic de Sí.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void siClic(object sender, RoutedEventArgs e)
        {
            confirmado = true;
            Close();
        }

        /// <summary>
        /// Atiende el clic de No.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void noClic(object sender, RoutedEventArgs e)
        {
            confirmado = false;
            Close();
        }
    }
}
