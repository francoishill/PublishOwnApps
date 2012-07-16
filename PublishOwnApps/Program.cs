using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PublishOwnApps
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

#if NOTUSEFACEDETECTION
					
#else
			if (SharedClasses.FaceDetectionInterop.CheckFaceDetectionDllsExistInCurrentExeDir(true)
				|| UserMessages.Confirm("Due to missing DLLs, application will not be able to do online publishing, continue withouth this support?"))
#endif
			Form1 mainform = new Form1();
			SharedClasses.AutoUpdatingForm.CheckForUpdates(
				exitApplicationAction: delegate { Application.Exit(); },
				ActionIfUptoDate_Versionstring: (versionstring) => { mainform.Text += " (up to date version " + versionstring + ")"; });
			Application.Run(mainform);
		}
	}
}
