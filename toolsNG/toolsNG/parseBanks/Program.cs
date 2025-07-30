using System;
using System.IO;

namespace parseBanks
{
    internal class Program
    {
        static int banksCount;
        static byte[] bankData = new byte[0x100000];
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("parseBanks - libNG / Hpman" + Environment.NewLine + "Truncate output binary to keep used data regions only." + Environment.NewLine);
                Console.WriteLine("Usage: parseBanks [input_file]");
                Environment.Exit(0);
            }
            try
            {
                Console.WriteLine("Truncating " + args[0] + "...");
                FileStream fs = new FileStream(args[0], FileMode.Open);
                if (fs.Length < 0x200000)
                    banksCount = 0;
                else banksCount = (int)(fs.Length >> 24) + 1;

                // relocate bank data
                for (int i = 0; i < banksCount; i++)
                {
                    fs.Seek((i << 24) + 0x200000, SeekOrigin.Begin);
                    fs.Read(bankData, 0, bankData.Length);
                    fs.Seek((i << 20) + 0x100000, SeekOrigin.Begin);
                    fs.Write(bankData, 0, bankData.Length);
                }

                // truncate file
                fs.SetLength((banksCount + 1) << 20);

                // make rom files with classic format
                string fileName;
                FileStream fs2;
                if (args[0].LastIndexOf('.') == -1)
                    fileName = args[0];
                else fileName = args[0].Substring(0, args[0].LastIndexOf('.') + 1);
                // P1
                fs.Seek(0, SeekOrigin.Begin);
                fs.Read(bankData, 0, bankData.Length);
                for (int x = 0; x < bankData.Length; x += 2)
                {
                    byte b = bankData[x];
                    bankData[x] = bankData[x + 1];
                    bankData[x + 1] = b;
                }
                fs2 = new FileStream(fileName + ".P1", FileMode.Create);
                fs2.Write(bankData, 0, bankData.Length);
                fs2.Close();
                // P2
                if (banksCount > 0)
                {
                    fs2 = new FileStream(fileName + ".P2", FileMode.Create);
                    for (int i = 0; i < banksCount; i++)
                    {
                        fs.Read(bankData, 0, bankData.Length);
                        for (int x = 0; x < bankData.Length; x += 2)
                        {
                            byte b = bankData[x];
                            bankData[x] = bankData[x + 1];
                            bankData[x + 1] = b;
                        }
                        fs2.Write(bankData, 0, bankData.Length);
                    }
                    fs2.Close();
                }

                // close file
                fs.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }
        }
    }
}