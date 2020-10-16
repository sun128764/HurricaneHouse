using System;

namespace Format
{
    /// <summary>
    /// Program setting class.
    /// </summary>
    public class ProgramSetting
    {
        public string ProjectName { set; get; }
        public string CloudPath { set; get; }
        public string LocalPath { set; get; }
        public string ProjectLocation { set; get; }
        public string SensorConfPath { set; get; }
        public TimeSpan _uploadSpan;

        public string UploadSpan
        {
            set
            {
                _uploadSpan = TimeSpan.FromMinutes(Math.Max(1, int.Parse(value)));
            }
            get
            {
                return _uploadSpan.TotalMinutes.ToString("F0");
            }
        }

        public TimeSpan _tokenRefreshSpan;

        public string TokenRefreshSpan
        {
            set
            {
                _tokenRefreshSpan = TimeSpan.FromMinutes(Math.Max(60, int.Parse(value)));
            }
            get
            {
                return _tokenRefreshSpan.TotalMinutes.ToString("F0");
            }
        }

        public string Username { set; get; }
        public string Password { set; get; }
        public string PortName { set; get; }
        public int BaudRate { set; get; }
    }
}