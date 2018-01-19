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

        /// <summary>
        /// In Windows, it is impossible to determine by name along if a URL is a directory or a file with no extension.  Thus
        /// the decision was made that all directory url's MUST end with a forward or backward slash (/ or \).  This removes any
        /// ambiguity.   This standard should be carried forward to all other implementation types (Aws, Azure, FTP, etc...)
        /// </summary>
        /// <param name="url">The full path to the object.</param>
        /// <returns></returns>
        public static bool IsDirectory(string url)
        {
            bool rc = false;
            if (url != null)
                rc = (url.EndsWith("/") || url.EndsWith(@"\"));
            return rc;
        }

        /// <summary>
        /// In Windows, it is impossible to determine by name along if a URL is a directory or a file with no extension.  Thus
        /// the decision was made that all directory url's MUST end with a forward or backward slash (/ or \).  This removes any
        /// ambiguity.   This standard should be carried forward to all other implementation types (Aws, Azure, FTP, etc...)
        /// </summary>
        /// <param name="url">The full path to the object.</param>
        /// <returns></returns>
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

        public static ZephyrFile CreateFile(string fileName, Clients clients = null, bool overwrite = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            ZephyrFile file = Utilities.GetZephyrFile(fileName, clients);
            return file.Create(overwrite, callbackLabel, callback);
        }

        public static ZephyrDirectory CreateDirectory(string dirName, Clients clients = null, bool failIfExists = false, String callbackLabel = null, Action<string, string> callback = null)
        {
            ZephyrDirectory dir = Utilities.GetZephyrDirectory(dirName, clients);
            return dir.Create(failIfExists, callbackLabel, callback);
        }

        public static void Delete(string name, Clients clients = null, bool recurse = true, bool stopOnError = true, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            if (Utilities.IsDirectory(name))
            {
                ZephyrDirectory dir = Utilities.GetZephyrDirectory(name, clients);
                dir.Delete(recurse, stopOnError, verbose, callbackLabel, callback);
            }
            else
            {
                ZephyrFile file = Utilities.GetZephyrFile(name, clients);
                file.Delete(stopOnError, verbose, callbackLabel, callback);
            }
        }

        public static bool Exists(string name, Clients clients = null)
        {
            if (Utilities.IsDirectory(name))
            {
                ZephyrDirectory dir = Utilities.GetZephyrDirectory(name, clients);
                return dir.Exists();
            }
            else
            {
                ZephyrFile file = Utilities.GetZephyrFile(name, clients);
                return file.Exists();
            }

        }


    }
}
