﻿namespace Format
{
    /// <summary>
    /// IpStackApi json class.
    /// </summary>
    public class IpStackApi
    {
        public string ip { get; set; }
        public string type { get; set; }
        public string continent_code { get; set; }
        public string continent_name { get; set; }
        public string country_code { get; set; }
        public string country_name { get; set; }
        public string region_code { get; set; }
        public string region_name { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }
        public Location location { get; set; }

        public class Location
        {
            public int geoname_id { get; set; }
            public string capital { get; set; }
            public Language[] languages { get; set; }
            public string country_flag { get; set; }
            public string country_flag_emoji { get; set; }
            public string country_flag_emoji_unicode { get; set; }
            public string calling_code { get; set; }
            public bool is_eu { get; set; }
        }

        public class Language
        {
            public string code { get; set; }
            public string name { get; set; }
            public string native { get; set; }
        }
    }
}