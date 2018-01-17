using System;
using System.Collections.Generic;
using Alphaleonis.Win32.Filesystem;

namespace Zephyr.Filesystem
{
    public class WindowsZephyrDirectory : ZephyrDirectory
    {
        private DirectoryInfo dirInfo = null;

        public WindowsZephyrDirectory() { }
        public WindowsZephyrDirectory(string fullPath)
        {
            FullName = fullPath;
        }

        public override String FullName
        {
            get { return dirInfo.FullName; }
            set { dirInfo = new DirectoryInfo( value ); }
        }
        public override String Name { get { return dirInfo?.Name; } }
        public override String Parent { get { return dirInfo?.Parent?.FullName; } }
        public override String Root { get { return dirInfo?.Root?.FullName; } }

        public override ZephyrDirectory Create(string childDirName = null, bool failIfExists = false, String callbackLabel = null, Action<string, string> callback = null)
        {
            if (childDirName == null || childDirName == FullName)
            {
                if (!Directory.Exists(FullName))
                    Directory.CreateDirectory(FullName);
                else if (failIfExists)
                    throw new Exception($"Directory [{FullName}] Already Exists.");
                callback?.Invoke( callbackLabel, $"Directory [{FullName}] Was Created." );
                return this;
            }
            else
            {
                String childDirNameString = PathCombine( FullName, childDirName );
                WindowsZephyrDirectory synDir = new WindowsZephyrDirectory( childDirNameString );
                synDir.Create(null, failIfExists, callbackLabel, callback);
                return synDir;
            }
        }

        public override ZephyrFile CreateFile(string fullName, String callbackLabel = null, Action<string, string> callback = null)
        {
            return new WindowsZephyrFile(fullName);
        }

        public override void Delete(string dirName = null, bool recurse = true, bool stopOnError = true, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            if ( dirName == null || dirName == FullName)
            {
                try
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(FullName);

                    if (dirInfo.Exists)
                    {
                        if (!recurse)
                        {
                            int dirs = dirInfo.GetDirectories().Length;
                            int files = dirInfo.GetFiles().Length;
                            if (dirs > 0 || files > 0)
                                throw new Exception($"Directory [{FullName}] is not empty.");
                        }
                        dirInfo.Delete(recurse);
                    }

                    if (verbose)
                        Logger.Log($"Directory [{FullName}] Was Deleted.", callbackLabel, callback);
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
                WindowsZephyrDirectory dir = new WindowsZephyrDirectory(dirName);
                dir.Delete(null, recurse, stopOnError, verbose, callbackLabel, callback);
            }

            dirInfo = null;
        }

        public override bool Exists(string dirName = null)
        {
            if ( dirName == null )
                return Directory.Exists( FullName );
            else
                return Directory.Exists( dirName );
        }

        public override IEnumerable<ZephyrDirectory> GetDirectories()
        {
            String[] directories = Directory.GetDirectories( FullName );

            List<ZephyrDirectory> synDirs = new List<ZephyrDirectory>();
            foreach (string dir in directories)
            {
                ZephyrDirectory synDir = new WindowsZephyrDirectory( Path.Combine( FullName, dir ) );
                synDirs.Add( synDir );
            }

            return synDirs;
        }

        public override IEnumerable<ZephyrFile> GetFiles()
        {
            String[] files = Directory.GetFiles( FullName );
            List<ZephyrFile> synFiles = new List<ZephyrFile>();
            foreach (string file in files)
            {
                ZephyrFile synFile = new WindowsZephyrFile( Path.Combine( FullName, file ) );
                synFiles.Add( synFile );
            }

            return synFiles;
        }

        public override string PathCombine(params string[] paths)
        {
            return Path.Combine( paths );
        }
    }
}
