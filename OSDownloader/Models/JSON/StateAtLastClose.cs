﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDownloader.Models.JSON
{

    public class StateAtLastClose
    {
        public Osses osses { get; set; }
    }

    public class Osses
    {
        public OS[] os { get; set; }
    }

    public class OS
    {
        public string unique_identifier { get; set; }
        public string selected_action_string { get; set; }
        public string status { get; set; }
        public string boot_wim_url { get; set; }
        public string boot_wim_temp_path { get; set; }
        public long boot_wim_file_size { get; set; }
        public long boot_wim_downloaded_size { get; set; }
        public string boot_wim_status { get; set; }
        public string boot_wim_status_text { get; set; }
        public TimeSpan boot_wim_total_time { get; set; }
        public DateTime boot_wim_added_on { get; set; }
        public DateTime boot_wim_completed_on { get; set; }
        public bool boot_wim_has_error { get; set; }
        public string os_wim_url { get; set; }
        public string os_wim_temp_path { get; set; }
        public string os_wim_file_size { get; set; }
        public string os_wim_downloaded_size { get; set; }
        public string os_wim_status { get; set; }
        public string os_wim_status_text { get; set; }
        public string os_wim_total_time { get; set; }
        public string os_wim_added_on { get; set; }
        public string os_wim_completed_on { get; set; }
        public string os_wim_has_error { get; set; }
    }

}
