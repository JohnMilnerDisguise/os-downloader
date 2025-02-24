using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGet
{

    public class APIContentsRootObject
    {
        public long last_updated { get; set; }
        public Model[] models { get; set; }
    }

    public class Model
    {
        public bool has_recovery_drive { get; set; }
        public string boot_type { get; set; }
        public string name { get; set; }
        public string d3Model { get; set; }
        public Redisguis[] redisguises { get; set; }
    }

    public class Redisguis
    {
        public long os_wim_file_size { get; set; }
        public string iso_file_name { get; set; }
        public long iso_file_size { get; set; }
        public string release_notes { get; set; }
        public long release_date { get; set; }
        public bool r2_sends_remote_postboot_logs { get; set; }
        public string aws_url_boot_wim { get; set; }
        public string aws_url_os_wim { get; set; }
        public string aws_url_iso { get; set; }
        public string redisguise_name { get; set; }
        public string redisguise_handle { get; set; }
        public bool r2_sends_remote_reimager_logs { get; set; }
        public string os_wim_file_name { get; set; }
        public int boot_wim_file_size { get; set; }
        public string boot_wim_file_name { get; set; }
        public bool r2_is_compatible { get; set; }
        public string os_family_handle { get; set; }
        public Public_Version_Table[] public_version_table { get; set; }
    }

    public class Public_Version_Table
    {
        public string Package { get; set; }
        public string Version { get; set; }
    }

}
