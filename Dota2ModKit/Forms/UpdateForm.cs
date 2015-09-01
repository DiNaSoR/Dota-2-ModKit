﻿using MetroFramework;
using MetroFramework.Forms;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows.Forms;

namespace Dota2ModKit {
	public partial class UpdateForm : MetroForm {
		MainForm mainForm;
		public UpdateForm(MainForm mainForm) {
			this.mainForm = mainForm;
			InitializeComponent();

			// the user wants to update D2ModKit.

			try {
				// delete D2ModKit.zip if exists.
				if (File.Exists(Path.Combine(Environment.CurrentDirectory, "D2ModKit.zip"))) {
					File.Delete(Path.Combine(Environment.CurrentDirectory, "D2ModKit.zip"));
				}

				progressLabel.Text = "Downloading v" + mainForm.newVers + "...";

				WebClient wc = new WebClient();
				wc.DownloadFileCompleted += wc_DownloadFileCompleted;
				wc.DownloadProgressChanged += wc_DownloadProgressChanged;

				// start downloading.
				wc.DownloadFileAsync(new Uri(mainForm.newVersUrl), Path.Combine(Environment.CurrentDirectory, "D2ModKit.zip"));
			} catch (Exception ex) {
				MetroMessageBox.Show(this, ex.Message,
					ex.ToString(),
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
		}

		private void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e) {
			// delete the nested D2ModKit_temp folder if it exists.
			if (Directory.Exists(Path.Combine(Environment.CurrentDirectory, "D2ModKit_temp"))) {
				Directory.Delete(Path.Combine(Environment.CurrentDirectory, "D2ModKit_temp"), true);
			}

			// extract it now
			string zipPath = Path.Combine(Environment.CurrentDirectory, "D2ModKit.zip");
			ZipFile.ExtractToDirectory(zipPath, Path.Combine(Environment.CurrentDirectory, "D2ModKit_temp"));

			// get the new D2ModKit.exe.
			string tempDir = Path.Combine(Environment.CurrentDirectory, "D2ModKit_temp");
			string path = Path.Combine(tempDir, "D2ModKit.exe");

			// delete D2ModKit_new.exe if it exists.
			if (File.Exists(Path.Combine(Environment.CurrentDirectory, "D2ModKit_new.exe"))) {
				File.Delete(Path.Combine(Environment.CurrentDirectory, "D2ModKit_new.exe"));
			}

			// move the new d2modkit.exe to the main folder, and rename it.
			File.Move(path, Path.Combine(Environment.CurrentDirectory, "D2ModKit_new.exe"));

			// transfer possible new files to the main folder.
			string[] files = Directory.GetFiles(tempDir);
			for (int i = 0; i < files.Length; i++) {
				string name = files[i].Substring(files[i].LastIndexOf('\\') + 1);
				
				// it will raise an exception if file is already there.
				try {
					File.Move(files[i], Path.Combine(Environment.CurrentDirectory, name));
				} catch (Exception) { }
			}

			// transfer possible new dirs to the main folder.
			string[] dirs = Directory.GetDirectories(tempDir);
			for (int i = 0; i < dirs.Length; i++) {
				string name = dirs[i].Substring(dirs[i].LastIndexOf('\\') + 1);
				// it will raise an exception if dir is already there.

				try {
					Directory.Move(dirs[i], Path.Combine(Environment.CurrentDirectory, name));
				} catch (Exception) { }
			}

			//delete the D2ModKit_temp folder.
			try {
				Directory.Delete(Path.Combine(Environment.CurrentDirectory, "D2ModKit_temp"), true);
				// delete .zip
				File.Delete(zipPath);
			} catch (Exception) { }

			// now run our other process to quit this application, and replace the .exe's.
			string batPath = Path.Combine(Environment.CurrentDirectory, "updater.bat");

			// let's always have a fresh batch file.
			if (File.Exists(batPath)) {
				File.Delete(batPath);
			}

			// Create a file to write to.
			string orig = Path.Combine(Environment.CurrentDirectory, "D2ModKit_new.exe");
			string dest = Path.Combine(Environment.CurrentDirectory, "D2ModKit.exe");
			using (StreamWriter sw = File.CreateText(batPath)) {
				sw.WriteLine("taskkill /f /im \"D2ModKit.exe\"");
				sw.WriteLine("del /F /Q " + dest);
				sw.WriteLine("move /Y \"D2ModKit_new.exe\" \"D2ModKit.exe\"");
				sw.WriteLine("start D2ModKit.exe");
				//sw.WriteLine("timeout 10");
				sw.WriteLine("del /F /Q updater.bat");
			}
			Process.Start(batPath);
			Application.Exit();
		}


		private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
			metroProgressBar1.Value = metroProgressBar1.Value + (e.ProgressPercentage - metroProgressBar1.Value);
		}
	}
}
