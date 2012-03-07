using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharedClasses;

namespace PublishOwnApps
{
	public partial class Form1 : Form
	{
		TextFeedbackEventHandler textFeedbackEvent;
		ProgressChangedEventHandler progressChangedEvent;

		public Form1()
		{
			InitializeComponent();

			textFeedbackEvent += new TextFeedbackEventHandler(OnTextFeedbackEvent);
			progressChangedEvent += new ProgressChangedEventHandler(OnProgressChangedEvent);

			comboBoxProjectName.Items.Clear();
			foreach (string item in GlobalSettings.PublishSettings.Instance.ListedApplicationNames.Split('|'))
				comboBoxProjectName.Items.Add(item);
		}

		private void OnTextFeedbackEvent(object sender, TextFeedbackEventArgs e)
		{
			Action action = new Action(
				delegate
				{
					textBoxMessages.Text += (textBoxMessages.Text.Trim().Length > 0 ? Environment.NewLine : "") + e.FeedbackText;
					textBoxMessages.SelectionStart = textBoxMessages.Text.Length;
					textBoxMessages.SelectionLength = 0;
					textBoxMessages.ScrollToCaret();
				});
			if (this.InvokeRequired)
				this.Invoke(action);
			else
				action();
		}

		private void OnProgressChangedEvent(object sender, ProgressChangedEventArgs e)
		{
			Action action = new Action(
				delegate
				{
					progressBar.Maximum = e.MaximumValue;
					progressBar.Value = e.CurrentValue;
					if (progressBar.Maximum == progressBar.Value)
						progressBar.Value = 0;
				});
			if (this.InvokeRequired)
				this.Invoke(action);
			else
				action();
		}

		private ThreadingInterop.WaitIndicator currentProgressBar = null;
		private bool InitialTopmost = false;
		private void buttonPublishNow_Click(object sender, EventArgs e)
		{
			InitialTopmost = this.TopMost;
			this.TopMost = false;
			using (ThreadingInterop.WaitIndicator wi = new ThreadingInterop.WaitIndicator())
			{
				currentProgressBar = wi;
				UpdateProgressBarPosition();

				if (comboBoxProjectName.Text.Trim().Length == 0)
					UserMessages.ShowWarningMessage("Please select a project name first");
				else
				{
					if (radioButtonLocal.Checked)
					{
						string tmpNoUseVersionStr;
						VisualStudioInterop.PerformPublish(
							textfeedbackSenderObject: this,
							projName: comboBoxProjectName.Text,
							versionString: out tmpNoUseVersionStr,
							HasPlugins: checkBoxHasPlugins.Checked,
							AutomaticallyUpdateRevision: checkBoxUpdateRevision.Checked,
							WriteIntoRegistryForWindowsAutostartup: checkBoxAutoStartupWithWindows.Checked,
							textFeedbackEvent: textFeedbackEvent);
					}
					else if (radioButtonOnline.Checked)
					{
						VisualStudioInterop.PerformPublishOnline(
								 textfeedbackSenderObject: this,
								 projName: comboBoxProjectName.Text,
								 HasPlugins: checkBoxHasPlugins.Checked,
								 AutomaticallyUpdateRevision: checkBoxUpdateRevision.Checked,
								 WriteIntoRegistryForWindowsAutostartup: checkBoxAutoStartupWithWindows.Checked,
								 textFeedbackEvent: textFeedbackEvent,
								 progressChanged: progressChangedEvent);
					}
					else
						UserMessages.ShowWarningMessage("Please choose either local or online");
				}
				currentProgressBar = null;
			}
			this.TopMost = InitialTopmost;
		}

		private void comboBoxProjectName_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboBoxProjectName.SelectedIndex == -1)
				return;

			if (comboBoxProjectName.SelectedItem.ToString().ToLower() == "MonitorSystem".ToLower())
			{
				checkBoxHasPlugins.Checked = false;
				//checkBoxUpdateRevision.Checked = true;
				checkBoxAutoStartupWithWindows.Checked = true;
			}
			else if (comboBoxProjectName.SelectedItem.ToString().ToLower() == "QuickAccess".ToLower())
			{
				checkBoxHasPlugins.Checked = true;
				//checkBoxUpdateRevision.Checked = true;
				checkBoxAutoStartupWithWindows.Checked = true;
			}
			else if (comboBoxProjectName.SelectedItem.ToString().ToLower() == "PublishOwnApps".ToLower())
			{
				checkBoxHasPlugins.Checked = false;
				//checkBoxUpdateRevision.Checked = true;
				checkBoxAutoStartupWithWindows.Checked = false;
			}
		}

		private void radioButtonOnline_CheckedChanged(object sender, EventArgs e)
		{
			progressBar.Visible = radioButtonOnline.Checked;
		}

		private void Form1_LocationChanged(object sender, EventArgs e)
		{
			UpdateProgressBarPosition();
		}

		private void Form1_SizeChanged(object sender, EventArgs e)
		{
			UpdateProgressBarPosition();
		}

		private void UpdateProgressBarPosition()
		{
			if (currentProgressBar != null)
			{
				try
				{
					currentProgressBar.progressForm.Location = new Point(
						this.Left + (this.Width / 2) - (currentProgressBar.progressForm.Width / 2),
						this.Top + (this.Height / 2) - (currentProgressBar.progressForm.Height / 2));
				}
				catch { }
			}
		}

		private void Form1_Shown(object sender, EventArgs e)
		{
			this.Cursor = Cursors.Default;
			comboBoxProjectName.DroppedDown = true;
			this.Cursor = Cursors.Default;
			Application.DoEvents();
		}
	}
}
