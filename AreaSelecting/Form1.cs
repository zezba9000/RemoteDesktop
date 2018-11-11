using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace CaptureAreaSelector
{
    public partial class Form1 : Form
    {

        FrameForm frame;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            frame = new FrameForm();

            frame.ShowInTaskbar = false; //タスクバーに表示させない

            frame.FrameBorderSize = 10; //線の太さ

            frame.FrameColor = Color.Blue; //線の色

            frame.AllowedTransform = true; //サイズ変更の可否

            frame.Show();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (frame != null)
                MessageBox.Show(frame.SelectedWindow.ToString());
        }
    }
}
