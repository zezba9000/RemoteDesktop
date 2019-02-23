using RemoteDesktop.Android.Core;
using Xamarin.Forms;

namespace RemoteDesktop.Client.Android
{
	public class SettingPage : ContentPage
	{
        public static string filledIPAddress = "";
        private Entry ipAddrEntry;

		public SettingPage ()
		{
			Title = "Setting";
            Content = new StackLayout {
                Children = {
                    new Label {
                        Text = "接続先サーバのIPアドレスを指定",
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                        //VerticalOptions = LayoutOptions.CenterAndExpand
                    },
                    { ipAddrEntry = new Entry {
                        Text = GlobalConfiguration.ServerAddress,
                        Keyboard = Keyboard.Url,
                        HorizontalTextAlignment = TextAlignment.Center,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
//                        VerticalOptions = LayoutOptions.CenterAndExpand
                    } }
                }
			};
		}

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            GlobalConfiguration.ServerAddress = ipAddrEntry.Text;
        }
    }
}
