using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Codelex
{
    public partial class Cargando : Window
    {
        private const int TiempoMinimo = 300;

        private DateTime _inicio;
        private DispatcherTimer _timer = new DispatcherTimer();
        private bool _cerrada = false;

        public Cargando()
        {
            InitializeComponent();

            _inicio = DateTime.Now;
            _timer.Tick += Timer_Tick;

            DoubleAnimation animacion = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(1));
            animacion.RepeatBehavior = RepeatBehavior.Forever;
            giro.BeginAnimation(RotateTransform.AngleProperty, animacion);
        }

        public static void Mostrar(Window destino)
        {
            Cargando cargando = new Cargando();
            cargando.Show();

            destino.ContentRendered += cargando.DestinoListo;
            destino.Show();
        }

        public static Cargando Iniciar(Window dueno, string texto)
        {
            Cargando cargando = new Cargando();
            cargando.txtTexto.Text = texto;
            cargando.Owner = dueno;
            cargando.Topmost = false;
            cargando.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            cargando.Show();
            return cargando;
        }

        public void Terminar()
        {
            if (_cerrada)
                return;

            double faltante = TiempoMinimo - (DateTime.Now - _inicio).TotalMilliseconds;

            if (faltante <= 0)
            {
                Close();
                return;
            }

            _timer.Interval = TimeSpan.FromMilliseconds(faltante);
            _timer.Start();
        }

        private void DestinoListo(object sender, EventArgs e)
        {
            Window destino = (Window)sender;
            destino.ContentRendered -= DestinoListo;
            Terminar();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();

            if (!_cerrada)
                Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _cerrada = true;
            _timer.Stop();
            base.OnClosed(e);
        }
    }
}
