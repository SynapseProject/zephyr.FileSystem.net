using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.IO;
using System.Reflection;
using NUnit.Framework;
using Alphaleonis.Win32.Filesystem;

using Zephyr.Filesystem;

namespace Zephyr.Filesystem.Tests
{
    [TestFixture]
    public class Windows
    {
        // Environment Variables : Set the variables below to reflect your environment.
        String workspace = @"C:\Temp\";

        // Test Case Variables : These are set in the Setup() method
        String workingPath = null;
        String filesPath = null;
        Clients clients = new Clients();
        ZephyrDirectory workingDir = null;
        ZephyrDirectory filesDir = null;

        [OneTimeSetUp]
        public void Setup()
        {
            workingPath = Path.Combine(workspace, $"temp_{Global.RandomDirectory}\\");

            // Get Path To The  Project's "TestFiles" Folder
            String assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            UriBuilder uri = new UriBuilder(assemblyDir);
            string path = Uri.UnescapeDataString(uri.Path);
            DirectoryInfo dInfo = new DirectoryInfo(Path.GetDirectoryName(path));
            filesPath = $"{dInfo.Parent.FullName}\\TestFiles\\";

            // Get TestFiles Directory, Create Temporary Source and Target Directories
            filesDir = Utilities.GetZephyrDirectory(filesPath, clients);
            workingDir = Utilities.CreateDirectory(workingPath, clients);
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            Utilities.Delete(workingPath, clients);
        }

        [Test]
        public void WindowsDirectoryProperties()
        {
            String dirName = Global.RandomDirectory;
            String path = Path.Combine(workingPath, $"{dirName}\\");
            Console.WriteLine(path);
            ZephyrDirectory dir = new WindowsZephyrDirectory(path);
            dir.Create();

            Console.WriteLine($"FullName : {dir.FullName}");
            Assert.AreEqual(dir.FullName, path);

            Console.WriteLine($"Name     : {dir.Name}");
            Assert.AreEqual(dir.Name, dirName);

            Console.WriteLine($"Parent   : {dir.Parent}");
            Assert.AreEqual(dir.Parent, workingPath);

            Console.WriteLine($"Root     : {dir.Root}");
            Assert.AreEqual(dir.Root, Directory.GetDirectoryRoot(workingPath));

            Console.WriteLine($"Exists   : {dir.Exists}");
            Assert.That(dir.Exists);

            dir.Delete();
        }

        [Test]
        public void WindowsCreateDirectory()
        {
            String path = Path.Combine(workingPath, $"{Global.RandomDirectory}\\");
            Console.WriteLine(path);
            ZephyrDirectory dir = new WindowsZephyrDirectory(path);
            dir.Create();
            Assert.That(Utilities.Exists(path));
            dir.Delete();
        }

        [Test]
        public void WindowsDeleteDirectory()
        {
            String path = Path.Combine(workingPath, $"{Global.RandomDirectory}\\");
            Console.WriteLine(path);
            ZephyrDirectory dir = new WindowsZephyrDirectory(path);
            dir.Create();
            dir.Delete();
            Assert.That(!Utilities.Exists(path, clients));
        }

        [Test]
        public void WindowsCreateFileMethod()
        {
            String path = Path.Combine(workingPath, $"{Global.RandomFile}");
            Console.WriteLine(path);
            ZephyrFile file = workingDir.CreateFile(path);
            Assert.That(!file.Exists);
            file.Create();
            Assert.That(file.Exists);
            file.Delete();
        }

        [Test]
        public void WindowsCreateDirectoryMethod()
        {
            String path = Path.Combine(workingPath, $"{Global.RandomDirectory}\\");
            Console.WriteLine(path);
            ZephyrDirectory dir = workingDir.CreateDirectory(path);
            Assert.That(!dir.Exists);
            dir.Create();
            Assert.That(dir.Exists);
            dir.Delete();
        }

        [Test]
        public void WindowsGetDirectoriesandGetFiles()
        {
            String path = Path.Combine(workingPath, $"{Global.RandomDirectory}\\");
            Console.WriteLine(path);
            ZephyrDirectory dir = workingDir.CreateDirectory(path);
            dir.Create();
            filesDir.CopyTo(dir, verbose: false);

            List<ZephyrDirectory> dirs = (List<ZephyrDirectory>)(dir.GetDirectories());
            Console.WriteLine($"Found [{dirs.Count}] Sub-directories.");
            Assert.AreEqual(dirs.Count, 3);

            List<ZephyrFile> files = (List<ZephyrFile>)(dir.GetFiles());
            Console.WriteLine($"Found [{files.Count}] Files.");
            Assert.AreEqual(files.Count, 5);

            dir.Delete(verbose: false);
        }

        [Test]
        public void WindowsPathCombine()
        {
            String path = Path.Combine(workingPath, $"{Global.RandomDirectory}\\");
            Console.WriteLine(path);
            ZephyrDirectory dir = workingDir.CreateDirectory(path);

            string testpath = dir.PathCombine(dir.FullName, "michael\\", "j\\", "fox\\");
            Console.WriteLine($"Test Path : {testpath}");
            Assert.AreEqual(testpath, $"{dir.FullName}michael\\j\\fox\\");
        }

        //TODO : Implement Windows Version of CopyTo (different class perhaps)
        //TODO : Implement Windows Version of MoveTo (different class perhaps)
        //TODO : Implement Windows Version of IsEmpty
        //TODO : Implement Windows Version of Purge



    }
}
