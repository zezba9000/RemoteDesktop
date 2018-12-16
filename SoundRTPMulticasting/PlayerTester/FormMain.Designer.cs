namespace PlayerTester
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
            this.TextBoxFileName = new System.Windows.Forms.TextBox();
            this.LabelChannels = new System.Windows.Forms.Label();
            this.ComboboxChannels = new System.Windows.Forms.ComboBox();
            this.LabelBitsPerSample = new System.Windows.Forms.Label();
            this.ComboboxBitsPerSample = new System.Windows.Forms.ComboBox();
            this.LabelSamplesPreSecond = new System.Windows.Forms.Label();
            this.ComboboxSamplesPerSecond = new System.Windows.Forms.ComboBox();
            this.LabelWaveOut = new System.Windows.Forms.Label();
            this.ComboboxWaveOut = new System.Windows.Forms.ComboBox();
            this.ButtonOpenFileDialog = new System.Windows.Forms.Button();
            this.OpenFileDialogMain = new System.Windows.Forms.OpenFileDialog();
            this.ButtonPlay = new System.Windows.Forms.Button();
            this.ButtonPause = new System.Windows.Forms.Button();
            this.ButtonRecord = new System.Windows.Forms.Button();
            this.LabelWaveIn = new System.Windows.Forms.Label();
            this.ComboboxWaveIn = new System.Windows.Forms.ComboBox();
            this.CheckBoxAppend = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // TextBoxFileName
            // 
            this.TextBoxFileName.Location = new System.Drawing.Point(12, 232);
            this.TextBoxFileName.Name = "TextBoxFileName";
            this.TextBoxFileName.Size = new System.Drawing.Size(189, 20);
            this.TextBoxFileName.TabIndex = 1;
            this.TextBoxFileName.Text = "C:\\Record.wav";
            // 
            // LabelChannels
            // 
            this.LabelChannels.AutoSize = true;
            this.LabelChannels.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelChannels.Location = new System.Drawing.Point(10, 182);
            this.LabelChannels.Name = "LabelChannels";
            this.LabelChannels.Size = new System.Drawing.Size(51, 13);
            this.LabelChannels.TabIndex = 23;
            this.LabelChannels.Text = "Channels";
            // 
            // ComboboxChannels
            // 
            this.ComboboxChannels.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboboxChannels.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ComboboxChannels.FormattingEnabled = true;
            this.ComboboxChannels.Location = new System.Drawing.Point(13, 198);
            this.ComboboxChannels.Name = "ComboboxChannels";
            this.ComboboxChannels.Size = new System.Drawing.Size(48, 20);
            this.ComboboxChannels.TabIndex = 22;
            // 
            // LabelBitsPerSample
            // 
            this.LabelBitsPerSample.AutoSize = true;
            this.LabelBitsPerSample.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelBitsPerSample.Location = new System.Drawing.Point(10, 141);
            this.LabelBitsPerSample.Name = "LabelBitsPerSample";
            this.LabelBitsPerSample.Size = new System.Drawing.Size(75, 13);
            this.LabelBitsPerSample.TabIndex = 21;
            this.LabelBitsPerSample.Text = "BitsPerSample";
            // 
            // ComboboxBitsPerSample
            // 
            this.ComboboxBitsPerSample.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboboxBitsPerSample.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ComboboxBitsPerSample.FormattingEnabled = true;
            this.ComboboxBitsPerSample.Location = new System.Drawing.Point(12, 157);
            this.ComboboxBitsPerSample.Name = "ComboboxBitsPerSample";
            this.ComboboxBitsPerSample.Size = new System.Drawing.Size(73, 20);
            this.ComboboxBitsPerSample.TabIndex = 20;
            // 
            // LabelSamplesPreSecond
            // 
            this.LabelSamplesPreSecond.AutoSize = true;
            this.LabelSamplesPreSecond.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelSamplesPreSecond.Location = new System.Drawing.Point(9, 97);
            this.LabelSamplesPreSecond.Name = "LabelSamplesPreSecond";
            this.LabelSamplesPreSecond.Size = new System.Drawing.Size(99, 13);
            this.LabelSamplesPreSecond.TabIndex = 19;
            this.LabelSamplesPreSecond.Text = "SamplesPerSecond";
            // 
            // ComboboxSamplesPerSecond
            // 
            this.ComboboxSamplesPerSecond.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboboxSamplesPerSecond.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ComboboxSamplesPerSecond.FormattingEnabled = true;
            this.ComboboxSamplesPerSecond.Location = new System.Drawing.Point(13, 113);
            this.ComboboxSamplesPerSecond.Name = "ComboboxSamplesPerSecond";
            this.ComboboxSamplesPerSecond.Size = new System.Drawing.Size(95, 20);
            this.ComboboxSamplesPerSecond.TabIndex = 18;
            // 
            // LabelWaveOut
            // 
            this.LabelWaveOut.AutoSize = true;
            this.LabelWaveOut.Location = new System.Drawing.Point(10, 6);
            this.LabelWaveOut.Name = "LabelWaveOut";
            this.LabelWaveOut.Size = new System.Drawing.Size(56, 13);
            this.LabelWaveOut.TabIndex = 17;
            this.LabelWaveOut.Text = "Wave Out";
            // 
            // ComboboxWaveOut
            // 
            this.ComboboxWaveOut.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboboxWaveOut.FormattingEnabled = true;
            this.ComboboxWaveOut.Location = new System.Drawing.Point(12, 22);
            this.ComboboxWaveOut.Name = "ComboboxWaveOut";
            this.ComboboxWaveOut.Size = new System.Drawing.Size(233, 21);
            this.ComboboxWaveOut.TabIndex = 16;
            // 
            // ButtonOpenFileDialog
            // 
            this.ButtonOpenFileDialog.Location = new System.Drawing.Point(212, 232);
            this.ButtonOpenFileDialog.Name = "ButtonOpenFileDialog";
            this.ButtonOpenFileDialog.Size = new System.Drawing.Size(33, 20);
            this.ButtonOpenFileDialog.TabIndex = 25;
            this.ButtonOpenFileDialog.Text = "...";
            this.ButtonOpenFileDialog.UseVisualStyleBackColor = true;
            this.ButtonOpenFileDialog.Click += new System.EventHandler(this.ButtonOpenFileDialog_Click);
            // 
            // OpenFileDialogMain
            // 
            this.OpenFileDialogMain.FileName = "MyRecord.wav";
            this.OpenFileDialogMain.Filter = "Wave Dateien (*.wav)|*.wav|Alle Dateien (*.*)|*.*";
            // 
            // ButtonPlay
            // 
            this.ButtonPlay.Location = new System.Drawing.Point(12, 258);
            this.ButtonPlay.Name = "ButtonPlay";
            this.ButtonPlay.Size = new System.Drawing.Size(64, 23);
            this.ButtonPlay.TabIndex = 27;
            this.ButtonPlay.Text = "Play";
            this.ButtonPlay.UseVisualStyleBackColor = true;
            this.ButtonPlay.Click += new System.EventHandler(this.ButtonOpen_Click);
            // 
            // ButtonPause
            // 
            this.ButtonPause.Enabled = false;
            this.ButtonPause.Location = new System.Drawing.Point(170, 258);
            this.ButtonPause.Name = "ButtonPause";
            this.ButtonPause.Size = new System.Drawing.Size(75, 23);
            this.ButtonPause.TabIndex = 29;
            this.ButtonPause.Text = "Pause";
            this.ButtonPause.UseVisualStyleBackColor = true;
            this.ButtonPause.Click += new System.EventHandler(this.ButtonPause_Click);
            // 
            // ButtonRecord
            // 
            this.ButtonRecord.Location = new System.Drawing.Point(92, 258);
            this.ButtonRecord.Name = "ButtonRecord";
            this.ButtonRecord.Size = new System.Drawing.Size(64, 23);
            this.ButtonRecord.TabIndex = 30;
            this.ButtonRecord.Text = "Record";
            this.ButtonRecord.UseVisualStyleBackColor = true;
            this.ButtonRecord.Click += new System.EventHandler(this.ButtonRecord_Click);
            // 
            // LabelWaveIn
            // 
            this.LabelWaveIn.AutoSize = true;
            this.LabelWaveIn.Location = new System.Drawing.Point(11, 50);
            this.LabelWaveIn.Name = "LabelWaveIn";
            this.LabelWaveIn.Size = new System.Drawing.Size(48, 13);
            this.LabelWaveIn.TabIndex = 32;
            this.LabelWaveIn.Text = "Wave In";
            // 
            // ComboboxWaveIn
            // 
            this.ComboboxWaveIn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboboxWaveIn.FormattingEnabled = true;
            this.ComboboxWaveIn.Location = new System.Drawing.Point(13, 66);
            this.ComboboxWaveIn.Name = "ComboboxWaveIn";
            this.ComboboxWaveIn.Size = new System.Drawing.Size(232, 21);
            this.ComboboxWaveIn.TabIndex = 31;
            // 
            // CheckBoxAppend
            // 
            this.CheckBoxAppend.AutoSize = true;
            this.CheckBoxAppend.Location = new System.Drawing.Point(182, 199);
            this.CheckBoxAppend.Name = "CheckBoxAppend";
            this.CheckBoxAppend.Size = new System.Drawing.Size(63, 17);
            this.CheckBoxAppend.TabIndex = 34;
            this.CheckBoxAppend.Text = "Append";
            this.CheckBoxAppend.UseVisualStyleBackColor = true;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(249, 285);
            this.Controls.Add(this.CheckBoxAppend);
            this.Controls.Add(this.LabelWaveIn);
            this.Controls.Add(this.ComboboxWaveIn);
            this.Controls.Add(this.ButtonRecord);
            this.Controls.Add(this.ButtonPause);
            this.Controls.Add(this.ButtonPlay);
            this.Controls.Add(this.ButtonOpenFileDialog);
            this.Controls.Add(this.LabelChannels);
            this.Controls.Add(this.ComboboxChannels);
            this.Controls.Add(this.LabelBitsPerSample);
            this.Controls.Add(this.ComboboxBitsPerSample);
            this.Controls.Add(this.LabelSamplesPreSecond);
            this.Controls.Add(this.ComboboxSamplesPerSecond);
            this.Controls.Add(this.LabelWaveOut);
            this.Controls.Add(this.ComboboxWaveOut);
            this.Controls.Add(this.TextBoxFileName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Player Tester";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TextBoxFileName;
        private System.Windows.Forms.Label LabelChannels;
        private System.Windows.Forms.ComboBox ComboboxChannels;
        private System.Windows.Forms.Label LabelBitsPerSample;
        private System.Windows.Forms.ComboBox ComboboxBitsPerSample;
        private System.Windows.Forms.Label LabelSamplesPreSecond;
        private System.Windows.Forms.ComboBox ComboboxSamplesPerSecond;
        private System.Windows.Forms.Label LabelWaveOut;
        private System.Windows.Forms.ComboBox ComboboxWaveOut;
        private System.Windows.Forms.Button ButtonOpenFileDialog;
        private System.Windows.Forms.OpenFileDialog OpenFileDialogMain;
        private System.Windows.Forms.Button ButtonPlay;
        private System.Windows.Forms.Button ButtonPause;
        private System.Windows.Forms.Button ButtonRecord;
        private System.Windows.Forms.Label LabelWaveIn;
        private System.Windows.Forms.ComboBox ComboboxWaveIn;
        private System.Windows.Forms.CheckBox CheckBoxAppend;
    }
}

