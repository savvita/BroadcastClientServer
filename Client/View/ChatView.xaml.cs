using Client.ViewModel;
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

namespace Client.View
{
    /// <summary>
    /// Interaction logic for ChatView.xaml
    /// </summary>
    public partial class ChatView : Window
    {
        public ChatView()
        {
            InitializeComponent();
            this.DataContext = new ClientViewModel();
        }

        private void DoubleAnimation_Completed(object sender, EventArgs e)
        {
            msg.Text = String.Empty;
            msg.Opacity = (double)msg.GetAnimationBaseValue(OpacityProperty);
        }
    }
}
