using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Taskbar;
using SharedClasses;

namespace PublishOwnApps
{
	public partial class MainForm : Form
	{
		TextFeedbackEventHandler textFeedbackEvent;
		ProgressChangedEventHandler progressChangedEvent;
		string AppNameReceivedViaCommandline = null;

		public MainForm()
		{
			InitializeComponent();

			checkBoxTopmost.Checked = this.TopMost;

			WindowMessagesInterop.InitializeClientMessages();

			textFeedbackEvent += new TextFeedbackEventHandler(OnTextFeedbackEvent);
			progressChangedEvent += new ProgressChangedEventHandler(OnProgressChangedEvent);

			//CustomBalloonTipwpf.ShowCustomBalloonTip(
			//	"Test title",
			//	"Message 123",
			//	2000,
			//	CustomBalloonTipwpf.IconTypes.Information);
		}

		private void PopulateApplicationsList()
		{
			var _jumpList = JumpList.CreateJumpList();

			comboBoxProjectName.Items.Clear();
			_jumpList.ClearAllUserTasks();

			JumpListCustomCategory userActionsCategory = new JumpListCustomCategory("Publish applications");

			//foreach (string item in GlobalSettings.PublishSettings.Instance.ListedApplicationNames.Split('|').OrderBy(s => s))
			int listcnt = 0;
			foreach (string appname in SettingsSimple.PublishSettings.Instance.ListedApplicationNames.OrderBy(s => s))
			{
				comboBoxProjectName.Items.Add(
					new ApplicationToPublish(
						appname,
						HasPlugins: appname.Equals("QuickAccess", StringComparison.InvariantCultureIgnoreCase),
						UpdateRevisionNumber: false,
						AutostartWithWindows: appname.Equals(StringComparison.InvariantCultureIgnoreCase,
							"ApplicationManager", "MonitorSystem", "QuickAccess", "StartupTodoManager", "TestingMonitorSubversion")
						));

				string appnameWithSpaces = appname.InsertSpacesBeforeCamelCase();
				string appExePath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86).TrimEnd('\\') + string.Format("\\{0}\\{1}.exe", appnameWithSpaces, appname);
				JumpListLink actionPublishQuickAccess = new JumpListLink(Assembly.GetEntryAssembly().Location, appnameWithSpaces);
				actionPublishQuickAccess.Arguments = appname;
				if (File.Exists(appExePath))
					actionPublishQuickAccess.IconReference = new Microsoft.WindowsAPICodePack.Shell.IconReference(appExePath, 0);
				userActionsCategory.AddJumpListItems(actionPublishQuickAccess);
				listcnt++;
			}
			if (listcnt > _jumpList.MaxSlotsInList)
				UserMessages.ShowWarningMessage(
					string.Format("The taskbar jumplist has {0} maximum slots but the list is {1}, the extra items will be truncated", _jumpList.MaxSlotsInList, listcnt));

			_jumpList.AddCustomCategories(userActionsCategory);
			_jumpList.Refresh();

			if (Environment.GetCommandLineArgs().Length == 2)
				//Probably passed via windows 7 JumpList, see what happens in Form1_Shown
				AppNameReceivedViaCommandline = Environment.GetCommandLineArgs()[1];

			this.Cursor = Cursors.Default;
			if (AppNameReceivedViaCommandline == null)
				comboBoxProjectName.DroppedDown = true;
			else
			{
				comboBoxProjectName.Focus();
				this.comboBoxProjectName.Text = AppNameReceivedViaCommandline;
				ApplicationToPublish apptoPublishFromCommandline = this.comboBoxProjectName.SelectedItem as ApplicationToPublish;
				if (apptoPublishFromCommandline != null)
				{
					this.WindowState = FormWindowState.Minimized;
					buttonPublishNow.PerformClick();
				}
			}
			this.Cursor = Cursors.Default;
			Application.DoEvents();
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

		private void Form1_Load(object sender, EventArgs e)
		{
			TaskbarManager.Instance.ApplicationId = ThisAppName;

			//OnTextFeedbackEvent(null, new TextFeedbackEventArgs("Hallo my name is Francois Hill", TextFeedbackType.Subtle, new Range(17, 8, Range.LinkTypes.OpenUrl)));
		}

		private void Form1_Shown(object sender, EventArgs e)
		{
			PopulateApplicationsList();
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
							EncodeAndDecodeInterop.EncodeStringHex(prefix + hyperlink, err => UserMessages.ShowErrorMessage(err)));

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

		private void SetWindows7progress(int currentValue, int maximumValue)
		{
			TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
			TaskbarManager.Instance.SetProgressValue(currentValue, maximumValue);
		}

		private void OnProgressChangedEvent(object sender, ProgressChangedEventArgs e)
		{
			Action action = new Action(
				delegate
				{
					SetWindows7progress(e.CurrentValue, e.MaximumValue);
					progressBar.Maximum = e.MaximumValue;
					progressBar.Value = e.CurrentValue;
					if (progressBar.Maximum == progressBar.Value)
					{
						progressBar.Value = 0;
						SetWindows7progress(0, e.MaximumValue);
					}
				});
			if (this.InvokeRequired)
				this.Invoke(action);
			else
				action();
		}

		//private ThreadingInterop.WaitIndicator currentProgressBar = null;
		private bool InitialTopmost = false;
		private void buttonPublishNow_Click(object sender, EventArgs e)
		{
			if (comboBoxProjectName.Text.Trim().Length == 0)
				UserMessages.ShowWarningMessage("Please select a project name first.");
			else
			{
				TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Indeterminate, this.Handle);
				try
				{
					if (comboBoxProjectName.SelectedIndex == -1)
						PublishApplication(new ApplicationToPublish(comboBoxProjectName.Text, checkBoxHasPlugins.Checked, checkBoxUpdateRevision.Checked, checkBoxAutoStartupWithWindows.Checked));
					else
						PublishApplication(comboBoxProjectName.SelectedItem as ApplicationToPublish);
				}
				finally
				{
					TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress, this.Handle);
				}
			}
		}

		private void buttonPublishList_Click(object sender, EventArgs e)
		{
			if (comboBoxProjectName.SelectedIndex != -1)
				UserMessages.ShowInfoMessage("There is still an item in the combobox, click to add it to the list or clear it before continuing.");
			else
			{
				TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Indeterminate, this.Handle);
				try
				{
					foreach (TreeNode node in treeViewPublishList.Nodes)
						PublishApplication(node.Tag as ApplicationToPublish);
				}
				finally
				{
					TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress, this.Handle);
				}
			}
		}

		private bool BusyPublishing = false;
		private const string ThisAppName = "PublishOwnApps";
		private void PublishApplication(ApplicationToPublish apptoPublish)
		{
			InitialTopmost = this.TopMost;
			this.TopMost = false;
			//using (ThreadingInterop.WaitIndicator wi = new ThreadingInterop.WaitIndicator(this))
			//{
			if (BusyPublishing)
			{
				UserMessages.ShowWarningMessage("Publishing is busy, please be patient...");
				return;
			}

			BusyPublishing = true;

			//currentProgressBar = wi;
			//UpdateProgressBarPosition();

			if (radioButtonLocal.Checked)
			{
				string tmpNoUseVersionStr;
				string tmpNoUseSetupPath;
				bool publishResult = PublishInterop.PerformPublish(
					projName: apptoPublish.ApplicationName,
					_64Only: false,
					HasPlugins: apptoPublish.HasPlugins,
					AutomaticallyUpdateRevision: true,//apptoPublish.UpdateRevisionNumber,//checkBoxUpdateRevision.Checked,
					InstallLocallyAfterSuccessfullNSIS: checkBoxInstallLocally.Checked,
					StartupWithWindows: apptoPublish.AutostartWithWindows,
					SelectSetupIfSuccessful: checkBoxOpenFolder.Checked,
					publishedVersionString: out tmpNoUseVersionStr,
					publishedSetupPath: out tmpNoUseSetupPath,
					actionOnMessage: (mes, msgtype) =>
					{
						TextFeedbackType tmpFeedbackType = TextFeedbackType.Subtle;
						switch (msgtype)
						{
							case FeedbackMessageTypes.Success:
								tmpFeedbackType = TextFeedbackType.Success;
								break;
							case FeedbackMessageTypes.Error:
								tmpFeedbackType = TextFeedbackType.Error;
								break;
							case FeedbackMessageTypes.Warning:
								tmpFeedbackType = TextFeedbackType.Noteworthy;
								break;
							case FeedbackMessageTypes.Status:
								tmpFeedbackType = TextFeedbackType.Subtle;
								break;
						}
						OnTextFeedbackEvent(null, new TextFeedbackEventArgs(mes, tmpFeedbackType));
					},
					actionOnProgressPercentage: (progperc) =>
					{
						OnProgressChangedEvent(null, new ProgressChangedEventArgs(progperc, 100));
					});//Only used when downloading DotNetChecker.dll

				if (!publishResult)
					OnTextFeedbackEvent(null, new TextFeedbackEventArgs("UNABLE to publish " + apptoPublish.ApplicationName, TextFeedbackType.Error));
			}
			else if (radioButtonOnline.Checked)
			{
				string tmpNoUsePublishedVersionString;
				string tmpNoUsePublishedSetupPath;

				bool publishResult = PublishInterop.PerformPublishOnline(
					projName: apptoPublish.ApplicationName,
					_64Only: false,//Not only 64bit
					HasPlugins: apptoPublish.HasPlugins,
					AutomaticallyUpdateRevision: true,//apptoPublish.UpdateRevisionNumber,//checkBoxUpdateRevision.Checked,
					InstallLocallyAfterSuccessfullNSIS: checkBoxInstallLocally.Checked,
					StartupWithWindows: apptoPublish.AutostartWithWindows,
					SelectSetupIfSuccessful: checkBoxOpenFolder.Checked,
					OpenWebsite: checkBoxOpenWebsite.Checked,
					publishedVersionString: out tmpNoUsePublishedVersionString,
					publishedSetupPath: out tmpNoUsePublishedSetupPath,
					actionOnMessage: (mes, msgtype) =>
					{
						TextFeedbackType tmpFeedbackType = TextFeedbackType.Subtle;
						switch (msgtype)
						{
							case FeedbackMessageTypes.Success:
								tmpFeedbackType = TextFeedbackType.Success;
								break;
							case FeedbackMessageTypes.Error:
								tmpFeedbackType = TextFeedbackType.Error;
								break;
							case FeedbackMessageTypes.Warning:
								tmpFeedbackType = TextFeedbackType.Noteworthy;
								break;
							case FeedbackMessageTypes.Status:
								tmpFeedbackType = TextFeedbackType.Subtle;
								break;
						}
						OnTextFeedbackEvent(null, new TextFeedbackEventArgs(mes, tmpFeedbackType));
					},
					actionOnProgressPercentage: (progperc) =>
					{
						OnProgressChangedEvent(null, new ProgressChangedEventArgs(progperc, 100));
					});

				if (!publishResult)
					OnTextFeedbackEvent(null, new TextFeedbackEventArgs("UNABLE to publish " + apptoPublish.ApplicationName, TextFeedbackType.Error));
			}
			else
				UserMessages.ShowWarningMessage("Please choose either local or online.");

			BusyPublishing = false;

			//currentProgressBar = null;
			//}
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
			//UpdateProgressBarPosition();
		}

		private void Form1_SizeChanged(object sender, EventArgs e)
		{
			//UpdateProgressBarPosition();
		}

		/*private void UpdateProgressBarPosition()
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
		}*/

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

		private void labelAbout_Click(object sender, EventArgs e)
		{
			AboutWindow2.ShowAboutWindow(new System.Collections.ObjectModel.ObservableCollection<DisplayItem>()
			{
				new DisplayItem("Author", "Francois Hill"),
				new DisplayItem("Icon(s) obtained from", null)
			});
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
