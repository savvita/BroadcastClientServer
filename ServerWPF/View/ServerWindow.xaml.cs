using ServerWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ServerWPF.View
{
    /// <summary>
    /// Interaction logic for ServerWindow.xaml
    /// </summary>
    public partial class ServerWindow : Window
    {
        public ServerWindow()
        {
            InitializeComponent();
            this.DataContext = new ServerViewModel();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //StyleSelector selector = Clients.ItemContainerStyleSelector;
            //Clients.ItemContainerStyleSelector = null;
            //Clients.ItemContainerStyleSelector = selector;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Clients.SelectedIndex = -1;
            e.Handled = true;
        }

        private void DoubleAnimation_Completed(object sender, EventArgs e)
        {
            msg.Text = String.Empty;
            msg.Opacity = (double)msg.GetAnimationBaseValue(OpacityProperty);
        }
    }
}
