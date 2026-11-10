using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Codelex
{
    public partial class Cargando : Window
    {
        private const int TIEMPO_MINIMO = 300;

        private DateTime inicio;
        private DispatcherTimer temporizador = new DispatcherTimer();
        private bool cerrada = false;

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        public Cargando()
        {
            InitializeComponent();

            inicio = DateTime.Now;
            temporizador.Tick += temporizadorTic;

            DoubleAnimation animacion = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(1));
            animacion.RepeatBehavior = RepeatBehavior.Forever;
            giro.BeginAnimation(RotateTransform.AngleProperty, animacion);
        }

        /// <summary>
        /// Muestra la ventana destino y cierra la de carga.
        /// </summary>
        /// <param name="destino">Ventana que se va a mostrar.</param>
        public static void mostrar(Window destino)
        {
            Cargando cargando = new Cargando();
            cargando.Show();

            destino.ContentRendered += cargando.destinoListo;
            destino.Show();
        }

        /// <summary>
        /// Muestra la ventana de carga sobre otra ventana.
        /// </summary>
        /// <param name="dueno">Ventana dueña del cuadro.</param>
        /// <param name="texto">Texto a usar.</param>
        /// <returns>Resultado de tipo Cargando.</returns>
        public static Cargando iniciar(Window dueno, string texto)
        {
            Cargando cargando = new Cargando();
            cargando.txtTexto.Text = texto;
            cargando.Owner = dueno;
            cargando.Topmost = false;
            cargando.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            cargando.Show();
            return cargando;
        }

        /// <summary>
        /// Cierra la ventana de carga.
        /// </summary>
        public void terminar()
        {
            if (cerrada)
                return;

            double faltante = TIEMPO_MINIMO - (DateTime.Now - inicio).TotalMilliseconds;

            if (faltante <= 0)
            {
                Close();
                return;
            }

            temporizador.Interval = TimeSpan.FromMilliseconds(faltante);
            temporizador.Start();
        }

        /// <summary>
        /// Cierra la ventana de carga cuando la ventana destino ya se mostró.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void destinoListo(object sender, EventArgs e)
        {
            Window destino = (Window)sender;
            destino.ContentRendered -= destinoListo;
            terminar();
        }

        /// <summary>
        /// Atiende cada tic de temporizador.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void temporizadorTic(object sender, EventArgs e)
        {
            temporizador.Stop();

            if (!cerrada)
                Close();
        }

        /// <summary>
        /// Libera la ventana de carga al cerrarse.
        /// </summary>
        /// <param name="e">Datos del evento.</param>
        protected override void OnClosed(EventArgs e)
        {
            cerrada = true;
            temporizador.Stop();
            base.OnClosed(e);
        }
    }
}
