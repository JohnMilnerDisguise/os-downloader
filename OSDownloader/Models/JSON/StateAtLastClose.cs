using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

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
        public string boot_wim_temp_file_name_only { get; set; }
        public long boot_wim_file_size { get; set; }
        public long boot_wim_downloaded_size { get; set; }
        public string boot_wim_status { get; set; }
        public string boot_wim_status_text { get; set; }
        [JsonConverter(typeof(TimeSpanConverter))]
        public TimeSpan boot_wim_elapsed_time { get; set; }
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime boot_wim_added_on { get; set; }
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime boot_wim_completed_on { get; set; }
        public bool boot_wim_has_error { get; set; }
        public string os_wim_url { get; set; }
        public string os_wim_temp_path { get; set; }
        public string os_wim_temp_file_name_only { get; set; }
        public long os_wim_file_size { get; set; }
        public long os_wim_downloaded_size { get; set; }
        public string os_wim_status { get; set; }
        public string os_wim_status_text { get; set; }
        [JsonConverter(typeof(TimeSpanConverter))]
        public TimeSpan os_wim_elapsed_time { get; set; }
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime os_wim_added_on { get; set; }
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime os_wim_completed_on { get; set; }
        public bool os_wim_has_error { get; set; }
    }

}
