using Codelex.BDD;
using System.Windows;

namespace Codelex
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            int cantidad = CuentasRecordadas.Listar().Count;

            if (cantidad == 0)
                new MainWindow().Show();
            else
                new SeleccionarCuenta(cantidad == 1).Show();
        }
    }
}