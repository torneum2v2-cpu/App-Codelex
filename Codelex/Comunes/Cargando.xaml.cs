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

        public Cargando()
        {
            InitializeComponent();

            inicio = DateTime.Now;
            temporizador.Tick += temporizadorTic;

            DoubleAnimation animacion = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(1));
            animacion.RepeatBehavior = RepeatBehavior.Forever;
            giro.BeginAnimation(RotateTransform.AngleProperty, animacion);
        }

        public static void mostrar(Window destino)
        {
            Cargando cargando = new Cargando();
            cargando.Show();

            destino.ContentRendered += cargando.destinoListo;
            destino.Show();
        }

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

        private void destinoListo(object sender, EventArgs e)
        {
            Window destino = (Window)sender;
            destino.ContentRendered -= destinoListo;
            terminar();
        }

        private void temporizadorTic(object sender, EventArgs e)
        {
            temporizador.Stop();

            if (!cerrada)
                Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            cerrada = true;
            temporizador.Stop();
            base.OnClosed(e);
        }
    }
}
