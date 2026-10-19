using System.Windows;

namespace AppCodelex;

/// <summary>
/// Interaction logic for Menu.xaml
/// </summary>
public partial class Menu : Window
{
    public Menu()
    {
        InitializeComponent();
    }

    private void Clientes_Click(object sender, RoutedEventArgs e)
    {
        Clientes ventana = new Clientes();
        ventana.Show();
    }
}
