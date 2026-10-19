using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Codelex
{
    /// <summary>
    /// Lógica de interacción para Recuperacion.xaml
    /// </summary>
    public partial class Recuperacion : Window
    {

        


        public Recuperacion()
        {
            InitializeComponent();
        }

        private void BtnSendCode_Click(object sender, RoutedEventArgs e)
        {
            
            try
            {
                var recuperar = new RecuperarCuenta();
                string resultado = recuperar.recoverPassword(txtEmail.Text.Trim());
                MessageBox.Show(resultado);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());   // muestra el error completo
            }
        }

        private void BtnResetPassword_Click(object sender, EventArgs e)
        {
            // tu código aquí
        }
    }
}
