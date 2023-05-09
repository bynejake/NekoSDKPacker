using System;
using System.IO;
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
                WaitingForUser();
                return;
            }

            if (!Directory.Exists(args[0]))
            {
                Console.WriteLine("Specified folder doesn't exist.");
                WaitingForUser();
                return;
            }

            try
            {
                ArchiveWriter.Create(args[0], args[0] + ".pak");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                WaitingForUser();
            }
        }

        private static void WaitingForUser()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
