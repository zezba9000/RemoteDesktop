namespace MulticastStreamerXama
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
			this.GroupBoxSound = new System.Windows.Forms.GroupBox();
			this.CheckBoxStreamFile = new System.Windows.Forms.CheckBox();
			this.LabelSamplesPerSecond = new System.Windows.Forms.Label();
			this.LabelBitsPerSample = new System.Windows.Forms.Label();
			this.LabelChannels = new System.Windows.Forms.Label();
			this.ButtonStart = new System.Windows.Forms.Button();
			this.ComboboxSamplesPerSecond = new System.Windows.Forms.ComboBox();
			this.ComboboxBitsPerSample = new System.Windows.Forms.ComboBox();
			this.ComboboxChannels = new System.Windows.Forms.ComboBox();
			this.ButtonStreamFile = new System.Windows.Forms.Button();
			this.GroupBoxDestination = new System.Windows.Forms.GroupBox();
			this.CheckBoxJitterBuffer = new System.Windows.Forms.CheckBox();
			this.ComboboxBufferCount = new System.Windows.Forms.ComboBox();
			this.LabelMCAddress = new System.Windows.Forms.Label();
			this.LabelBufferCount = new System.Windows.Forms.Label();
			this.TextBoxMulticastAddress = new System.Windows.Forms.TextBox();
			this.TextBoxMulticastPort = new System.Windows.Forms.TextBox();
			this.LabelMCPort = new System.Windows.Forms.Label();
			this.LabelSoundDeviceName = new System.Windows.Forms.Label();
			this.ComboboxSoundDeviceName = new System.Windows.Forms.ComboBox();
			this.GroupBoxFile = new System.Windows.Forms.GroupBox();
			this.CheckBoxLoop = new System.Windows.Forms.CheckBox();
			this.ProgressBarFile = new System.Windows.Forms.ProgressBar();
			this.ListBoxFile = new System.Windows.Forms.ListBox();
			this.ButtonOpenFileDialog = new System.Windows.Forms.Button();
			this.TextBoxFileName = new System.Windows.Forms.TextBox();
			this.OpenFileDialogMain = new System.Windows.Forms.OpenFileDialog();
			this.GroupBoxSound.SuspendLayout();
			this.GroupBoxDestination.SuspendLayout();
			this.GroupBoxFile.SuspendLayout();
			this.SuspendLayout();
			//
			// GroupBoxSound
			//
			this.GroupBoxSound.Controls.Add(this.CheckBoxStreamFile);
			this.GroupBoxSound.Controls.Add(this.LabelSamplesPerSecond);
			this.GroupBoxSound.Controls.Add(this.LabelBitsPerSample);
			this.GroupBoxSound.Controls.Add(this.LabelChannels);
			this.GroupBoxSound.Controls.Add(this.ButtonStart);
			this.GroupBoxSound.Controls.Add(this.ComboboxSamplesPerSecond);
			this.GroupBoxSound.Controls.Add(this.ComboboxBitsPerSample);
			this.GroupBoxSound.Controls.Add(this.ComboboxChannels);
			this.GroupBoxSound.Location = new System.Drawing.Point(4, 99);
			this.GroupBoxSound.Name = "GroupBoxSound";
			this.GroupBoxSound.Size = new System.Drawing.Size(432, 111);
			this.GroupBoxSound.TabIndex = 25;
			this.GroupBoxSound.TabStop = false;
			//
			// CheckBoxStreamFile
			//
			this.CheckBoxStreamFile.AutoSize = true;
			this.CheckBoxStreamFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CheckBoxStreamFile.Location = new System.Drawing.Point(343, 88);
			this.CheckBoxStreamFile.Name = "CheckBoxStreamFile";
			this.CheckBoxStreamFile.Size = new System.Drawing.Size(79, 17);
			this.CheckBoxStreamFile.TabIndex = 16;
			this.CheckBoxStreamFile.Text = "Extended";
			this.CheckBoxStreamFile.UseVisualStyleBackColor = true;
			this.CheckBoxStreamFile.CheckedChanged += new System.EventHandler(this.CheckBoxStreamFile_CheckedChanged);
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
			// ButtonStart
			//
			this.ButtonStart.BackColor = System.Drawing.Color.Gainsboro;
			this.ButtonStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 22F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ButtonStart.Location = new System.Drawing.Point(248, 22);
			this.ButtonStart.Margin = new System.Windows.Forms.Padding(2);
			this.ButtonStart.Name = "ButtonStart";
			this.ButtonStart.Size = new System.Drawing.Size(174, 50);
			this.ButtonStart.TabIndex = 0;
			this.ButtonStart.Text = "Start";
			this.ButtonStart.UseVisualStyleBackColor = false;
			this.ButtonStart.Click += new System.EventHandler(this.ButtonStart_Click);
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
			// ButtonStreamFile
			//
			this.ButtonStreamFile.BackColor = System.Drawing.Color.Gainsboro;
			this.ButtonStreamFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ButtonStreamFile.ForeColor = System.Drawing.Color.Black;
			this.ButtonStreamFile.Location = new System.Drawing.Point(310, 46);
			this.ButtonStreamFile.Margin = new System.Windows.Forms.Padding(2);
			this.ButtonStreamFile.Name = "ButtonStreamFile";
			this.ButtonStreamFile.Size = new System.Drawing.Size(112, 70);
			this.ButtonStreamFile.TabIndex = 16;
			this.ButtonStreamFile.Text = "Stream File";
			this.ButtonStreamFile.UseVisualStyleBackColor = false;
			this.ButtonStreamFile.Click += new System.EventHandler(this.ButtonStreamFile_Click);
			//
			// GroupBoxDestination
			//
			this.GroupBoxDestination.Controls.Add(this.CheckBoxJitterBuffer);
			this.GroupBoxDestination.Controls.Add(this.ComboboxBufferCount);
			this.GroupBoxDestination.Controls.Add(this.LabelMCAddress);
			this.GroupBoxDestination.Controls.Add(this.LabelBufferCount);
			this.GroupBoxDestination.Controls.Add(this.TextBoxMulticastAddress);
			this.GroupBoxDestination.Controls.Add(this.TextBoxMulticastPort);
			this.GroupBoxDestination.Controls.Add(this.LabelMCPort);
			this.GroupBoxDestination.Controls.Add(this.LabelSoundDeviceName);
			this.GroupBoxDestination.Controls.Add(this.ComboboxSoundDeviceName);
			this.GroupBoxDestination.Location = new System.Drawing.Point(4, 9);
			this.GroupBoxDestination.Name = "GroupBoxDestination";
			this.GroupBoxDestination.Size = new System.Drawing.Size(433, 93);
			this.GroupBoxDestination.TabIndex = 24;
			this.GroupBoxDestination.TabStop = false;
			this.GroupBoxDestination.Text = "Destination";
			//
			// CheckBoxJitterBuffer
			//
			this.CheckBoxJitterBuffer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CheckBoxJitterBuffer.AutoSize = true;
			this.CheckBoxJitterBuffer.Checked = true;
			this.CheckBoxJitterBuffer.CheckState = System.Windows.Forms.CheckState.Checked;
			this.CheckBoxJitterBuffer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CheckBoxJitterBuffer.Location = new System.Drawing.Point(342, 61);
			this.CheckBoxJitterBuffer.Name = "CheckBoxJitterBuffer";
			this.CheckBoxJitterBuffer.Size = new System.Drawing.Size(85, 17);
			this.CheckBoxJitterBuffer.TabIndex = 20;
			this.CheckBoxJitterBuffer.Text = "Time Sync";
			this.CheckBoxJitterBuffer.UseVisualStyleBackColor = true;
			this.CheckBoxJitterBuffer.CheckedChanged += new System.EventHandler(this.CheckBoxJitterBuffer_CheckedChanged);
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
			this.ComboboxBufferCount.Location = new System.Drawing.Point(363, 22);
			this.ComboboxBufferCount.Margin = new System.Windows.Forms.Padding(2);
			this.ComboboxBufferCount.Name = "ComboboxBufferCount";
			this.ComboboxBufferCount.Size = new System.Drawing.Size(60, 21);
			this.ComboboxBufferCount.TabIndex = 19;
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
			// LabelBufferCount
			//
			this.LabelBufferCount.AutoSize = true;
			this.LabelBufferCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LabelBufferCount.Location = new System.Drawing.Point(285, 25);
			this.LabelBufferCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.LabelBufferCount.Name = "LabelBufferCount";
			this.LabelBufferCount.Size = new System.Drawing.Size(66, 13);
			this.LabelBufferCount.TabIndex = 18;
			this.LabelBufferCount.Text = "Buffer Count";
			//
			// TextBoxMulticastAddress
			//
			this.TextBoxMulticastAddress.BackColor = System.Drawing.Color.White;
			this.TextBoxMulticastAddress.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TextBoxMulticastAddress.ForeColor = System.Drawing.Color.DimGray;
			this.TextBoxMulticastAddress.Location = new System.Drawing.Point(58, 22);
			this.TextBoxMulticastAddress.Margin = new System.Windows.Forms.Padding(2);
			this.TextBoxMulticastAddress.Name = "TextBoxMulticastAddress";
			this.TextBoxMulticastAddress.Size = new System.Drawing.Size(115, 20);
			this.TextBoxMulticastAddress.TabIndex = 1;
			this.TextBoxMulticastAddress.Text = "239.192.17.20";
			this.TextBoxMulticastAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			//
			// TextBoxMulticastPort
			//
			this.TextBoxMulticastPort.BackColor = System.Drawing.Color.White;
			this.TextBoxMulticastPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TextBoxMulticastPort.ForeColor = System.Drawing.Color.DimGray;
			this.TextBoxMulticastPort.Location = new System.Drawing.Point(207, 22);
			this.TextBoxMulticastPort.Margin = new System.Windows.Forms.Padding(2);
			this.TextBoxMulticastPort.Name = "TextBoxMulticastPort";
			this.TextBoxMulticastPort.Size = new System.Drawing.Size(59, 20);
			this.TextBoxMulticastPort.TabIndex = 2;
			this.TextBoxMulticastPort.Text = "16400";
			this.TextBoxMulticastPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
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
			this.LabelSoundDeviceName.Location = new System.Drawing.Point(5, 63);
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
			this.ComboboxSoundDeviceName.Location = new System.Drawing.Point(84, 59);
			this.ComboboxSoundDeviceName.Margin = new System.Windows.Forms.Padding(2);
			this.ComboboxSoundDeviceName.Name = "ComboboxSoundDeviceName";
			this.ComboboxSoundDeviceName.Size = new System.Drawing.Size(245, 21);
			this.ComboboxSoundDeviceName.TabIndex = 12;
			//
			// GroupBoxFile
			//
			this.GroupBoxFile.Controls.Add(this.CheckBoxLoop);
			this.GroupBoxFile.Controls.Add(this.ProgressBarFile);
			this.GroupBoxFile.Controls.Add(this.ListBoxFile);
			this.GroupBoxFile.Controls.Add(this.ButtonOpenFileDialog);
			this.GroupBoxFile.Controls.Add(this.TextBoxFileName);
			this.GroupBoxFile.Controls.Add(this.ButtonStreamFile);
			this.GroupBoxFile.Location = new System.Drawing.Point(4, 219);
			this.GroupBoxFile.Name = "GroupBoxFile";
			this.GroupBoxFile.Size = new System.Drawing.Size(432, 146);
			this.GroupBoxFile.TabIndex = 26;
			this.GroupBoxFile.TabStop = false;
			this.GroupBoxFile.Text = "WAV File";
			//
			// CheckBoxLoop
			//
			this.CheckBoxLoop.AutoSize = true;
			this.CheckBoxLoop.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CheckBoxLoop.Location = new System.Drawing.Point(366, 123);
			this.CheckBoxLoop.Name = "CheckBoxLoop";
			this.CheckBoxLoop.Size = new System.Drawing.Size(54, 17);
			this.CheckBoxLoop.TabIndex = 30;
			this.CheckBoxLoop.Text = "Loop";
			this.CheckBoxLoop.UseVisualStyleBackColor = true;
			this.CheckBoxLoop.CheckedChanged += new System.EventHandler(this.CheckBoxLoop_CheckedChanged);
			//
			// ProgressBarFile
			//
			this.ProgressBarFile.Location = new System.Drawing.Point(8, 125);
			this.ProgressBarFile.Name = "ProgressBarFile";
			this.ProgressBarFile.Size = new System.Drawing.Size(342, 13);
			this.ProgressBarFile.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.ProgressBarFile.TabIndex = 29;
			//
			// ListBoxFile
			//
			this.ListBoxFile.BackColor = System.Drawing.Color.DarkSeaGreen;
			this.ListBoxFile.FormattingEnabled = true;
			this.ListBoxFile.Location = new System.Drawing.Point(8, 47);
			this.ListBoxFile.Name = "ListBoxFile";
			this.ListBoxFile.Size = new System.Drawing.Size(285, 69);
			this.ListBoxFile.TabIndex = 28;
			//
			// ButtonOpenFileDialog
			//
			this.ButtonOpenFileDialog.Location = new System.Drawing.Point(389, 19);
			this.ButtonOpenFileDialog.Name = "ButtonOpenFileDialog";
			this.ButtonOpenFileDialog.Size = new System.Drawing.Size(33, 20);
			this.ButtonOpenFileDialog.TabIndex = 27;
			this.ButtonOpenFileDialog.Text = "...";
			this.ButtonOpenFileDialog.UseVisualStyleBackColor = true;
			this.ButtonOpenFileDialog.Click += new System.EventHandler(this.ButtonOpenFileDialog_Click);
			//
			// TextBoxFileName
			//
			this.TextBoxFileName.Location = new System.Drawing.Point(8, 19);
			this.TextBoxFileName.Name = "TextBoxFileName";
			this.TextBoxFileName.Size = new System.Drawing.Size(375, 20);
			this.TextBoxFileName.TabIndex = 26;
			this.TextBoxFileName.Text = "C:\\Record.wav";
			this.TextBoxFileName.TextChanged += new System.EventHandler(this.TextBoxFileName_TextChanged);
			//
			// OpenFileDialogMain
			//
			this.OpenFileDialogMain.FileName = "MyRecord.wav";
			this.OpenFileDialogMain.Filter = "Wave Dateien (*.wav)|*.wav|Alle Dateien (*.*)|*.*";
			//
			// FormMain
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(436, 213);
			this.Controls.Add(this.GroupBoxSound);
			this.Controls.Add(this.GroupBoxFile);
			this.Controls.Add(this.GroupBoxDestination);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "FormMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Multicast Streamer";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
			this.GroupBoxSound.ResumeLayout(false);
			this.GroupBoxSound.PerformLayout();
			this.GroupBoxDestination.ResumeLayout(false);
			this.GroupBoxDestination.PerformLayout();
			this.GroupBoxFile.ResumeLayout(false);
			this.GroupBoxFile.PerformLayout();
			this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox GroupBoxSound;
    private System.Windows.Forms.Label LabelSamplesPerSecond;
    private System.Windows.Forms.Label LabelBitsPerSample;
    private System.Windows.Forms.Label LabelChannels;
    private System.Windows.Forms.Button ButtonStart;
    private System.Windows.Forms.ComboBox ComboboxSamplesPerSecond;
    private System.Windows.Forms.ComboBox ComboboxBitsPerSample;
    private System.Windows.Forms.ComboBox ComboboxChannels;
    private System.Windows.Forms.GroupBox GroupBoxDestination;
    private System.Windows.Forms.Label LabelMCAddress;
    private System.Windows.Forms.TextBox TextBoxMulticastAddress;
    private System.Windows.Forms.TextBox TextBoxMulticastPort;
    private System.Windows.Forms.Label LabelMCPort;
    private System.Windows.Forms.Label LabelSoundDeviceName;
    private System.Windows.Forms.ComboBox ComboboxSoundDeviceName;
    private System.Windows.Forms.ComboBox ComboboxBufferCount;
    private System.Windows.Forms.Label LabelBufferCount;
    private System.Windows.Forms.Button ButtonStreamFile;
    private System.Windows.Forms.GroupBox GroupBoxFile;
    private System.Windows.Forms.Button ButtonOpenFileDialog;
    private System.Windows.Forms.TextBox TextBoxFileName;
    private System.Windows.Forms.OpenFileDialog OpenFileDialogMain;
    private System.Windows.Forms.ListBox ListBoxFile;
    private System.Windows.Forms.CheckBox CheckBoxStreamFile;
    private System.Windows.Forms.ProgressBar ProgressBarFile;
    private System.Windows.Forms.CheckBox CheckBoxLoop;
		private System.Windows.Forms.CheckBox CheckBoxJitterBuffer;
  }
}
