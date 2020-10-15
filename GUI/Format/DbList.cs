
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace GUI.Format
{
    class DbInfo
    {
        string Url;
        bool NeedPassWord;
        string User;
        string PassWord;
    }
    /// <summary>
    /// Data base list with user name and password.
    /// </summary>
    class DbList : List<DbInfo>
    {
        static public DbList ReadDbList(string path)
        {
            string file = File.ReadAllText(path);
            try
            {
                DbList list = JsonConvert.DeserializeObject<DbList>(file);
                return list;
            }
            catch (JsonSerializationException)
            {
                System.Windows.MessageBox.Show("Can not read setting file. Please choose correct file.");
                return null;
            }
        }

        static public void SaveDbList(DbList list)
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
