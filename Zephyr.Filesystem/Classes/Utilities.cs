using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alphaleonis.Win32.Filesystem;

namespace Zephyr.Filesystem
{
    public static class Utilities
    {
        public static UrlType GetUrlType(string url)
        {
            UrlType type = UrlType.Unknown;

            if (url != null)
            {
                if (url.StartsWith("s3://", StringComparison.OrdinalIgnoreCase))
                {
                    if (IsDirectory(url))
                        type = UrlType.AwsS3Directory;
                    else
                        type = UrlType.AwsS3File;
                }
                else if (url.StartsWith("\\"))
                {
                    if (IsDirectory(url))
                        type = UrlType.NetworkDirectory;
                    else
                        type = UrlType.NetworkFile;
                }
                else
                {
                    if (IsDirectory(url))
                        type = UrlType.LocalDirectory;
                    else
                        type = UrlType.LocalFile;
                }
            }

            return type;
        }

        public static bool IsDirectory(string url)
        {
            bool rc = false;
            if (url != null)
                rc = (url.EndsWith("/") || url.EndsWith(@"\"));
            return rc;
        }

        public static bool IsFile(string url)
        {
            return !IsDirectory(url);
        }

        public static ZephyrFile GetZephyrFile(string url, Clients clients = null)
        {
            ZephyrFile file = null;
            UrlType type = GetUrlType(url);
            switch (type)
            {
                case UrlType.LocalFile:
                    file = new WindowsZephyrFile(url);
                    break;
                case UrlType.NetworkFile:
                    file = new WindowsZephyrFile(url);
                    break;
                case UrlType.AwsS3File:
                    file = new AwsS3ZephyrFile(clients?.aws, url);
                    break;
            }

            return file;
        }

        public static ZephyrDirectory GetZephyrDirectory(string url, Clients clients = null)
        {
            ZephyrDirectory dir = null;
            UrlType type = GetUrlType(url);
            switch (type)
            {
                case UrlType.LocalDirectory:
                    dir = new WindowsZephyrDirectory(url);
                    break;
                case UrlType.NetworkDirectory:
                    dir = new WindowsZephyrDirectory(url);
                    break;
                case UrlType.AwsS3Directory:
                    dir = new AwsS3ZephyrDirectory(clients?.aws, url);
                    break;
                default:
                    throw new Exception($"Url [{url}] Is Not A Known Directory Type.");
            }

            return dir;
        }
    }
}
