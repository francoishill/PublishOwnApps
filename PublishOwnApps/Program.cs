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
			SharedClasses.AutoUpdatingForm.CheckForUpdates();
			Application.Run(new Form1());
		}
	}
}
