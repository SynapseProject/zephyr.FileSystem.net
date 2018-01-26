using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using NUnit.Framework;

using Zephyr.Filesystem;

namespace Zephyr.Filesystem.Tests
{
    [SetUpFixture]
    public class Global
    {
        // Environment Variables : Set the variables below to reflect your environment.
        public static String WindowsWorkspace = @"C:\Temp\";

        // Variables Used In All Test Cases
        public static String TestFilesPath = null;
        public static WindowsZephyrDirectory TestFilesDirectory = null;
        public static Clients Clients = new Clients();

        // Variables Used in Windows-Based Test Cases
        public static String WindowsWorkingPath = null;
        public static ZephyrDirectory WindowsWorkingDirectory = null;
        public static string RandomWindowsDirectory { get { return Path.GetRandomFileName().Replace(".", ""); } }
        public static string RandomWindowsFile { get { return Path.GetRandomFileName(); } }

        [OneTimeSetUp]
        public void Init()
        {
            WindowsWorkingPath = Path.Combine(WindowsWorkspace, $"temp_{Global.RandomWindowsDirectory}\\");

            // Get Path To The Project's "TestFiles" Folder
            String assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            UriBuilder uri = new UriBuilder(assemblyDir);
            string path = Uri.UnescapeDataString(uri.Path);
            DirectoryInfo dInfo = new DirectoryInfo(Path.GetDirectoryName(path));
            TestFilesPath = $"{dInfo.Parent.FullName}\\TestFiles\\";

            WindowsWorkingDirectory = Utilities.CreateDirectory(WindowsWorkingPath, Clients);
            TestFilesDirectory = new WindowsZephyrDirectory(Global.TestFilesPath);

        }

        [OneTimeTearDown]
        public void Teardown()
        {
            Utilities.Delete(WindowsWorkingPath, Clients);
        }

        public static string DirectoryObjectCounts(ZephyrDirectory dir)
        {
            List<ZephyrDirectory> dirs = (List<ZephyrDirectory>)dir.GetDirectories();
            List<ZephyrFile> files = (List<ZephyrFile>)dir.GetFiles();

            //TODO: Sort Directories To Ensure Counts Come Back In Same Order

            String counts = $"{dirs.Count},{files.Count}";

            foreach (ZephyrDirectory childDir in dirs)
                counts = $"{counts},{DirectoryObjectCounts(childDir)}";

            return counts;
        }

        public static ZephyrDirectory StageTestFilesToWindows()
        {
            String path = Path.Combine(WindowsWorkingPath, $"{RandomWindowsDirectory}\\");
            Console.WriteLine($"{path}");
            ZephyrDirectory dir = WindowsWorkingDirectory.CreateDirectory(path);
            dir.Create();
            TestFilesDirectory.CopyTo(dir, verbose: false);

            return dir;
        }
    }
}
