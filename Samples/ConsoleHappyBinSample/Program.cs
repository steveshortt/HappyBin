using System;

namespace ConsoleHappyBinSample
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine( System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString() );
			System.Diagnostics.Process.Start( "HappyBinCli.exe" );
		}
	}
}