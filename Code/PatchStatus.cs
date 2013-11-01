using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HappyBin.AutoUpdater
{
	public class PatchStatus
	{
		public RuntimeExeInfo ExeInfo { get; set; }
		public bool PatchFilePathExists { get; set; }
		public bool PatchIsMandatory { get; set; }
	}
}