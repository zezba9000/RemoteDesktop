namespace MulticastPlayer
{
    partial class FormMain
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.ButtonStart = new System.Windows.Forms.Button();
            this.TextBoxMCAddress = new System.Windows.Forms.TextBox();
            this.TextBoxMCPort = new System.Windows.Forms.TextBox();
            this.LabelBitsPerSample = new System.Windows.Forms.Label();
            this.LabelChannels = new System.Windows.Forms.Label();
            this.LabelSamplesPerSecond = new System.Windows.Forms.Label();
            this.LabelMCAddress = new System.Windows.Forms.Label();
            this.LabelMCPort = new System.Windows.Forms.Label();
            this.LabelSoundDeviceName = new System.Windows.Forms.Label();
            this.ComboboxSoundDeviceName = new System.Windows.Forms.ComboBox();
            this.ComboboxBitsPerSample = new System.Windows.Forms.ComboBox();
            this.ComboboxChannels = new System.Windows.Forms.ComboBox();
            this.ComboboxSamplesPerSecond = new System.Windows.Forms.ComboBox();
            this.LabelBufferSize = new System.Windows.Forms.Label();
            this.NumericUpDownPacketSize = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.ComboboxBufferCount = new System.Windows.Forms.ComboBox();
            this.GroupBoxBuffer = new System.Windows.Forms.GroupBox();
            this.GroupBoxSource = new System.Windows.Forms.GroupBox();
            this.GroupBoxSound = new System.Windows.Forms.GroupBox();
            this.CheckBoxDrawCurve = new System.Windows.Forms.CheckBox();
            this.PanelCurve = new System.Windows.Forms.Panel();
            this.ProgressBarJitterBuffer = new System.Windows.Forms.ProgressBar();
            this.NumericUpDownJitterBuffer = new System.Windows.Forms.NumericUpDown();
            this.LabelJitterBuffer = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.NumericUpDownPacketSize)).BeginInit();
            this.GroupBoxBuffer.SuspendLayout();
            this.GroupBoxSource.SuspendLayout();
            this.GroupBoxSound.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericUpDownJitterBuffer)).BeginInit();
            this.SuspendLayout();
            // 
            // ButtonStart
            // 
            this.ButtonStart.BackColor = System.Drawing.Color.Gainsboro;
            this.ButtonStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 22F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonStart.Location = new System.Drawing.Point(248, 23);
            this.ButtonStart.Margin = new System.Windows.Forms.Padding(2);
            this.ButtonStart.Name = "ButtonStart";
            this.ButtonStart.Size = new System.Drawing.Size(174, 49);
            this.ButtonStart.TabIndex = 0;
            this.ButtonStart.Text = "Start";
            this.ButtonStart.UseVisualStyleBackColor = false;
            this.ButtonStart.Click += new System.EventHandler(this.ButtonStart_Click);
            // 
            // TextBoxMCAddress
            // 
            this.TextBoxMCAddress.BackColor = System.Drawing.Color.White;
            this.TextBoxMCAddress.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxMCAddress.ForeColor = System.Drawing.Color.DimGray;
            this.TextBoxMCAddress.Location = new System.Drawing.Point(58, 22);
            this.TextBoxMCAddress.Margin = new System.Windows.Forms.Padding(2);
            this.TextBoxMCAddress.Name = "TextBoxMCAddress";
            this.TextBoxMCAddress.Size = new System.Drawing.Size(115, 20);
            this.TextBoxMCAddress.TabIndex = 1;
            this.TextBoxMCAddress.Text = "239.192.17.20";
            this.TextBoxMCAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TextBoxMCPort
            // 
            this.TextBoxMCPort.BackColor = System.Drawing.Color.White;
            this.TextBoxMCPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxMCPort.ForeColor = System.Drawing.Color.DimGray;
            this.TextBoxMCPort.Location = new System.Drawing.Point(207, 22);
            this.TextBoxMCPort.Margin = new System.Windows.Forms.Padding(2);
            this.TextBoxMCPort.Name = "TextBoxMCPort";
            this.TextBoxMCPort.Size = new System.Drawing.Size(59, 20);
            this.TextBoxMCPort.TabIndex = 2;
            this.TextBoxMCPort.Text = "16400";
            this.TextBoxMCPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // LabelBitsPerSample
            // 
            this.LabelBitsPerSample.AutoSize = true;
            this.LabelBitsPerSample.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelBitsPerSample.Location = new System.Drawing.Point(5, 54);
            this.LabelBitsPerSample.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LabelBitsPerSample.Name = "LabelBitsPerSample";
            this.LabelBitsPerSample.Size = new System.Drawing.Size(80, 13);
            this.LabelBitsPerSample.TabIndex = 6;
            this.LabelBitsPerSample.Text = "Bits per Sample";
            // 
            // LabelChannels
            // 
            this.LabelChannels.AutoSize = true;
            this.LabelChannels.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelChannels.Location = new System.Drawing.Point(5, 81);
            this.LabelChannels.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LabelChannels.Name = "LabelChannels";
            this.LabelChannels.Size = new System.Drawing.Size(51, 13);
            this.LabelChannels.TabIndex = 7;
            this.LabelChannels.Text = "Channels";
            // 
            // LabelSamplesPerSecond
            // 
            this.LabelSamplesPerSecond.AutoSize = true;
            this.LabelSamplesPerSecond.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelSamplesPerSecond.Location = new System.Drawing.Point(5, 26);
            this.LabelSamplesPerSecond.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LabelSamplesPerSecond.Name = "LabelSamplesPerSecond";
            this.LabelSamplesPerSecond.Size = new System.Drawing.Size(105, 13);
            this.LabelSamplesPerSecond.TabIndex = 8;
            this.LabelSamplesPerSecond.Text = "Samples per Second";
            // 
            // LabelMCAddress
            // 
            this.LabelMCAddress.AutoSize = true;
            this.LabelMCAddress.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelMCAddress.Location = new System.Drawing.Point(9, 25);
            this.LabelMCAddress.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LabelMCAddress.Name = "LabelMCAddress";
            this.LabelMCAddress.Size = new System.Drawing.Size(45, 13);
            this.LabelMCAddress.TabIndex = 9;
            this.LabelMCAddress.Text = "Address";
            // 
            // LabelMCPort
            // 
            this.LabelMCPort.AutoSize = true;
            this.LabelMCPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelMCPort.Location = new System.Drawing.Point(177, 25);
            this.LabelMCPort.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LabelMCPort.Name = "LabelMCPort";
            this.LabelMCPort.Size = new System.Drawing.Size(26, 13);
            this.LabelMCPort.TabIndex = 10;
            this.LabelMCPort.Text = "Port";
            // 
            // LabelSoundDeviceName
            // 
            this.LabelSoundDeviceName.AutoSize = true;
            this.LabelSoundDeviceName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelSoundDeviceName.Location = new System.Drawing.Point(5, 60);
            this.LabelSoundDeviceName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LabelSoundDeviceName.Name = "LabelSoundDeviceName";
            this.LabelSoundDeviceName.Size = new System.Drawing.Size(75, 13);
            this.LabelSoundDeviceName.TabIndex = 11;
            this.LabelSoundDeviceName.Text = "Sound Device";
            // 
            // ComboboxSoundDeviceName
            // 
            this.ComboboxSoundDeviceName.BackColor = System.Drawing.Color.White;
            this.ComboboxSoundDeviceName.DropDownHeight = 800;
            this.ComboboxSoundDeviceName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboboxSoundDeviceName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ComboboxSoundDeviceName.ForeColor = System.Drawing.Color.Black;
            this.ComboboxSoundDeviceName.FormattingEnabled = true;
            this.ComboboxSoundDeviceName.IntegralHeight = false;
            this.ComboboxSoundDeviceName.Location = new System.Drawing.Point(84, 56);
            this.ComboboxSoundDeviceName.Margin = new System.Windows.Forms.Padding(2);
            this.ComboboxSoundDeviceName.Name = "ComboboxSoundDeviceName";
            this.ComboboxSoundDeviceName.Size = new System.Drawing.Size(182, 21);
            this.ComboboxSoundDeviceName.TabIndex = 12;
            // 
            // ComboboxBitsPerSample
            // 
            this.ComboboxBitsPerSample.BackColor = System.Drawing.Color.White;
            this.ComboboxBitsPerSample.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboboxBitsPerSample.FormattingEnabled = true;
            this.ComboboxBitsPerSample.Items.AddRange(new object[] {
            "16",
            "8"});
            this.ComboboxBitsPerSample.Location = new System.Drawing.Point(121, 51);
            this.ComboboxBitsPerSample.Margin = new System.Windows.Forms.Padding(2);
            this.ComboboxBitsPerSample.Name = "ComboboxBitsPerSample";
            this.ComboboxBitsPerSample.Size = new System.Drawing.Size(113, 21);
            this.ComboboxBitsPerSample.TabIndex = 13;
            // 
            // ComboboxChannels
            // 
            this.ComboboxChannels.BackColor = System.Drawing.Color.White;
            this.ComboboxChannels.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboboxChannels.FormattingEnabled = true;
            this.ComboboxChannels.Items.AddRange(new object[] {
            "2",
            "1"});
            this.ComboboxChannels.Location = new System.Drawing.Point(121, 78);
            this.ComboboxChannels.Margin = new System.Windows.Forms.Padding(2);
            this.ComboboxChannels.Name = "ComboboxChannels";
            this.ComboboxChannels.Size = new System.Drawing.Size(113, 21);
            this.ComboboxChannels.TabIndex = 14;
            // 
            // ComboboxSamplesPerSecond
            // 
            this.ComboboxSamplesPerSecond.BackColor = System.Drawing.Color.White;
            this.ComboboxSamplesPerSecond.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboboxSamplesPerSecond.FormattingEnabled = true;
            this.ComboboxSamplesPerSecond.Items.AddRange(new object[] {
            "5000",
            "8000",
            "11025",
            "22050",
            "44100"});
            this.ComboboxSamplesPerSecond.Location = new System.Drawing.Point(121, 23);
            this.ComboboxSamplesPerSecond.Margin = new System.Windows.Forms.Padding(2);
            this.ComboboxSamplesPerSecond.Name = "ComboboxSamplesPerSecond";
            this.ComboboxSamplesPerSecond.Size = new System.Drawing.Size(113, 21);
            this.ComboboxSamplesPerSecond.TabIndex = 15;
            // 
            // LabelBufferSize
            // 
            this.LabelBufferSize.AutoSize = true;
            this.LabelBufferSize.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelBufferSize.Location = new System.Drawing.Point(3, 27);
            this.LabelBufferSize.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LabelBufferSize.Name = "LabelBufferSize";
            this.LabelBufferSize.Size = new System.Drawing.Size(64, 13);
            this.LabelBufferSize.TabIndex = 16;
            this.LabelBufferSize.Text = "Packet Size";
            // 
            // NumericUpDownPacketSize
            // 
            this.NumericUpDownPacketSize.BackColor = System.Drawing.Color.White;
            this.NumericUpDownPacketSize.Location = new System.Drawing.Point(73, 23);
            this.NumericUpDownPacketSize.Margin = new System.Windows.Forms.Padding(2);
            this.NumericUpDownPacketSize.Maximum = new decimal(new int[] {
            16000,
            0,
            0,
            0});
            this.NumericUpDownPacketSize.Minimum = new decimal(new int[] {
            172,
            0,
            0,
            0});
            this.NumericUpDownPacketSize.Name = "NumericUpDownPacketSize";
            this.NumericUpDownPacketSize.Size = new System.Drawing.Size(66, 20);
            this.NumericUpDownPacketSize.TabIndex = 17;
            this.NumericUpDownPacketSize.Value = new decimal(new int[] {
            16000,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 60);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "Buffer Count";
            // 
            // ComboboxBufferCount
            // 
            this.ComboboxBufferCount.BackColor = System.Drawing.Color.White;
            this.ComboboxBufferCount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboboxBufferCount.FormattingEnabled = true;
            this.ComboboxBufferCount.Items.AddRange(new object[] {
            "1",
            "2",
            "4",
            "6",
            "8",
            "10",
            "12",
            "16",
            "24",
            "32",
            "64"});
            this.ComboboxBufferCount.Location = new System.Drawing.Point(73, 57);
            this.ComboboxBufferCount.Margin = new System.Windows.Forms.Padding(2);
            this.ComboboxBufferCount.Name = "ComboboxBufferCount";
            this.ComboboxBufferCount.Size = new System.Drawing.Size(66, 21);
            this.ComboboxBufferCount.TabIndex = 19;
            // 
            // GroupBoxBuffer
            // 
            this.GroupBoxBuffer.Controls.Add(this.LabelBufferSize);
            this.GroupBoxBuffer.Controls.Add(this.ComboboxBufferCount);
            this.GroupBoxBuffer.Controls.Add(this.NumericUpDownPacketSize);
            this.GroupBoxBuffer.Controls.Add(this.label1);
            this.GroupBoxBuffer.Location = new System.Drawing.Point(287, 5);
            this.GroupBoxBuffer.Name = "GroupBoxBuffer";
            this.GroupBoxBuffer.Size = new System.Drawing.Size(149, 90);
            this.GroupBoxBuffer.TabIndex = 20;
            this.GroupBoxBuffer.TabStop = false;
            this.GroupBoxBuffer.Text = "Buffer";
            // 
            // GroupBoxSource
            // 
            this.GroupBoxSource.Controls.Add(this.LabelMCAddress);
            this.GroupBoxSource.Controls.Add(this.TextBoxMCAddress);
            this.GroupBoxSource.Controls.Add(this.TextBoxMCPort);
            this.GroupBoxSource.Controls.Add(this.LabelMCPort);
            this.GroupBoxSource.Controls.Add(this.LabelSoundDeviceName);
            this.GroupBoxSource.Controls.Add(this.ComboboxSoundDeviceName);
            this.GroupBoxSource.Location = new System.Drawing.Point(4, 5);
            this.GroupBoxSource.Name = "GroupBoxSource";
            this.GroupBoxSource.Size = new System.Drawing.Size(276, 90);
            this.GroupBoxSource.TabIndex = 21;
            this.GroupBoxSource.TabStop = false;
            this.GroupBoxSource.Text = "Source";
            // 
            // GroupBoxSound
            // 
            this.GroupBoxSound.Controls.Add(this.CheckBoxDrawCurve);
            this.GroupBoxSound.Controls.Add(this.LabelSamplesPerSecond);
            this.GroupBoxSound.Controls.Add(this.LabelBitsPerSample);
            this.GroupBoxSound.Controls.Add(this.LabelChannels);
            this.GroupBoxSound.Controls.Add(this.ButtonStart);
            this.GroupBoxSound.Controls.Add(this.ComboboxSamplesPerSecond);
            this.GroupBoxSound.Controls.Add(this.ComboboxBitsPerSample);
            this.GroupBoxSound.Controls.Add(this.ComboboxChannels);
            this.GroupBoxSound.Location = new System.Drawing.Point(4, 95);
            this.GroupBoxSound.Name = "GroupBoxSound";
            this.GroupBoxSound.Size = new System.Drawing.Size(432, 104);
            this.GroupBoxSound.TabIndex = 22;
            this.GroupBoxSound.TabStop = false;
            this.GroupBoxSound.Text = "Sound";
            // 
            // CheckBoxDrawCurve
            // 
            this.CheckBoxDrawCurve.AutoSize = true;
            this.CheckBoxDrawCurve.Location = new System.Drawing.Point(329, 82);
            this.CheckBoxDrawCurve.Name = "CheckBoxDrawCurve";
            this.CheckBoxDrawCurve.Size = new System.Drawing.Size(84, 17);
            this.CheckBoxDrawCurve.TabIndex = 0;
            this.CheckBoxDrawCurve.Text = "Show Curve";
            this.CheckBoxDrawCurve.UseVisualStyleBackColor = true;
            this.CheckBoxDrawCurve.CheckedChanged += new System.EventHandler(this.CheckBoxDrawCurve_CheckedChanged);
            // 
            // PanelCurve
            // 
            this.PanelCurve.BackColor = System.Drawing.Color.Black;
            this.PanelCurve.Location = new System.Drawing.Point(4, 235);
            this.PanelCurve.Name = "PanelCurve";
            this.PanelCurve.Size = new System.Drawing.Size(432, 314);
            this.PanelCurve.TabIndex = 23;
            // 
            // ProgressBarJitterBuffer
            // 
            this.ProgressBarJitterBuffer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ProgressBarJitterBuffer.Location = new System.Drawing.Point(4, 209);
            this.ProgressBarJitterBuffer.Name = "ProgressBarJitterBuffer";
            this.ProgressBarJitterBuffer.Size = new System.Drawing.Size(313, 14);
            this.ProgressBarJitterBuffer.Step = 1;
            this.ProgressBarJitterBuffer.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.ProgressBarJitterBuffer.TabIndex = 24;
            // 
            // NumericUpDownJitterBuffer
            // 
            this.NumericUpDownJitterBuffer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.NumericUpDownJitterBuffer.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NumericUpDownJitterBuffer.Location = new System.Drawing.Point(372, 204);
            this.NumericUpDownJitterBuffer.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.NumericUpDownJitterBuffer.Name = "NumericUpDownJitterBuffer";
            this.NumericUpDownJitterBuffer.Size = new System.Drawing.Size(64, 21);
            this.NumericUpDownJitterBuffer.TabIndex = 25;
            this.NumericUpDownJitterBuffer.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.NumericUpDownJitterBuffer.ValueChanged += new System.EventHandler(this.NumericUpDownJitterBuffer_ValueChanged);
            // 
            // LabelJitterBuffer
            // 
            this.LabelJitterBuffer.AutoSize = true;
            this.LabelJitterBuffer.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelJitterBuffer.Location = new System.Drawing.Point(322, 206);
            this.LabelJitterBuffer.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LabelJitterBuffer.Name = "LabelJitterBuffer";
            this.LabelJitterBuffer.Size = new System.Drawing.Size(45, 17);
            this.LabelJitterBuffer.TabIndex = 26;
            this.LabelJitterBuffer.Text = "Jitter";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(440, 229);
            this.Controls.Add(this.LabelJitterBuffer);
            this.Controls.Add(this.NumericUpDownJitterBuffer);
            this.Controls.Add(this.ProgressBarJitterBuffer);
            this.Controls.Add(this.PanelCurve);
            this.Controls.Add(this.GroupBoxSound);
            this.Controls.Add(this.GroupBoxSource);
            this.Controls.Add(this.GroupBoxBuffer);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MulticastPlayer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.NumericUpDownPacketSize)).EndInit();
            this.GroupBoxBuffer.ResumeLayout(false);
            this.GroupBoxBuffer.PerformLayout();
            this.GroupBoxSource.ResumeLayout(false);
            this.GroupBoxSource.PerformLayout();
            this.GroupBoxSound.ResumeLayout(false);
            this.GroupBoxSound.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericUpDownJitterBuffer)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ButtonStart;
        private System.Windows.Forms.TextBox TextBoxMCAddress;
        private System.Windows.Forms.TextBox TextBoxMCPort;
        private System.Windows.Forms.Label LabelBitsPerSample;
        private System.Windows.Forms.Label LabelChannels;
        private System.Windows.Forms.Label LabelSamplesPerSecond;
        private System.Windows.Forms.Label LabelMCAddress;
        private System.Windows.Forms.Label LabelMCPort;
        private System.Windows.Forms.Label LabelSoundDeviceName;
        private System.Windows.Forms.ComboBox ComboboxSoundDeviceName;
        private System.Windows.Forms.ComboBox ComboboxBitsPerSample;
        private System.Windows.Forms.ComboBox ComboboxChannels;
        private System.Windows.Forms.ComboBox ComboboxSamplesPerSecond;
        private System.Windows.Forms.Label LabelBufferSize;
        private System.Windows.Forms.NumericUpDown NumericUpDownPacketSize;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ComboboxBufferCount;
        private System.Windows.Forms.GroupBox GroupBoxBuffer;
        private System.Windows.Forms.GroupBox GroupBoxSource;
        private System.Windows.Forms.GroupBox GroupBoxSound;
        private System.Windows.Forms.Panel PanelCurve;
        private System.Windows.Forms.CheckBox CheckBoxDrawCurve;
        private System.Windows.Forms.ProgressBar ProgressBarJitterBuffer;
        private System.Windows.Forms.NumericUpDown NumericUpDownJitterBuffer;
        private System.Windows.Forms.Label LabelJitterBuffer;
    }
}

