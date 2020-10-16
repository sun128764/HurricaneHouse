using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Format
{
    class DbInfo
    {
        public string DataBaseAddress { get; set; }
        public string DataBaseName { get; set; }
        public string MeasurementName { get; set; }
        public bool needAuth { get; set; }
        public string UserName { get; set; }
        public string PassWord { get; set; }
    }
    /// <summary>
    /// Data base list with user name and password.
    /// </summary>
    class DbInfoList : List<DbInfo>
    {
        static public DbInfoList ReadDbList(string path)
        {
            string file = File.ReadAllText(path);
            try
            {
                DbInfoList list = JsonConvert.DeserializeObject<DbInfoList>(file);
                return list;
            }
            catch (JsonSerializationException)
            {
                System.Windows.MessageBox.Show("Can not read setting file. Please choose correct file.");
                return null;
            }
        }

        static public void SaveDbList(DbInfoList list)
        {
            string setting = JsonConvert.SerializeObject(list, Formatting.Indented);
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Setting files (*.json)|*.json",
                AddExtension = true,
                OverwritePrompt = true
            };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, setting);
            }
        }
    }
}
