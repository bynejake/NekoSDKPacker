using System.Resources;

namespace NekoSDKPacker
{
    internal class Msg
    {
        private static ResourceManager resourceMananger;

        private Msg()
        {
        }

        internal static ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMananger, null))
                {
                    resourceMananger = new ResourceManager("NekoSDKPacker.res.Msg", typeof(Msg).Assembly);
                }
                return resourceMananger;
            }
        }

        internal static string Error
        {
            get
            {
                return ResourceManager.GetString("Error");
            }
        }

        internal static string Success
        {
            get
            {
                return ResourceManager.GetString("Success");
            }
        }

        internal static string Usage
        {
            get
            {
                return ResourceManager.GetString("Usage");
            }
        }

        internal static string Waiting
        {
            get
            {
                return ResourceManager.GetString("Waiting");
            }
        }
    }
}
