using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Neptun
{
    /// <summary>
    /// Interaction logic for MessageReadDialogBox.xaml
    /// </summary>
    public partial class MessageReadDialogBox : BaseMessageUserControl
    {
        public MessageReadDialogBox()
        {
            
            InitializeComponent();
        }

		private void Hyperlink_MouseLeftButtonDown(object sender, System.Windows.RoutedEventArgs e)
		{
            var hyperlink = (Hyperlink)sender;
            Process.Start(hyperlink.NavigateUri.ToString());
        }
	}
}
