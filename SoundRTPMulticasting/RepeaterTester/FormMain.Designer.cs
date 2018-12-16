namespace WinSoundTester
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
            this.ButtonStartRepeater = new System.Windows.Forms.Button();
            this.ComboboxWaveOut = new System.Windows.Forms.ComboBox();
            this.ComboboxWaveIn = new System.Windows.Forms.ComboBox();
            this.LabelWaveIn = new System.Windows.Forms.Label();
            this.LabelWaveOut = new System.Windows.Forms.Label();
            this.LabelSamplesPreSecond = new System.Windows.Forms.Label();
            this.ComboboxSamplesPerSecond = new System.Windows.Forms.ComboBox();
            this.LabelBitsPerSample = new System.Windows.Forms.Label();
            this.ComboboxBitsPerSample = new System.Windows.Forms.ComboBox();
            this.LabelChannels = new System.Windows.Forms.Label();
            this.ComboboxChannels = new System.Windows.Forms.ComboBox();
            this.LabelBufferCount = new System.Windows.Forms.Label();
            this.ComboboxBufferCount = new System.Windows.Forms.ComboBox();
            this.LabelBufferSize = new System.Windows.Forms.Label();
            this.ComboboxBufferSize = new System.Windows.Forms.ComboBox();
            this.CheckBoxMute = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // ButtonStartRepeater
            // 
            this.ButtonStartRepeater.BackColor = System.Drawing.SystemColors.Control;
            this.ButtonStartRepeater.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonStartRepeater.Location = new System.Drawing.Point(102, 201);
            this.ButtonStartRepeater.Name = "ButtonStartRepeater";
            this.ButtonStartRepeater.Size = new System.Drawing.Size(117, 32);
            this.ButtonStartRepeater.TabIndex = 0;
            this.ButtonStartRepeater.Text = "Start";
            this.ButtonStartRepeater.UseVisualStyleBackColor = false;
            this.ButtonStartRepeater.Click += new System.EventHandler(this.ButtonStartRepeater_Click);
            // 
            // ComboboxWaveOut
            // 
            this.ComboboxWaveOut.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ComboboxWaveOut.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboboxWaveOut.FormattingEnabled = true;
            this.ComboboxWaveOut.Location = new System.Drawing.Point(10, 69);
            this.ComboboxWaveOut.Name = "ComboboxWaveOut";
            this.ComboboxWaveOut.Size = new System.Drawing.Size(209, 21);
            this.ComboboxWaveOut.TabIndex = 1;
            // 
            // ComboboxWaveIn
            // 
            this.ComboboxWaveIn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ComboboxWaveIn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboboxWaveIn.FormattingEnabled = true;
            this.ComboboxWaveIn.Location = new System.Drawing.Point(10, 25);
            this.ComboboxWaveIn.Name = "ComboboxWaveIn";
            this.ComboboxWaveIn.Size = new System.Drawing.Size(209, 21);
            this.ComboboxWaveIn.TabIndex = 2;
            // 
            // LabelWaveIn
            // 
            this.LabelWaveIn.AutoSize = true;
            this.LabelWaveIn.Location = new System.Drawing.Point(8, 9);
            this.LabelWaveIn.Name = "LabelWaveIn";
            this.LabelWaveIn.Size = new System.Drawing.Size(48, 13);
            this.LabelWaveIn.TabIndex = 3;
            this.LabelWaveIn.Text = "Wave In";
            // 
            // LabelWaveOut
            // 
            this.LabelWaveOut.AutoSize = true;
            this.LabelWaveOut.Location = new System.Drawing.Point(8, 53);
            this.LabelWaveOut.Name = "LabelWaveOut";
            this.LabelWaveOut.Size = new System.Drawing.Size(56, 13);
            this.LabelWaveOut.TabIndex = 4;
            this.LabelWaveOut.Text = "Wave Out";
            // 
            // LabelSamplesPreSecond
            // 
            this.LabelSamplesPreSecond.AutoSize = true;
            this.LabelSamplesPreSecond.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelSamplesPreSecond.Location = new System.Drawing.Point(7, 108);
            this.LabelSamplesPreSecond.Name = "LabelSamplesPreSecond";
            this.LabelSamplesPreSecond.Size = new System.Drawing.Size(99, 13);
            this.LabelSamplesPreSecond.TabIndex = 6;
            this.LabelSamplesPreSecond.Text = "SamplesPerSecond";
            // 
            // ComboboxSamplesPerSecond
            // 
            this.ComboboxSamplesPerSecond.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ComboboxSamplesPerSecond.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboboxSamplesPerSecond.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ComboboxSamplesPerSecond.FormattingEnabled = true;
            this.ComboboxSamplesPerSecond.Location = new System.Drawing.Point(11, 124);
            this.ComboboxSamplesPerSecond.Name = "ComboboxSamplesPerSecond";
            this.ComboboxSamplesPerSecond.Size = new System.Drawing.Size(95, 20);
            this.ComboboxSamplesPerSecond.TabIndex = 5;
            // 
            // LabelBitsPerSample
            // 
            this.LabelBitsPerSample.AutoSize = true;
            this.LabelBitsPerSample.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelBitsPerSample.Location = new System.Drawing.Point(8, 152);
            this.LabelBitsPerSample.Name = "LabelBitsPerSample";
            this.LabelBitsPerSample.Size = new System.Drawing.Size(75, 13);
            this.LabelBitsPerSample.TabIndex = 8;
            this.LabelBitsPerSample.Text = "BitsPerSample";
            // 
            // ComboboxBitsPerSample
            // 
            this.ComboboxBitsPerSample.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ComboboxBitsPerSample.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboboxBitsPerSample.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ComboboxBitsPerSample.FormattingEnabled = true;
            this.ComboboxBitsPerSample.Location = new System.Drawing.Point(10, 168);
            this.ComboboxBitsPerSample.Name = "ComboboxBitsPerSample";
            this.ComboboxBitsPerSample.Size = new System.Drawing.Size(73, 20);
            this.ComboboxBitsPerSample.TabIndex = 7;
            // 
            // LabelChannels
            // 
            this.LabelChannels.AutoSize = true;
            this.LabelChannels.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelChannels.Location = new System.Drawing.Point(8, 193);
            this.LabelChannels.Name = "LabelChannels";
            this.LabelChannels.Size = new System.Drawing.Size(51, 13);
            this.LabelChannels.TabIndex = 10;
            this.LabelChannels.Text = "Channels";
            // 
            // ComboboxChannels
            // 
            this.ComboboxChannels.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ComboboxChannels.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboboxChannels.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ComboboxChannels.FormattingEnabled = true;
            this.ComboboxChannels.Location = new System.Drawing.Point(11, 209);
            this.ComboboxChannels.Name = "ComboboxChannels";
            this.ComboboxChannels.Size = new System.Drawing.Size(48, 20);
            this.ComboboxChannels.TabIndex = 9;
            // 
            // LabelBufferCount
            // 
            this.LabelBufferCount.AutoSize = true;
            this.LabelBufferCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelBufferCount.Location = new System.Drawing.Point(161, 108);
            this.LabelBufferCount.Name = "LabelBufferCount";
            this.LabelBufferCount.Size = new System.Drawing.Size(63, 13);
            this.LabelBufferCount.TabIndex = 12;
            this.LabelBufferCount.Text = "BufferCount";
            // 
            // ComboboxBufferCount
            // 
            this.ComboboxBufferCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ComboboxBufferCount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboboxBufferCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ComboboxBufferCount.FormattingEnabled = true;
            this.ComboboxBufferCount.Location = new System.Drawing.Point(163, 124);
            this.ComboboxBufferCount.Name = "ComboboxBufferCount";
            this.ComboboxBufferCount.Size = new System.Drawing.Size(52, 20);
            this.ComboboxBufferCount.TabIndex = 11;
            // 
            // LabelBufferSize
            // 
            this.LabelBufferSize.AutoSize = true;
            this.LabelBufferSize.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelBufferSize.Location = new System.Drawing.Point(161, 152);
            this.LabelBufferSize.Name = "LabelBufferSize";
            this.LabelBufferSize.Size = new System.Drawing.Size(55, 13);
            this.LabelBufferSize.TabIndex = 14;
            this.LabelBufferSize.Text = "BufferSize";
            // 
            // ComboboxBufferSize
            // 
            this.ComboboxBufferSize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ComboboxBufferSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboboxBufferSize.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ComboboxBufferSize.FormattingEnabled = true;
            this.ComboboxBufferSize.Location = new System.Drawing.Point(161, 168);
            this.ComboboxBufferSize.Name = "ComboboxBufferSize";
            this.ComboboxBufferSize.Size = new System.Drawing.Size(54, 20);
            this.ComboboxBufferSize.TabIndex = 13;
            // 
            // CheckBoxMute
            // 
            this.CheckBoxMute.AutoSize = true;
            this.CheckBoxMute.ForeColor = System.Drawing.Color.DimGray;
            this.CheckBoxMute.Location = new System.Drawing.Point(104, 171);
            this.CheckBoxMute.Name = "CheckBoxMute";
            this.CheckBoxMute.Size = new System.Drawing.Size(50, 17);
            this.CheckBoxMute.TabIndex = 15;
            this.CheckBoxMute.Text = "Mute";
            this.CheckBoxMute.UseVisualStyleBackColor = true;
            this.CheckBoxMute.CheckedChanged += new System.EventHandler(this.CheckBoxMute_CheckedChanged);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(230, 238);
            this.Controls.Add(this.CheckBoxMute);
            this.Controls.Add(this.LabelBufferSize);
            this.Controls.Add(this.ComboboxBufferSize);
            this.Controls.Add(this.LabelBufferCount);
            this.Controls.Add(this.ComboboxBufferCount);
            this.Controls.Add(this.LabelChannels);
            this.Controls.Add(this.ComboboxChannels);
            this.Controls.Add(this.LabelBitsPerSample);
            this.Controls.Add(this.ComboboxBitsPerSample);
            this.Controls.Add(this.LabelSamplesPreSecond);
            this.Controls.Add(this.ComboboxSamplesPerSecond);
            this.Controls.Add(this.LabelWaveOut);
            this.Controls.Add(this.LabelWaveIn);
            this.Controls.Add(this.ComboboxWaveIn);
            this.Controls.Add(this.ComboboxWaveOut);
            this.Controls.Add(this.ButtonStartRepeater);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Repeater Tester";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button ButtonStartRepeater;
		private System.Windows.Forms.ComboBox ComboboxWaveOut;
		private System.Windows.Forms.ComboBox ComboboxWaveIn;
		private System.Windows.Forms.Label LabelWaveIn;
        private System.Windows.Forms.Label LabelWaveOut;
        private System.Windows.Forms.Label LabelSamplesPreSecond;
        private System.Windows.Forms.ComboBox ComboboxSamplesPerSecond;
        private System.Windows.Forms.Label LabelBitsPerSample;
        private System.Windows.Forms.ComboBox ComboboxBitsPerSample;
        private System.Windows.Forms.Label LabelChannels;
        private System.Windows.Forms.ComboBox ComboboxChannels;
        private System.Windows.Forms.Label LabelBufferCount;
        private System.Windows.Forms.ComboBox ComboboxBufferCount;
        private System.Windows.Forms.Label LabelBufferSize;
        private System.Windows.Forms.ComboBox ComboboxBufferSize;
        private System.Windows.Forms.CheckBox CheckBoxMute;
	}
}

