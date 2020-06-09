using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace GUI
{
    class DataUpload
    {
        private string fileName;
        private readonly Process p;
        public DataUpload()
        {
            p = new Process();
            p.StartInfo.CreateNoWindow = true;         // 不创建新窗口    
            p.StartInfo.UseShellExecute = false;       //不启用shell启动进程  
            p.StartInfo.RedirectStandardInput = true;  // 重定向输入    
            p.StartInfo.RedirectStandardOutput = true; // 重定向标准输出    
            p.StartInfo.RedirectStandardError = true;  // 重定向错误输出  
            p.StartInfo.FileName = "cmd.exe";
        }
        public void Init()
        {
            try
            {
                p.Start();
                p.StandardInput.WriteLine("tapis auth tokens create");
                p.StandardInput.AutoFlush = true;
                p.StandardInput.WriteLine("exit");
                StreamReader reader = p.StandardOutput;
                string output = reader.ReadToEnd(); //获取错误信息到error
                reader.Close(); //close进程
                p.WaitForExit();  //等待程序执行完退出进程
                p.Close();
            }
            catch (SystemException e)
            {
                
            }
        }
        
        public bool CheckEnv()
        {
            try
            {
                p.Start();
                p.StandardInput.WriteLine("tapis --help");
                p.StandardInput.AutoFlush = true;
                p.StandardInput.WriteLine("exit");
                StreamReader reader = p.StandardOutput;
                string output = reader.ReadToEnd(); //获取错误信息到error
                reader.Close(); //close进程
                p.WaitForExit();  //等待程序执行完退出进程
                p.Close();
                if (output.Contains("Tapis CLI")) return true;
                else return false;
            }
            catch (SystemException e)
            {
                return false;
            }
        }

        public void Upload(string filePath)
        {
            try
            {
                p.Start();
                p.StandardInput.WriteLine("tapis files upload agave://project-6284144844314644966-242ac11c-0001-012/GUI_Test/ "+filePath);
                p.StandardInput.AutoFlush = true;
                p.StandardInput.WriteLine("exit");
                StreamReader reader = p.StandardOutput;
                string output = reader.ReadToEnd(); //获取错误信息到error
                reader.Close(); //close进程
                p.WaitForExit();  //等待程序执行完退出进程
                p.Close();
            }
            catch (SystemException e)
            {

            }
        }
    }
}
