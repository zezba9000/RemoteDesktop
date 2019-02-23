using RemoteDesktop.Android.Core;
using System;
using Xamarin.Forms;

namespace RemoteDesktop.Client.Android
{
	public class SettingPage : ContentPage
	{
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
                    //{ ipAddrEntry = new Entry {
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
            ipAddrEntry.Unfocused += OnUnfocused;
		}

        // コントロールからカーソルが離れた瞬間に発火するイベント
        private void OnUnfocused(object sender, EventArgs eventArgs)
        {
            GlobalConfiguration.ServerAddress = ((Entry)sender).Text;
            //Device.BeginInvokeOnMainThread(() =>
            //{
            //    DisplayAlert("", ((Entry)sender).Text, "OK");
            //});
        }

    }
}
