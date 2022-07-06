using ServerWPF.Model;
using System.Windows;
using System.Windows.Controls;

namespace ServerWPF.View
{
    public class BannedStyleSelector : StyleSelector
    {

        public Style? BannedStyle { get; set; }
        public Style? UnbannedStyle { get; set; }

        public override Style? SelectStyle(object item, DependencyObject container)
        {
            ClientModel client = (ClientModel)item;

            switch (client.IsBanned)
            {
                case true:
                    return BannedStyle;
                case false:
                    return UnbannedStyle;
            }
        }
    }
}
