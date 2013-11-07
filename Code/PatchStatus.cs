using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HappyBin.AutoUpdater
{
	public class PatchStatus
	{
		public RuntimeExeInfo ExeInfo { get; set; }
		public bool PatchIsValid { get; set; }
		public bool PatchIsMandatory { get; set; }
	}
}