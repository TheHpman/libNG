using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NeoTools
{
    public static class FileOps
    {
        public static void strToFile(string fName, string str, bool append = false)
        {
            FileStream fs = null;
            if (append) fs = File.Open(fName, FileMode.Append);
            else fs = File.Create(fName);

            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(str);

            sw.Close();
            fs.Close();
        }
    }
}
