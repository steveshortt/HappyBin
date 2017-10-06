using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyBin.AutoUpdater
{
    public enum ProcessType
    {
        Process,
        Service
    }

    public class UpdaterSettings
    {
        public string UpdateConfigUri { get; set; }
        public string ProcessName { get; set; }
        public ProcessType ProcessType { get; set; }
        public string DownloadFolder { get; set; }
        public int WaitForExitMillseconds { get; set; }
        public bool StartProcessAfterInstall { get; set; }
    }
}
