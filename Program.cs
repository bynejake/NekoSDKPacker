using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace NekoSDKPacker
{
    class Program
    {
        private static string[] MsgZH =
        {
            "用法:\n" +
            "   命令行:\n" +
            "       NekoSDKPacker [目录]\n" +
            "   或者:\n" +
            "       将文件夹拖放到exe上",
            "目录不存在。",
            "打包成功: {0}",
            "请按任意键继续..."
        };

        private static string[] MsgEN =
        {
            "Usage:\n" +
            "   CommandLine:\n" +
            "       NekoSDKPacker [folder]\n" +
            "   Or:\n" +
            "       Simply drag folder into exe",
            "Specified folder doesn't exist.",
            "Pack successfully: {0}",
            "Press any key to continue..."
        };

        private static string[] Msg = CultureInfo.InstalledUICulture.Name.StartsWith("zh") ? MsgZH : MsgEN;

        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (args.Length != 1)
            {
                Console.WriteLine(Msg[0]);
                WatingIfNecessary();
                return;
            }

            string folderPath = Path.TrimEndingDirectorySeparator(args[0]);
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine(Msg[1]);
                WatingIfNecessary();
                return;
            }

            try
            {
                string filePath = folderPath + ".pak";
                ArchiveWriter.Create(folderPath, filePath);
                Console.WriteLine(Msg[2], filePath);
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
                Console.WriteLine(Msg[3]);
                Console.ReadKey();
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint GetConsoleProcessList(uint[] processList, uint processCount);
    }
}
