using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace HappyBin.AutoUpdater
{
	public class RuntimeExeInfo
	{
		public RuntimeExeInfo(string filePath)
		{
			this.Name = Path.GetFileName( filePath );
			this.FolderPath = Path.GetDirectoryName( filePath );
			if( !Path.IsPathRooted( this.FolderPath ) )
			{
				this.FolderPath = Path.Combine( Directory.GetCurrentDirectory(), this.FolderPath );
			}
			this.FullPath = Path.Combine( this.FolderPath, this.Name );

			if( this.Exists )
			{
				FileVersionInfo ver = FileVersionInfo.GetVersionInfo( this.FullPath );
				this.Version = new Version( ver.ProductVersion );
			}
		}

		public string Name { get; private set; }
		public string FolderPath { get; private set; }
		public string FullPath { get; private set; }
		public bool Exists { get { return File.Exists( this.FullPath ); } }
		public Version Version { get; private set; }
	}
}