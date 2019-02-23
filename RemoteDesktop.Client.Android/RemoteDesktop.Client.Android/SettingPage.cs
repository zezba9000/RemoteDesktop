using RemoteDesktop.Android.Core;
using System;
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
            ipAddrEntry.Completed += OnCompleted;
		}

        // コントロールからカーソルが離れた瞬間に発火するイベント
        private void OnCompleted(object sender, EventArgs eventArgs)
        {
            GlobalConfiguration.ServerAddress = ipAddrEntry.Text;
        }

    }
}
