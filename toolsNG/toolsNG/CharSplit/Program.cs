using System;
using System.IO;

namespace CharSplit
{
    class Program
    {
        static int i;
        static byte[] buffer;
        static byte[] outdata1;
        static byte[] outdata2;
        static bool convCart = false;
        static bool convCD = false;
        static string in_File;
        static string out_File;

        static void showUsage(bool showApp = true)
        {
            if (showApp)
                Console.WriteLine("CharSplit v1.0a - libNG / Hpman" + Environment.NewLine + "Converts raw NeoGeo char data to ROM/CD format." + Environment.NewLine);
            Console.WriteLine("Usage: CharSplit [input_file] <options> [output_file_prefix]");
            Console.WriteLine("Options:" + Environment.NewLine + "\t-rom\tOuput to ROM format ([output_file_prefix].C1 & .C2)" + Environment.NewLine + "\t-cd\tOutput to CD format ([output_file_prefix].SPR)");
#if DEBUG
            Console.Read();
#endif
        }

        static bool parseArg(string arg)
        {
            if (arg.ToLower() == "-rom")
            {
                convCart = true;
                return true;
            }
            if (arg.ToLower() == "-cd")
            {
                convCD = true;
                return true;
            }
            Console.WriteLine("Invalid option " + arg + "\n");
            showUsage(false);
            return false;
        }

        static void toCart(string outFile)
        {
            i = 0;

            outdata1 = new byte[buffer.Length / 2];
            outdata2 = new byte[buffer.Length / 2];

            while (i < buffer.Length)
            {
                outdata1[(i / 2)] = buffer[i];
                outdata2[(i / 2)] = buffer[i + 1];
                outdata1[(i / 2) + 1] = buffer[i + 2];
                outdata2[(i / 2) + 1] = buffer[i + 3];
                i += 4;
            }
            try
            {
                File.WriteAllBytes(outFile + ".C1", outdata1);
                File.WriteAllBytes(outFile + ".C2", outdata2);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error writing output file(s)");
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }
        }

        static void toCD(string outFile)
        {
            i = 0;
            outdata1 = new byte[buffer.Length];

            while (i < buffer.Length)
            {
                outdata1[i] = buffer[i + 2];
                outdata1[i + 1] = buffer[i];
                outdata1[i + 2] = buffer[i + 3];
                outdata1[i + 3] = buffer[i + 1];
                i += 4;
            }
            try
            {
                File.WriteAllBytes(outFile + ".SPR", outdata1);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error writing output file(s)");
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }
        }

        static void Main(string[] args)
        {
            if (args == null)
            {
                showUsage();
                return;
            }
            if (args.Length == 0)
            {
                showUsage();
                return;
            }
            if ((args.Length < 2) || (args.Length > 4))
            {
                showUsage();
                return;
            }

            switch (args.Length)
            {
                case 2:
                    in_File = args[0];
                    out_File = args[1];
                    convCart = true;
                    break;
                case 3:
                    in_File = args[0];
                    out_File = args[2];
                    if (!parseArg(args[1])) return;
                    break;
                case 4:
                    in_File = args[0];
                    out_File = args[3];
                    if (!parseArg(args[1])) return;
                    if (!parseArg(args[2])) return;
                    break;
            }

            //parsed ok
            try
            {
                buffer = File.ReadAllBytes(in_File);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error opening " + in_File);
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }

            if (buffer.Length % 4 != 0)
            {
                Console.WriteLine("Incorrect input file length.");
                Environment.Exit(1);
            }

            if (convCart) toCart(out_File);
            if (convCD) toCD(out_File);
        }
    }
}
