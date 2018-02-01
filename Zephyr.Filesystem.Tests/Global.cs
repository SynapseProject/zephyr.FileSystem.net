using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using NUnit.Framework;

using Amazon;
using Amazon.S3;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;

using Zephyr.Filesystem;

namespace Zephyr.Filesystem.Tests
{
    [SetUpFixture]
    public class Global
    {
        // TODO : Set Working Directories.  Directories MUST end in a slash ('/' or '\').
        public static bool TestWindows = true;
        public static String WindowsWorkspace = @"C:\Temp\";
        public static String AwsS3Workspace = @"s3://mybucket/UnitTests/";

        // TODO : Set Amazon S3 Connection Variables.
        public static bool TestAws = false;
        public static RegionEndpoint AwsS3Region = RegionEndpoint.USEast1;
        public static string AwsS3AccessKey = null;
        public static string AwsS3SecretKey = null;
        public static string AwsS3SessionKey = null;       

        // Variables Used In All Test Cases
        public static String TestFilesPath = null;
        public static WindowsZephyrDirectory TestFilesDirectory = null;
        public static Clients Clients = new Clients();
        public static string RandomDirectory { get { return Path.GetRandomFileName().Replace(".", ""); } }
        public static string RandomFile { get { return Path.GetRandomFileName(); } }

        // Variables Used in Windows-Based Test Cases
        public static String WindowsWorkingPath = null;
        public static ZephyrDirectory WindowsWorkingDirectory = null;

        // Variables Used in AwsS3-Based Test Cases
        public static String AwsS3WorkingPath = null;
        public static ZephyrDirectory AwsS3WorkingDirectory = null;

        [OneTimeSetUp]
        public void Init()
        {
            if (TestWindows)
            {
                WindowsWorkingPath = Path.Combine(WindowsWorkspace, $"temp_{Global.RandomDirectory}\\");
                WindowsWorkingDirectory = Utilities.CreateDirectory(WindowsWorkingPath, Clients);
            }

            if (TestAws)
            {
                Clients.aws = Utilities.InitAwsClient(AwsS3Region, AwsS3AccessKey, AwsS3SecretKey);
                AwsS3WorkingPath = $"{AwsS3Workspace}{Global.RandomDirectory}/";
                AwsS3WorkingDirectory = Utilities.CreateDirectory(AwsS3WorkingPath, Clients);
            }

            // Get Path To The Project's "TestFiles" Folder
            String assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            UriBuilder uri = new UriBuilder(assemblyDir);
            string path = Uri.UnescapeDataString(uri.Path);
            DirectoryInfo dInfo = new DirectoryInfo(Path.GetDirectoryName(path));
            TestFilesPath = $"{dInfo.Parent.FullName}\\TestFiles\\";
            TestFilesDirectory = new WindowsZephyrDirectory(Global.TestFilesPath);

        }

        [OneTimeTearDown]
        public void Teardown()
        {
            if (TestWindows)
                Utilities.Delete(WindowsWorkingPath, Clients);

            if (TestAws)
                Utilities.Delete(AwsS3WorkingPath, Clients);
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
            String path = Path.Combine(WindowsWorkingPath, $"{RandomDirectory}\\");
            Console.WriteLine($"{path}");
            ZephyrDirectory dir = WindowsWorkingDirectory.CreateDirectory(path);
            dir.Create();
            TestFilesDirectory.CopyTo(dir, verbose: false);

            return dir;
        }

        public static ZephyrDirectory StageTestFilesToAws()
        {
            String path = $"{AwsS3WorkingPath}{RandomDirectory}/";
            Console.WriteLine($"{path}");
            ZephyrDirectory dir = AwsS3WorkingDirectory.CreateDirectory(path);
            dir.Create();
            TestFilesDirectory.CopyTo(dir, verbose: false);

            return dir;
        }
    }
}
