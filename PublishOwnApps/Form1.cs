﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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

			checkBoxTopmost.Checked = this.TopMost;

			WindowMessagesInterop.InitializeClientMessages();

			textFeedbackEvent += new TextFeedbackEventHandler(OnTextFeedbackEvent);
			progressChangedEvent += new ProgressChangedEventHandler(OnProgressChangedEvent);

			comboBoxProjectName.Items.Clear();
			//foreach (string item in GlobalSettings.PublishSettings.Instance.ListedApplicationNames.Split('|').OrderBy(s => s))
			foreach (string item in OnlineSettings.PublishSettings.Instance.ListedApplicationNames.OrderBy(s => s))
				comboBoxProjectName.Items.Add(
					new ApplicationToPublish(
						item,
						HasPlugins: item.Equals("QuickAccess", StringComparison.InvariantCultureIgnoreCase),
						UpdateRevisionNumber: false,
						AutostartWithWindows: item.Equals(StringComparison.InvariantCultureIgnoreCase,
							"ApplicationManager", "MonitorSystem", "QuickAccess", "StartupTodoManager", "TestingMonitorSubversion")
						));

			//CustomBalloonTipwpf.ShowCustomBalloonTip(
			//	"Test title",
			//	"Message 123",
			//	2000,
			//	CustomBalloonTipwpf.IconTypes.Information);
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
					if (richTextBoxExMessages.Text.Trim().Length > 0)
						richTextBoxExMessages.AppendText(Environment.NewLine);

					if (!e.HyperlinkRange.HasValue)
						richTextBoxExMessages.AppendText(e.FeedbackText);
					else
					{
						string msgBefore = e.FeedbackText.Substring(0, e.HyperlinkRange.Value.Start);
						string hyperlink = e.FeedbackText.Substring(e.HyperlinkRange.Value.Start, e.HyperlinkRange.Value.Length);
						string msgAfter = e.HyperlinkRange.Value.End < e.FeedbackText.Length ? e.FeedbackText.Substring(e.HyperlinkRange.Value.End + 1) : "";

						richTextBoxExMessages.AppendText(msgBefore);

						richTextBoxExMessages.SelectionStart = richTextBoxExMessages.Text.Length;
						richTextBoxExMessages.SelectionLength = 0;
						string prefix = e.HyperlinkRange.Value.LinkType.ToString().ToLower() + ":";
						string displayPath = "";
						switch (e.HyperlinkRange.Value.LinkType)
						{
							case Range.LinkTypes.ExplorerSelect:
								displayPath = Path.GetFileName(hyperlink);
								break;
							case Range.LinkTypes.OpenUrl:
								displayPath = hyperlink.Replace('\\', '/');
								break;
							default:
								break;
						}

						richTextBoxExMessages.InsertLink(
							displayPath,
							EncodeAndDecodeInterop.EncodeStringHex(prefix + hyperlink));

						richTextBoxExMessages.AppendText(msgAfter);
					}

					richTextBoxExMessages.SelectionStart = richTextBoxExMessages.Text.Length;
					richTextBoxExMessages.SelectionLength = 0;
					richTextBoxExMessages.ScrollToCaret();
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

		private bool BusyPublishing = false;
		private void PublishApplication(ApplicationToPublish apptoPublish)
		{
			InitialTopmost = this.TopMost;
			this.TopMost = false;
			using (ThreadingInterop.WaitIndicator wi = new ThreadingInterop.WaitIndicator(this))
			{
				if (BusyPublishing)
				{
					UserMessages.ShowWarningMessage("Publishing is busy, please be patient...");
					return;
				}

				BusyPublishing = true;

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
						InstallLocallyAfterSuccessfullNSIS: checkBoxInstallLocally.Checked,
						AutomaticallyUpdateRevision: true,//apptoPublish.UpdateRevisionNumber,//checkBoxUpdateRevision.Checked,
						WriteIntoRegistryForWindowsAutostartup: apptoPublish.AutostartWithWindows,//checkBoxAutoStartupWithWindows.Checked,
						textFeedbackEvent: textFeedbackEvent,
						SelectInFolderAfterSuccessfullNSIS: checkBoxOpenFolder.Checked);
				}
				else if (radioButtonOnline.Checked)
				{
					VisualStudioInterop.PerformPublishOnline(
							 textfeedbackSenderObject: this,
							 projName: apptoPublish.ApplicationName,//comboBoxProjectName.Text,
							 HasPlugins: apptoPublish.HasPlugins,//checkBoxHasPlugins.Checked,
							 AutomaticallyUpdateRevision: true,//apptoPublish.UpdateRevisionNumber,//checkBoxUpdateRevision.Checked,
							 OpenSetupFileAfterSuccessfullNSIS: checkBoxInstallLocally.Checked,
							 WriteIntoRegistryForWindowsAutostartup: apptoPublish.AutostartWithWindows,//checkBoxAutoStartupWithWindows.Checked,
							 textFeedbackEvent: textFeedbackEvent,
							 progressChanged: progressChangedEvent,
							 OpenFolderAfterSuccessfullNSIS: checkBoxOpenFolder.Checked,
							 OpenWebsite: checkBoxOpenWebsite.Checked);
				}
				else
					UserMessages.ShowWarningMessage("Please choose either local or online.");

				BusyPublishing = false;

				currentProgressBar = null;
			}
			this.TopMost = InitialTopmost;
		}

		private void comboBoxProjectName_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboBoxProjectName.SelectedIndex == -1)
				return;

			checkBoxHasPlugins.Checked = (comboBoxProjectName.SelectedItem as ApplicationToPublish).HasPlugins;
			//checkBoxUpdateRevision.Checked = (comboBoxProjectName.SelectedItem as ApplicationToPublish).UpdateRevisionNumber;
			checkBoxAutoStartupWithWindows.Checked = (comboBoxProjectName.SelectedItem as ApplicationToPublish).AutostartWithWindows;
		}

		private void comboBoxProjectName_TextChanged(object sender, EventArgs e)
		{
			//checkBoxHasPlugins.Enabled = comboBoxProjectName.SelectedIndex != -1;
			//checkBoxUpdateRevision.Enabled = comboBoxProjectName.SelectedIndex != -1;
			//checkBoxAutoStartupWithWindows.Enabled = comboBoxProjectName.SelectedIndex != -1;
			checkBoxHasPlugins.Checked = false;
			//checkBoxUpdateRevision.Checked = false;
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
					Application.DoEvents();
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
			//checkBoxUpdateRevision.Checked = app.UpdateRevisionNumber;
			checkBoxAutoStartupWithWindows.Checked = app.AutostartWithWindows;
		}

		private void checkBoxHasPlugins_CheckedChanged(object sender, EventArgs e)
		{
			ApplicationToPublish app = comboBoxProjectName.SelectedItem as ApplicationToPublish;
			if (app == null) return;
			app.HasPlugins = checkBoxHasPlugins.Checked;
		}

		private void checkBoxUpdateRevision_CheckedChanged(object sender, EventArgs e)
		{
			ApplicationToPublish app = comboBoxProjectName.SelectedItem as ApplicationToPublish;
			if (app == null) return;
			app.UpdateRevisionNumber = checkBoxUpdateRevision.Checked;
		}

		private void checkBoxAutoStartupWithWindows_CheckedChanged(object sender, EventArgs e)
		{
			ApplicationToPublish app = comboBoxProjectName.SelectedItem as ApplicationToPublish;
			if (app == null) return;
			app.AutostartWithWindows = checkBoxAutoStartupWithWindows.Checked;
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (BusyPublishing)
			{
				e.Cancel = true;
				UserMessages.ShowWarningMessage("Busy publishing, please wait before closing...");
			}
		}

		private void checkBoxTopmost_CheckedChanged(object sender, EventArgs e)
		{
			this.TopMost = checkBoxTopmost.Checked;
		}

		private void richTextBoxExMessages_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			string pathWithPrefix = EncodeAndDecodeInterop.DecodeStringHex(e.LinkText.Substring(e.LinkText.LastIndexOf('#') + 1));
			string prefix = pathWithPrefix.Substring(0, pathWithPrefix.IndexOf(':'));
			string path = pathWithPrefix.Substring(pathWithPrefix.IndexOf(':') + 1);
			Range.LinkTypes linkType;
			if (Enum.TryParse<Range.LinkTypes>(prefix, true, out linkType))
			{
				switch (linkType)
				{
					case Range.LinkTypes.ExplorerSelect:
						Process.Start("explorer", "/select,\"" + path + "\"");
						break;
					case Range.LinkTypes.OpenUrl:
						Process.Start(path);
						break;
					default:
						break;
				}
			}
			else
				UserMessages.ShowWarningMessage("Cannot use link, unable to get linktype fro prefix = " + prefix);
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
