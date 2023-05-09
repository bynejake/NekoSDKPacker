using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace NekoSDKPacker
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (args.Length != 1)
            {
                Console.WriteLine(
                    "Usage:\n" +
                    "   CommandLine:\n" +
                    "       NekoSDKPacker [folder]\n" +
                    "   Or:\n" +
                    "       Simply drag folder into exe.");
                WatingIfNecessary();
                return;
            }

            string folderPath = Path.TrimEndingDirectorySeparator(args[0]);
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("Specified folder doesn't exist.");
                WatingIfNecessary();
                return;
            }

            try
            {
                string filePath = folderPath + ".pak";
                ArchiveWriter.Create(folderPath, filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                WatingIfNecessary();
            }
        }

        private static void WatingIfNecessary()
        {
            if (GetConsoleProcessList(new uint[1], 1) == 1)
            {
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint GetConsoleProcessList(uint[] processList, uint processCount);
    }
}
