using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Zephyr.Filesystem
{
    public abstract class ZephyrFile
    {
        public abstract String Name { get; }
        public abstract String FullName { get; set; }

        public ZephyrFile() { }

        public ZephyrFile(string fullName)
        {
            FullName = fullName;
        }

        public abstract ZephyrFile Create(string fileName = null, bool overwrite = true, String callbackLabel = null, Action<string, string> callback = null);
        public abstract void Delete(string fileName = null, bool stopOnError = true, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null);
        public abstract bool Exists(string fileName = null);

        public abstract ZephyrDirectory CreateDirectory(string dirName, String callbackLabel = null, Action<string, string> callback = null);

        public abstract Stream OpenStream(AccessType access, String callbackLabel = null, Action<string, string> callback = null);
        public abstract void CloseStream(String callbackLabel = null, Action<string, string> callback = null);

        public void CopyTo(ZephyrFile file, bool overwrite = true, bool stopOnError = true, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            try
            {
                if (file.Exists() && !overwrite)
                    throw new Exception($"File [{file.FullName}] Already Exists.");

                Stream source = this.OpenStream(AccessType.Read);
                Stream target = file.OpenStream(AccessType.Write);

                source.CopyTo(target);

                this.CloseStream();
                file.CloseStream();

                if (verbose)
                    Logger.Log($"Copied File [{this.FullName}] to [{file.FullName}].", callbackLabel, callback);
            }
            catch (Exception e)
            {
                Logger.Log($"ERROR - {e.Message}", callbackLabel, callback);
                if (stopOnError)
                    throw;
            }
        }

        public void MoveTo(ZephyrFile file, bool overwrite = true, bool stopOnError = true, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            try
            {
                if (file.Exists() && !overwrite)
                    throw new Exception($"File [{file.FullName}] Already Exists.");

                CopyTo(file, overwrite, false);
                this.Delete(stopOnError: stopOnError, verbose: false);
                if (verbose)
                    Logger.Log($"Moved File [{this.FullName}] to [{file.FullName}].", callbackLabel, callback);
            }
            catch (Exception e)
            {
                Logger.Log($"ERROR - {e.Message}", callbackLabel, callback);
                if (stopOnError)
                    throw;
            }
        }

        public void CopyTo(ZephyrDirectory dir, bool overwrite = true, bool stopOnError = true, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            String targetFilePath = dir.PathCombine(dir.FullName, this.Name);
            ZephyrFile targetFile = dir.CreateFile(targetFilePath);
            CopyTo(targetFile, overwrite, stopOnError, false, callbackLabel, callback);
            if (verbose)
                Logger.Log($"Copied File [{this.FullName}] to [{dir.FullName}].", callbackLabel, callback);
        }

        public void MoveTo(ZephyrDirectory dir, bool overwrite = true, bool stopOnError = true, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            CopyTo(dir, overwrite, stopOnError, false,callbackLabel, callback);
            this.Delete(stopOnError: stopOnError, verbose: false);
            if (verbose)
                Logger.Log($"Moved File [{this.FullName}] to [{dir.FullName}].", callbackLabel, callback);
        }
    }
}

