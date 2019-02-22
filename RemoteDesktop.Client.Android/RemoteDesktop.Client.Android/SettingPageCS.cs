using Xamarin.Forms;

namespace RemoteDesktop.Client.Android
{
	public class SettingPageCS : ContentPage
	{
		public SettingPageCS ()
		{
			Title = "Setting";
			Content = new StackLayout { 
				Children = {
					new Label {
						Text = "Todo list data goes here",
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.CenterAndExpand
					}
				}
			};
		}
	}
}
