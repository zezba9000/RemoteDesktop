using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;

using RemoteDesktop.Core;

namespace RemoteDesktop.Server
{
	// NOTE: this window is only for debugging
	public partial class MainForm : Form
	{
		private Bitmap bitmap;
		private Graphics graphics;
		private Timer timer;

		public MainForm()
		{
			InitializeComponent();
			
			timer = new Timer();
			timer.Interval = 1000 / 60;
			timer.Tick += Timer_Tick;
			timer.Start();
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			var size = this.ClientSize;
			pictureBox.Size = new Size(size.Width - 20, size.Height - 20);
			pictureBox.Location = new Point(10, 10);
			base.OnSizeChanged(e);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (graphics != null)
			{
				graphics.Dispose();
				graphics = null;
			}

			if (bitmap != null)
			{
				bitmap.Dispose();
				bitmap = null;
			}

			base.OnClosing(e);
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			CaptureScreen();
			if (pictureBox.Image == null) pictureBox.Image = bitmap;
			else pictureBox.Refresh();
		}

		private void CaptureScreen()
		{
			if (bitmap == null)
			{
				var screen = Screen.FromControl(this);
				var screenRect = screen.WorkingArea;

				bitmap = new Bitmap(screenRect.Width, screenRect.Height, PixelFormat.Format32bppRgb);
				graphics = Graphics.FromImage(bitmap);
			}

			graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
		}
	}
}
