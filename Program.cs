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
                Console.WriteLine(Msg.Usage);
                WatingIfNecessary();
                return;
            }

            string folderPath = Path.TrimEndingDirectorySeparator(args[0]);
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine(Msg.Error);
                WatingIfNecessary();
                return;
            }

            try
            {
                string filePath = folderPath + ".pak";
                ArchiveWriter.Create(folderPath, filePath);
                Console.WriteLine(Msg.Success, filePath);
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
                Console.WriteLine(Msg.Waiting);
                Console.ReadKey();
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint GetConsoleProcessList(uint[] processList, uint processCount);
    }
}
