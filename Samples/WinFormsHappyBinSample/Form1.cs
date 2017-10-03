using System;
using System.Windows.Forms;

namespace WinFormsHappyBinSample
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();

			this.Text = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start( "HappyBin.AutoUpdater.exe" );
		}
	}
}