using Codelex.BDD;
using System.Windows;

namespace Codelex
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Cualquier error no controlado (por ejemplo, base de datos caída) muestra un aviso y el programa continúa.
            DispatcherUnhandledException += (s, args) =>
            {
                args.Handled = true;
                MostrarErrorGlobal(args.Exception);
            };
            TaskScheduler.UnobservedTaskException += (s, args) => args.SetObserved();

            int cantidad = CuentasRecordadas.Listar().Count;

            if (cantidad == 0)
                new MainWindow().Show();
            else
                new SeleccionarCuenta(cantidad == 1).Show();
        }

        private static void MostrarErrorGlobal(Exception ex)
        {
            string mensaje = ex.Message;
            if (ex is Microsoft.Data.SqlClient.SqlException || ex.InnerException is Microsoft.Data.SqlClient.SqlException)
                mensaje = "No se pudo conectar a la base de datos. Verifica que el servidor esté encendido.\n\n" + ex.Message;

            MessageBox.Show(mensaje, "Codelex - Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}