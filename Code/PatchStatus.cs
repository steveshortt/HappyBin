using System;

namespace HappyBin.AutoUpdater
{
	public class PatchStatus
	{
		public RuntimeExeInfo ExeInfo { get; set; }
		public bool PatchIsValid { get; set; }
		public bool PatchIsMandatory { get; set; }
	}
}