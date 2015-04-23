using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Security.Principal;

namespace dnGREP
{
	public partial class OptionsForm : Form
	{
		private static string SHELL_KEY_NAME = "dnGREP";
		private static string OLD_SHELL_KEY_NAME = "nGREP";
		private static string SHELL_MENU_TEXT = "dnGREP...";
		private bool isAdministrator = true;

		public bool IsAdministrator
		{
			get { return isAdministrator; }
			set { isAdministrator = value; changeState(); }
		}
		
		public OptionsForm()
		{
			InitializeComponent();
			oldShellUnregister();
		}

		private void changeState()
		{
			if (!isAdministrator)
			{
				cbRegisterShell.Enabled = false;
				toolTip.SetToolTip(cbRegisterShell, "To set shell integration run dnGREP as Administrator.");
				toolTip.SetToolTip(grShell, "To set shell integration run dnGREP as Administrator.");
			}
			else
			{
				cbRegisterShell.Enabled = true;
				toolTip.SetToolTip(cbRegisterShell, "Shell integration enables running an application from shell context menu.");
				toolTip.SetToolTip(grShell, "");
			}

			if (Properties.Settings.Default.EnableUpdateChecking)
			{
				tbUpdateInterval.Enabled = true;
			}
			else
			{
				tbUpdateInterval.Enabled = false;
			}
			if (Properties.Settings.Default.ShowLinesInContext)
			{
				tbLinesAfter.Enabled = true;
				tbLinesBefore.Enabled = true;
			}
			else
			{
				tbLinesAfter.Enabled = false;
				tbLinesBefore.Enabled = false;
			}
			if (Properties.Settings.Default.UseCustomEditor)
			{
				rbSpecificEditor.Checked = true;
				rbDefaultEditor.Checked = false;
				tbEditorPath.Enabled = true;
				btnBrowse.Enabled = true;
				tbEditorArgs.Enabled = true;
			}
			else
			{
				rbSpecificEditor.Checked = false;
				rbDefaultEditor.Checked = true;
				tbEditorPath.Enabled = false;
				btnBrowse.Enabled = false;
				tbEditorArgs.Enabled = false;
			}
		}

		private bool isShellRegistered(string location)
		{
			if (!isAdministrator)
				return false;

			string regPath = string.Format(@"{0}\shell\{1}",
									   location, SHELL_KEY_NAME);
			try
			{
				return Registry.ClassesRoot.OpenSubKey(regPath) != null;
			}
			catch (UnauthorizedAccessException ex)
			{
				isAdministrator = false;
				return false;
			}
		}

		private void shellRegister(string location)
		{
			if (!isAdministrator)
				return;

			if (!isShellRegistered(location))
			{
				string regPath = string.Format(@"{0}\shell\{1}", location, SHELL_KEY_NAME);

				// add context menu to the registry

				using (RegistryKey key =
					   Registry.ClassesRoot.CreateSubKey(regPath))
				{
					key.SetValue(null, SHELL_MENU_TEXT);
				}

				// add command that is invoked to the registry
				string menuCommand = string.Format("\"{0}\" \"%1\"",
									   Application.ExecutablePath);
				using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(
					string.Format(@"{0}\command", regPath)))
				{
					key.SetValue(null, menuCommand);
				}
			}
		}

		private void shellUnregister(string location)
		{
			if (!isAdministrator)
				return;

			if (isShellRegistered(location))
			{
				string regPath = string.Format(@"{0}\shell\{1}", location, SHELL_KEY_NAME);
				Registry.ClassesRoot.DeleteSubKeyTree(regPath);
			}
		}

		private void oldShellUnregister()
		{
			if (!isAdministrator)
				return;

			string regPath = string.Format(@"Directory\shell\{0}", OLD_SHELL_KEY_NAME);
			if (Registry.ClassesRoot.OpenSubKey(regPath) != null)
			{
				Registry.ClassesRoot.DeleteSubKeyTree(regPath);
			}
		}

		private void checkIfAdmin()
		{
			WindowsIdentity wi = WindowsIdentity.GetCurrent();
			WindowsPrincipal wp = new WindowsPrincipal(wi);

			if (wp.IsInRole("Administrators"))
			{
				isAdministrator = true;
			}
			else
			{
				isAdministrator = false;
			}
		}

		private void OptionsForm_Load(object sender, EventArgs e)
		{
			checkIfAdmin();
			cbRegisterShell.Checked = isShellRegistered("Directory");
			cbCheckForUpdates.Checked = Properties.Settings.Default.EnableUpdateChecking;
			cbShowPath.Checked = Properties.Settings.Default.ShowFilePathInResults;
			cbShowContext.Checked = Properties.Settings.Default.ShowLinesInContext;
			tbLinesBefore.Text = Properties.Settings.Default.ContextLinesBefore.ToString();
			tbLinesAfter.Text = Properties.Settings.Default.ContextLinesAfter.ToString();
			cbSearchFileNameOnly.Checked = Properties.Settings.Default.AllowSearchingForFileNamePattern;
			cbPreviewResults.Checked = Properties.Settings.Default.PreviewResults;
			changeState();
		}
		
		private void cbRegisterShell_CheckedChanged(object sender, EventArgs e)
		{
			if (cbRegisterShell.Checked)
			{
				shellRegister("Directory");
				shellRegister("Drive");
				shellRegister("*");
			}
			else if (!cbRegisterShell.Checked)
			{
				shellUnregister("Directory");
				shellUnregister("Drive");
				shellUnregister("*");
			}
		}

		private void rbEditorCheckedChanged(object sender, EventArgs e)
		{
			if (rbDefaultEditor.Checked)
				Properties.Settings.Default.UseCustomEditor = false;
			else
				Properties.Settings.Default.UseCustomEditor = true;

			changeState();
		}

		private void OptionsForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			Properties.Settings.Default.ContextLinesBefore = int.Parse(tbLinesBefore.Text);
			Properties.Settings.Default.ContextLinesAfter = int.Parse(tbLinesAfter.Text);
			Properties.Settings.Default.AllowSearchingForFileNamePattern = cbSearchFileNameOnly.Checked;
			Properties.Settings.Default.PreviewResults = cbPreviewResults.Checked;
			Properties.Settings.Default.Save();
		}

		public static string GetEditorPath(string file, int line)
		{
			if (!Properties.Settings.Default.UseCustomEditor)
			{
				return file;
			}
			else
			{
				if (!string.IsNullOrEmpty(Properties.Settings.Default.CustomEditor))
				{
					string path = Properties.Settings.Default.CustomEditor.Replace("%file", "\"" + file + "\"").Replace("%line", line.ToString());
					return path;
				}
				else
				{
					return file;
				}
			}
		}

		private void formKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
				Close();
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				tbEditorPath.Text = openFileDialog.FileName;
			}
		}

		private void cbCheckForUpdates_CheckedChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default.EnableUpdateChecking = cbCheckForUpdates.Checked;
			if (tbUpdateInterval.Text.Trim() == "")
				tbUpdateInterval.Text = "1";
			changeState();
		}

		private void cbShowPath_CheckedChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default.ShowFilePathInResults = cbShowPath.Checked;
		}

		private void cbShowContext_CheckedChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default.ShowLinesInContext = cbShowContext.Checked;
			changeState();
		}
	}
}