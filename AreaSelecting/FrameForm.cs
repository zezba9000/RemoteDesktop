using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaptureAreaSelector
{

    public partial class FrameForm : Form
    {
        private static readonly Color PrimaryTransparencyKey = Color.White;
        private static readonly Color SecondaryTransparencyKey = Color.Black;

        /// <summary>
        /// 選択範囲の位置と範囲です。
        /// </summary>
        [Category("配置"), Description("選択範囲の位置と範囲です")]
        public Rectangle SelectedWindow
        {
            get
            {
                var rect = this.Bounds;
                rect.Inflate(_FrameBorderSize * -1, _FrameBorderSize * -1);
                return rect;
            }
        }

        private Color _FrameColor = Color.Red;
        /// <summary>
        /// フレームの色です
        /// </summary>
        [Category("表示"), Description("フレームの色です")]
        [DefaultValue(typeof(Color), "Red")]
        public Color FrameColor
        {
            get { return _FrameColor; }
            set
            {
                _FrameColor = value;
                if (_FrameColor == PrimaryTransparencyKey)
                    this.TransparencyKey = SecondaryTransparencyKey;
                else
                    this.TransparencyKey = PrimaryTransparencyKey;
                this.Refresh();
            }
        }

        private int _FrameBorderSize = 5;
        /// <summary>
        /// フレームの線の太さです
        /// </summary>
        [Category("表示"), Description("フレームの線の太さです")]
        [DefaultValue(5)]
        public int FrameBorderSize
        {
            get { return _FrameBorderSize; }
            set
            {
                _FrameBorderSize = value;
                this.Refresh();
            }
        }

        /// <summary>
        /// フレームの変形を許可します
        /// </summary>
        [Category("動作"), Description("フレームの変形を許可します")]
        [DefaultValue(true)]
        public bool AllowedTransform { get; set; } = true;

        Point mousePoint; //マウス位置の一時記憶

        public FrameForm()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.None;
            this.TransparencyKey = PrimaryTransparencyKey;
        }

        /// <summary>
        /// 枠だけ残して透明にする
        /// </summary>
        /// <param name="g"></param>
        void draw(Graphics g)
        {
            var rct = this.ClientRectangle;
            g.FillRectangle(new SolidBrush(FrameColor), rct);
            rct.Inflate(FrameBorderSize * -1, FrameBorderSize * -1);
            g.FillRectangle(new SolidBrush(TransparencyKey), rct);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                //位置を記憶する
                mousePoint = new Point(e.X, e.Y);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (AllowedTransform)
            {
                if (e.X > Width * 2 / 3 && e.Y > Height * 1 / 2) //カーソルが右下なら
                    frameTransform(e);
                else
                    frameMove(e);
            }
            else
                frameMove(e);

            base.OnMouseMove(e);
        }

        void frameMove(MouseEventArgs e)
        {
            Cursor = Cursors.SizeAll;

            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                this.Left += e.X - mousePoint.X;
                this.Top += e.Y - mousePoint.Y;
            }
        }

        void frameTransform(MouseEventArgs e)
        {
            if (AllowedTransform)
            {
                Cursor = Cursors.SizeNWSE;

                if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
                {

                    this.Width += e.X - mousePoint.X;
                    mousePoint.X += e.X - mousePoint.X;

                    this.Height += e.Y - mousePoint.Y;
                    mousePoint.Y += e.Y - mousePoint.Y;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            draw(e.Graphics);
            base.OnPaint(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.Refresh();
        }
    }
}
