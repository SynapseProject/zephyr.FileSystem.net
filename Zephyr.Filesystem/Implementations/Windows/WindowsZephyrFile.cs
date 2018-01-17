using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alphaleonis.Win32.Filesystem;

namespace Zephyr.Filesystem
{
    public class WindowsZephyrFile : ZephyrFile
    {
        private System.IO.FileStream fileStream;
        private bool isStreamOpen = false;

        public override string Name
        {
            get
            {
                return Path.GetFileName( this.FullName );
            }
        }

        public override string FullName { get; set; }

        public WindowsZephyrFile() : base() { }
        public WindowsZephyrFile(string fullName) : base( fullName ) { }

        public override System.IO.Stream OpenStream(AccessType access, String callbackLabel = null, Action<string, string> callback = null)
        {
            if ( !isStreamOpen )
            {
                fileStream = File.Open( FullName, System.IO.FileMode.OpenOrCreate, access == AccessType.Read ? System.IO.FileAccess.Read : System.IO.FileAccess.Write );
                isStreamOpen = true;
                callback?.Invoke( callbackLabel, $"File Stream [{FullName}] Has Been Opened." );
            }
            else
                callback?.Invoke( callbackLabel, $"File Stream [{FullName}] Is Already Open." );
            return fileStream;
        }

        public override void CloseStream(String callbackLabel = null, Action<string, string> callback = null)
        {
            if ( isStreamOpen )
            {
                fileStream.Close();
                isStreamOpen = false;
                callback?.Invoke( callbackLabel, $"File Stream [{FullName}] Has Been Closed." );
            }
            else
                callback?.Invoke( callbackLabel, $"File Stream [{FullName}] Is Already Closed." );

        }

        public override ZephyrFile Create(string fileName = null, bool overwrite = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            if ( fileName == null || fileName == FullName)
            {
                try
                {
                    if (this.Exists() && !overwrite)
                        throw new Exception($"File [{this.FullName}] Already Exists.");

                    fileStream = File.Open(FullName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
                    isStreamOpen = true;    // Opens Stream as Write By Default
                    callback?.Invoke(callbackLabel, $"File [{FullName}] Was Created.");
                    return this;
                }
                catch (Exception e)
                {
                    Logger.Log($"ERROR - {e.Message}", callbackLabel, callback);
                    throw;
                }
            }
            else
            {
                WindowsZephyrFile synFile = new WindowsZephyrFile( fileName );
                synFile.Create( null, overwrite, callbackLabel, callback );
                return synFile;
            }
        }

        public override ZephyrDirectory CreateDirectory(string dirName, String callbackLabel = null, Action<string, string> callback = null)
        {
            return new WindowsZephyrDirectory(dirName);
        }

        public override void Delete(string fileName = null, bool stopOnError = true, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            if ( fileName == null || fileName == FullName )
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(FullName);
                    if (fileInfo.Exists)
                        fileInfo.Delete();

                    if (verbose)
                        Logger.Log($"File [{FullName}] Was Deleted.", callbackLabel, callback);
                }
                catch (Exception e)
                {
                    Logger.Log(e.Message, callbackLabel, callback);
                    if (stopOnError)
                        throw;
                }
            }
            else
            {
                File.Delete( fileName );
                if (verbose)
                    Logger.Log($"File [{fileName}] Was Deleted.", callbackLabel, callback);
            }
        }

        public override bool Exists(string fileName = null)
        {
            if ( fileName == null )
                return File.Exists( FullName );
            else
                return File.Exists( fileName );
        }

    }
}
