using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDownloader.Models.JSON
{
    public class ExistingDeploymentShareContents
    {
        public Boot_Wims[] boot_wims { get; set; }
        public Os_Wims[] os_wims { get; set; }
    }

    public class Boot_Wims
    {
        public string handle { get; set; }
        public string file_name { get; set; }
        public int file_size { get; set; }
    }

    public class Os_Wims
    {
        public string handle { get; set; }
        public string file_name { get; set; }
        public long file_size { get; set; }
    }
}






