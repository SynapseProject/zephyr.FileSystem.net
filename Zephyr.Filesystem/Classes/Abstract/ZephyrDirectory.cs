using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Zephyr.Filesystem
{
    public abstract class ZephyrDirectory
    {
        public abstract String FullName { get; set; }

        public abstract String Name { get; }
        public abstract String Parent { get; }
        public abstract String Root { get; }

        public abstract ZephyrDirectory Create(string childDirName = null, bool failIfExists = false, String callbackLabel = null, Action<string, string> callback = null);
        public abstract void Delete(string dirName = null, bool recurse = true, bool stopOnError = true, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null);
        public abstract bool Exists(string dirName = null);

        public abstract ZephyrFile CreateFile(string fullName, String callbackLabel = null, Action<string, string> callback = null);

        public abstract IEnumerable<ZephyrDirectory> GetDirectories();
        public abstract IEnumerable<ZephyrFile> GetFiles();
        public abstract String PathCombine(params string[] paths);

        public void CopyTo(ZephyrDirectory target, bool recurse = true, bool overwrite = true, bool stopOnError = true, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            if (this.Exists())
            {
                foreach (ZephyrDirectory childDir in GetDirectories())
                {
                    try
                    {
                        String targetDirName = target.PathCombine(target.FullName, $"{childDir.Name}/");
                        ZephyrDirectory targetChild = target.Create(targetDirName);
                        if (recurse)
                            childDir.CopyTo(targetChild, recurse, overwrite, verbose, stopOnError, callbackLabel, callback);
                    }
                    catch (Exception e)
                    {
                        Logger.Log(e.Message, callbackLabel, callback);
                        if (stopOnError)
                            throw;
                    }
                }

                foreach (ZephyrFile file in GetFiles())
                {
                    try
                    {
                        String targetFileName = target.PathCombine(target.FullName, file.Name);
                        ZephyrFile targetFile = target.CreateFile(targetFileName, callbackLabel, callback);
                        file.CopyTo(targetFile, overwrite, stopOnError, verbose, callbackLabel, callback);
                    }
                    catch (Exception e)
                    {
                        Logger.Log(e.Message, callbackLabel, callback);
                        if (stopOnError)
                            throw;
                    }
                }

                if (verbose)
                    Logger.Log($"Copied Directory [{this.FullName}] to [{target.FullName}].", callbackLabel, callback);
            }
            else
            {
                string message = $"[{this.FullName}] Does Not Exist.";
                Logger.Log(message, callbackLabel, callback);
                if (stopOnError)
                    throw new Exception(message);
            }
        }

        public void MoveTo(ZephyrDirectory target, bool overwrite = true, bool stopOnError = true, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            if (this.Exists())
            {
                foreach (ZephyrDirectory childDir in GetDirectories())
                {
                    try
                    {
                        ZephyrDirectory targetChild = target.Create(childDir.Name);
                        childDir.MoveTo(targetChild, overwrite, stopOnError, verbose, callbackLabel, callback);
                        childDir.Delete(verbose: false);
                    }
                    catch (Exception e)
                    {
                        Logger.Log(e.Message, callbackLabel, callback);
                        if (stopOnError)
                            throw;
                    }
                }

                foreach (ZephyrFile file in GetFiles())
                {
                    try
                    {
                        String targetFileName = target.PathCombine(target.FullName, file.Name);
                        ZephyrFile targetFile = file.Create(targetFileName, overwrite);
                        file.MoveTo(targetFile, stopOnError, overwrite, verbose, callbackLabel, callback);
                    }
                    catch (Exception e)
                    {
                        Logger.Log(e.Message, callbackLabel, callback);
                        if (stopOnError)
                            throw;
                    }
                }

                if (verbose)
                    Logger.Log($"Moved Directory [{this.FullName}] to [{target.FullName}].", callbackLabel, callback);
            }
            else
            {
                string message = $"[{this.FullName}] Does Not Exist.";
                Logger.Log(message, callbackLabel, callback);
                if (stopOnError)
                    throw new Exception(message);
            }
        }

        public bool IsEmpty()
        {
            return (GetDirectories().Count() == 0 && GetFiles().Count() == 0);
        }

        public void Clear(string dirName = null, bool stopOnError = true, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            foreach ( ZephyrDirectory dir in GetDirectories() )
                dir.Delete(null, true, stopOnError, verbose, callbackLabel, callback);

            foreach ( ZephyrFile file in GetFiles() )
                file.Delete(null, stopOnError, verbose, callbackLabel, callback);
        }


    }
}
