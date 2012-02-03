namespace PublishOwnApps
{
	partial class Form1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.radioButtonLocal = new System.Windows.Forms.RadioButton();
			this.radioButtonOnline = new System.Windows.Forms.RadioButton();
			this.buttonPublishNow = new System.Windows.Forms.Button();
			this.checkBoxHasPlugins = new System.Windows.Forms.CheckBox();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.checkBoxUpdateRevision = new System.Windows.Forms.CheckBox();
			this.checkBoxAutoStartupWithWindows = new System.Windows.Forms.CheckBox();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// radioButtonLocal
			// 
			this.radioButtonLocal.AutoSize = true;
			this.radioButtonLocal.Checked = true;
			this.radioButtonLocal.ForeColor = System.Drawing.SystemColors.ControlText;
			this.radioButtonLocal.Location = new System.Drawing.Point(11, 63);
			this.radioButtonLocal.Name = "radioButtonLocal";
			this.radioButtonLocal.Size = new System.Drawing.Size(51, 17);
			this.radioButtonLocal.TabIndex = 0;
			this.radioButtonLocal.TabStop = true;
			this.radioButtonLocal.Text = "&Local";
			this.toolTip1.SetToolTip(this.radioButtonLocal, "The type of publish to be performed");
			this.radioButtonLocal.UseVisualStyleBackColor = true;
			// 
			// radioButtonOnline
			// 
			this.radioButtonOnline.AutoSize = true;
			this.radioButtonOnline.ForeColor = System.Drawing.SystemColors.ControlText;
			this.radioButtonOnline.Location = new System.Drawing.Point(86, 63);
			this.radioButtonOnline.Name = "radioButtonOnline";
			this.radioButtonOnline.Size = new System.Drawing.Size(55, 17);
			this.radioButtonOnline.TabIndex = 1;
			this.radioButtonOnline.Text = "&Online";
			this.toolTip1.SetToolTip(this.radioButtonOnline, "The type of publish to be performed");
			this.radioButtonOnline.UseVisualStyleBackColor = true;
			// 
			// buttonPublishNow
			// 
			this.buttonPublishNow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonPublishNow.Location = new System.Drawing.Point(149, 223);
			this.buttonPublishNow.Name = "buttonPublishNow";
			this.buttonPublishNow.Size = new System.Drawing.Size(75, 23);
			this.buttonPublishNow.TabIndex = 2;
			this.buttonPublishNow.Text = "&Publish now";
			this.buttonPublishNow.UseVisualStyleBackColor = true;
			// 
			// checkBoxHasPlugins
			// 
			this.checkBoxHasPlugins.AutoSize = true;
			this.checkBoxHasPlugins.Location = new System.Drawing.Point(12, 97);
			this.checkBoxHasPlugins.Name = "checkBoxHasPlugins";
			this.checkBoxHasPlugins.Size = new System.Drawing.Size(81, 17);
			this.checkBoxHasPlugins.TabIndex = 3;
			this.checkBoxHasPlugins.Text = "Has plugins";
			this.toolTip1.SetToolTip(this.checkBoxHasPlugins, "The application has projects inside its solution folder ending with \"Plugin\"");
			this.checkBoxHasPlugins.UseVisualStyleBackColor = true;
			// 
			// checkBoxUpdateRevision
			// 
			this.checkBoxUpdateRevision.AutoSize = true;
			this.checkBoxUpdateRevision.Location = new System.Drawing.Point(11, 129);
			this.checkBoxUpdateRevision.Name = "checkBoxUpdateRevision";
			this.checkBoxUpdateRevision.Size = new System.Drawing.Size(100, 17);
			this.checkBoxUpdateRevision.TabIndex = 4;
			this.checkBoxUpdateRevision.Text = "Update revision";
			this.toolTip1.SetToolTip(this.checkBoxUpdateRevision, "The revision (of the version) must be increased upon successful publish");
			this.checkBoxUpdateRevision.UseVisualStyleBackColor = true;
			// 
			// checkBoxAutoStartupWithWindows
			// 
			this.checkBoxAutoStartupWithWindows.AutoSize = true;
			this.checkBoxAutoStartupWithWindows.Location = new System.Drawing.Point(11, 165);
			this.checkBoxAutoStartupWithWindows.Name = "checkBoxAutoStartupWithWindows";
			this.checkBoxAutoStartupWithWindows.Size = new System.Drawing.Size(100, 17);
			this.checkBoxAutoStartupWithWindows.TabIndex = 5;
			this.checkBoxAutoStartupWithWindows.Text = "Load on startup";
			this.toolTip1.SetToolTip(this.checkBoxAutoStartupWithWindows, "The application must be placed in the registry to startup with windows");
			this.checkBoxAutoStartupWithWindows.UseVisualStyleBackColor = true;
			// 
			// comboBox1
			// 
			this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.comboBox1.ForeColor = System.Drawing.Color.Green;
			this.comboBox1.FormattingEnabled = true;
			this.comboBox1.Items.AddRange(new object[] {
            "MonitorSystem",
            "QuickAccess"});
			this.comboBox1.Location = new System.Drawing.Point(13, 13);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(211, 28);
			this.comboBox1.TabIndex = 6;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(236, 258);
			this.Controls.Add(this.comboBox1);
			this.Controls.Add(this.checkBoxAutoStartupWithWindows);
			this.Controls.Add(this.checkBoxUpdateRevision);
			this.Controls.Add(this.radioButtonLocal);
			this.Controls.Add(this.radioButtonOnline);
			this.Controls.Add(this.checkBoxHasPlugins);
			this.Controls.Add(this.buttonPublishNow);
			this.MinimizeBox = false;
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Form1";
			this.TopMost = true;
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.RadioButton radioButtonLocal;
		private System.Windows.Forms.RadioButton radioButtonOnline;
		private System.Windows.Forms.Button buttonPublishNow;
		private System.Windows.Forms.CheckBox checkBoxHasPlugins;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.CheckBox checkBoxUpdateRevision;
		private System.Windows.Forms.CheckBox checkBoxAutoStartupWithWindows;
		private System.Windows.Forms.ComboBox comboBox1;
	}
}

