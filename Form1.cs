using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.Collections;
using System.Windows.Forms;

namespace FileManager
{
	public partial class Form1 : Form
	{
		private string saved;
		public Form1()
		{
			InitializeComponent();
			UpdateList();
			listBox1.ContextMenu = new ContextMenu() {
				MenuItems = {
					new MenuItem("Properties", Properties_Click),
					new MenuItem("Refresh", (s, e) => UpdateList())
				}
			};
			listBox1.ContextMenu.Popup += Tools_Popup;
			refreshToolStripMenuItem.Click += (s, e) => UpdateList();
		}
		private void textBox1_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				listBox1.Focus();
		}
		private void textBox1_Enter(object sender, EventArgs e)
		{
			saved = textBox1.Text;
			button1.Enabled = false;
		}
		private void textBox1_Leave(object sender, EventArgs e)
		{
			if (textBox1.Text != String.Empty && !Directory.Exists(textBox1.Text))
			{
				MessageBox.Show("Could not found this directory.");
				textBox1.Text = saved;
			}
			saved = null;
			UpdateList();
		}

		private void UpdateList()
		{
			button1.Enabled = textBox1.Text != String.Empty;

			listBox1.Items.Clear();
			if (textBox1.Text == String.Empty)
			{
				foreach (DriveInfo driver in DriveInfo.GetDrives())
					if (driver.DriveType == DriveType.Fixed)
						listBox1.Items.Add(driver.Name);
			}
			else
			{
				foreach (string file in Directory.EnumerateFileSystemEntries(textBox1.Text))
					listBox1.Items.Add(Path.GetFileName(file));
			}
		}
		private void listBox1_DoubleClick(object sender, EventArgs e)
		{
			if (listBox1.SelectedIndex != -1)
			{
				string newFile = Path.Combine(textBox1.Text, listBox1.SelectedItem as string);
				if (File.GetAttributes(newFile).HasFlag(FileAttributes.Directory))
				{
					textBox1.Text = newFile;
					UpdateList();
				}
				else MessageBox.Show("This is a file");
			}
		}
		private void button1_Click(object sender, EventArgs e)
		{
			string parent = Path.GetFullPath(Path.Combine(textBox1.Text, ".."));
			textBox1.Text = textBox1.Text == parent ? String.Empty : parent;
			UpdateList();
		}

		private void Form1_SizeChanged(object sender, EventArgs e)
		{
			textBox1.Width = ClientSize.Width - textBox1.Location.X - button1.Location.X;
			listBox1.Size = new Size(
				ClientSize.Width - listBox1.Location.X - listBox1.Location.X,
				ClientSize.Height - listBox1.Location.Y - listBox1.Location.X);
		}
		private void Tools_Popup(object sender, EventArgs e)
		{
			IEnumerable items = sender is ContextMenu ?
				(sender as ContextMenu).MenuItems as IEnumerable :
				(sender as ToolStripMenuItem).DropDownItems as IEnumerable;

			IEnumerator item = items.GetEnumerator();
			item.Reset();
			item.MoveNext();
			PropertyInfo enabled = item.Current.GetType().GetProperty("Enabled");
			enabled.SetValue(item.Current, listBox1.SelectedIndex != -1);
		}
		private void Properties_Click(object sender, EventArgs e)
		{
			if (listBox1.SelectedIndex == -1) return;

			StringBuilder sb = new StringBuilder(150);
			string fullPath = Path.Combine(textBox1.Text, listBox1.SelectedItem as string);
			bool isDirecory = File.GetAttributes(fullPath).HasFlag(FileAttributes.Directory);
			FileSystemInfo info = isDirecory ? new DirectoryInfo(fullPath) as FileSystemInfo : new FileInfo(fullPath) as FileSystemInfo;

			sb.Append("Type:\t\t");
			sb.Append(isDirecory ? "Directory" : "File");
			sb.Append(Environment.NewLine);

			if (!isDirecory)
			{
				sb.Append("Size:\t\t");
				sb.Append(FormatSize((info as FileInfo).Length));
				sb.Append(Environment.NewLine);
			}

			sb.Append("Creation time:\t");
			sb.Append(info.CreationTime.ToString());
			sb.Append(Environment.NewLine);

			sb.Append("Last write time:\t");
			sb.Append(info.LastWriteTime.ToString());
			sb.Append(Environment.NewLine);

			sb.Append("Last access time:\t");
			sb.Append(info.LastAccessTime.ToString());
			MessageBox.Show(sb.ToString());
		}

		private void Font_Click(object sender, EventArgs e)
		{
			FontDialog dialog = new FontDialog();
			dialog.ShowColor = true;
			dialog.Font = listBox1.Font;
			dialog.Color = listBox1.ForeColor;
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				listBox1.Font = dialog.Font;
				listBox1.ForeColor = dialog.Color;
			}
		}
		private void Back_Click(object sender, EventArgs e)
		{
			ColorDialog dialog = new ColorDialog();
			dialog.Color = listBox1.BackColor;
			if (dialog.ShowDialog() == DialogResult.OK)
				listBox1.BackColor = dialog.Color;
		}

		private static readonly string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB" };
		public static string FormatSize(long bytes)
		{
			int counter = 0;
			double number = bytes;
			while (Math.Round(number / 1024) >= 1)
			{
				number = number / 1024;
				++counter;
			}
			return string.Format("{0:F2} {1}", number, suffixes[counter]);
		}
	}
}
