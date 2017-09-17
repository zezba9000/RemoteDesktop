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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RemoteDesktop.Client
{
	/// <summary>
	/// Interaction logic for SettingOverlay.xaml
	/// </summary>
	public partial class SettingOverlay : UserControl
	{
		public SettingOverlay()
		{
			InitializeComponent();
		}

		public void Show()
		{
			
			Visibility = Visibility.Visible;
		}

		private void applyButton_Click(object sender, RoutedEventArgs e)
		{
			
			Visibility = Visibility.Hidden;
		}
	}
}
