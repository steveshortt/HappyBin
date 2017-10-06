using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyBin.AutoUpdater
{
    public class UpdaterSettings
    {
        public string UpdateConfigUri { get; set; }
        public string RuntimeExe { get; set; }
        public string ServiceName { get; set; }
        public bool IsService { get { return !string.IsNullOrWhiteSpace( ServiceName ); } }
        public string ProcessToStop { get { return !string.IsNullOrWhiteSpace( ServiceName ) ? ServiceName : RuntimeExe; } }
        public string DownloadFolder { get; set; }
        public int WaitForExitMillseconds { get; set; }
        public bool StartProcessAfterInstall { get; set; }
    }
}
