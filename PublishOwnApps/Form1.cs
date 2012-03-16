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

			WindowMessagesInterop.InitializeClientMessages();

			textFeedbackEvent += new TextFeedbackEventHandler(OnTextFeedbackEvent);
			progressChangedEvent += new ProgressChangedEventHandler(OnProgressChangedEvent);

			comboBoxProjectName.Items.Clear();
			foreach (string item in GlobalSettings.PublishSettings.Instance.ListedApplicationNames.Split('|').OrderBy(s => s))
				comboBoxProjectName.Items.Add(
					new ApplicationToPublish(
						item,
						HasPlugins: item.Equals("QuickAccess", StringComparison.InvariantCultureIgnoreCase),
						UpdateRevisionNumber: false,
						AutostartWithWindows: item.Equals(StringComparison.InvariantCultureIgnoreCase,
							"ApplicationManager", "MonitorSystem", "QuickAccess", "StartupTodoManager", "TestingMonitorSubversion")
						));
		}

		protected override void WndProc(ref Message m)
		{
			WindowMessagesInterop.MessageTypes mt;
			WindowMessagesInterop.ClientHandleMessage(m.Msg, m.WParam, m.LParam, out mt);
			if (mt == WindowMessagesInterop.MessageTypes.Show)
				this.Show();
			else if (mt == WindowMessagesInterop.MessageTypes.Hide)
				this.Hide();
			else if (mt == WindowMessagesInterop.MessageTypes.Close)
				this.Close();
			else
				base.WndProc(ref m);
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			StylingInterop.SetTreeviewVistaStyle(treeViewPublishList);
			base.OnHandleCreated(e);
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
			if (comboBoxProjectName.Text.Trim().Length == 0)
				UserMessages.ShowWarningMessage("Please select a project name first.");
			else
			{
				if (comboBoxProjectName.SelectedIndex == -1)
					PublishApplication(new ApplicationToPublish(comboBoxProjectName.Text, checkBoxHasPlugins.Checked, checkBoxUpdateRevision.Checked, checkBoxAutoStartupWithWindows.Checked));
				else
					PublishApplication(comboBoxProjectName.SelectedItem as ApplicationToPublish);
			}
		}

		private void buttonPublishList_Click(object sender, EventArgs e)
		{
			if (comboBoxProjectName.SelectedIndex != -1)
				UserMessages.ShowInfoMessage("There is still an item in the combobox, click to add it to the list or clear it before continuing.");
			else
			{
				foreach (TreeNode node in treeViewPublishList.Nodes)
					PublishApplication(node.Tag as ApplicationToPublish);
			}
		}

		private void PublishApplication(ApplicationToPublish apptoPublish)
		{
			InitialTopmost = this.TopMost;
			this.TopMost = false;
			using (ThreadingInterop.WaitIndicator wi = new ThreadingInterop.WaitIndicator())
			{
				currentProgressBar = wi;
				UpdateProgressBarPosition();

				if (radioButtonLocal.Checked)
				{
					string tmpNoUseVersionStr;
					VisualStudioInterop.PerformPublish(
						textfeedbackSenderObject: this,
						projName: apptoPublish.ApplicationName,//comboBoxProjectName.Text,
						versionString: out tmpNoUseVersionStr,
						HasPlugins: apptoPublish.HasPlugins,//checkBoxHasPlugins.Checked,
						AutomaticallyUpdateRevision: apptoPublish.UpdateRevisionNumber,//checkBoxUpdateRevision.Checked,
						WriteIntoRegistryForWindowsAutostartup: apptoPublish.AutostartWithWindows,//checkBoxAutoStartupWithWindows.Checked,
						textFeedbackEvent: textFeedbackEvent);
				}
				else if (radioButtonOnline.Checked)
				{
					VisualStudioInterop.PerformPublishOnline(
							 textfeedbackSenderObject: this,
							 projName: apptoPublish.ApplicationName,//comboBoxProjectName.Text,
							 HasPlugins: apptoPublish.HasPlugins,//checkBoxHasPlugins.Checked,
							 AutomaticallyUpdateRevision: apptoPublish.UpdateRevisionNumber,//checkBoxUpdateRevision.Checked,
							 WriteIntoRegistryForWindowsAutostartup: apptoPublish.AutostartWithWindows,//checkBoxAutoStartupWithWindows.Checked,
							 textFeedbackEvent: textFeedbackEvent,
							 progressChanged: progressChangedEvent);
				}
				else
					UserMessages.ShowWarningMessage("Please choose either local or online.");

				currentProgressBar = null;
			}
			this.TopMost = InitialTopmost;
		}

		private void comboBoxProjectName_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboBoxProjectName.SelectedIndex == -1)
				return;

			checkBoxHasPlugins.Checked = (comboBoxProjectName.SelectedItem as ApplicationToPublish).HasPlugins;
			checkBoxUpdateRevision.Checked = (comboBoxProjectName.SelectedItem as ApplicationToPublish).UpdateRevisionNumber;
			checkBoxAutoStartupWithWindows.Checked = (comboBoxProjectName.SelectedItem as ApplicationToPublish).AutostartWithWindows;
		}

		private void comboBoxProjectName_TextChanged(object sender, EventArgs e)
		{
			//checkBoxHasPlugins.Enabled = comboBoxProjectName.SelectedIndex != -1;
			//checkBoxUpdateRevision.Enabled = comboBoxProjectName.SelectedIndex != -1;
			//checkBoxAutoStartupWithWindows.Enabled = comboBoxProjectName.SelectedIndex != -1;
			checkBoxHasPlugins.Checked = false;
			checkBoxUpdateRevision.Checked = false;
			checkBoxAutoStartupWithWindows.Checked = false;
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

		private void linkLabelAddToPublishList_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			string appname = comboBoxProjectName.Text;

			if (appname.Trim().Length == 0)//comboBoxProjectName.SelectedIndex == -1)
				UserMessages.ShowWarningMessage("Please select an item first or type its name.");
			else if (treeViewPublishList.Nodes.ContainsKey(appname))
				UserMessages.ShowWarningMessage("Cannot have duplicates in the publish list.");
			else
			{
				TreeNode newNode = treeViewPublishList.Nodes.Add(appname, appname);
				newNode.Tag =
					comboBoxProjectName.SelectedIndex == -1
					? new ApplicationToPublish(appname, checkBoxHasPlugins.Checked, checkBoxUpdateRevision.Checked, checkBoxAutoStartupWithWindows.Checked)
					: comboBoxProjectName.SelectedItem as ApplicationToPublish;
				comboBoxProjectName.SelectedIndex = -1;
			}
		}

		private void treeViewPublishList_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (treeViewPublishList.SelectedNode == null)
				return;
			ApplicationToPublish app = treeViewPublishList.SelectedNode.Tag as ApplicationToPublish;
			if (app == null)
				return;
			checkBoxHasPlugins.Checked = app.HasPlugins;
			checkBoxUpdateRevision.Checked = app.UpdateRevisionNumber;
			checkBoxAutoStartupWithWindows.Checked = app.AutostartWithWindows;
		}
	}

	public class ApplicationToPublish
	{
		public string ApplicationName;
		public bool HasPlugins;
		public bool UpdateRevisionNumber;
		public bool AutostartWithWindows;
		public ApplicationToPublish(string ApplicationName, bool HasPlugins, bool UpdateRevisionNumber, bool AutostartWithWindows)
		{
			this.ApplicationName = ApplicationName;
			this.HasPlugins = HasPlugins;
			this.UpdateRevisionNumber = UpdateRevisionNumber;
			this.AutostartWithWindows = AutostartWithWindows;
		}
		public override string ToString()
		{
			return ApplicationName;
		}
	}
}
