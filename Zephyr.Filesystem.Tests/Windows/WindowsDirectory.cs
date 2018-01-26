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
    public class WindowsDirectory
    {
        [OneTimeSetUp]
        public void Setup()
        {
        }

        [OneTimeTearDown]
        public void Teardown()
        {
        }

        [Test]
        public void WindowsDirectoryProperties()
        {
            String dirName = Global.RandomWindowsDirectory;
            String path = Path.Combine(Global.WindowsWorkingPath, $"{dirName}\\");
            Console.WriteLine(path);
            ZephyrDirectory dir = new WindowsZephyrDirectory(path);
            dir.Create();

            Console.WriteLine($"FullName : {dir.FullName}");
            Assert.AreEqual(dir.FullName, path);

            Console.WriteLine($"Name     : {dir.Name}");
            Assert.AreEqual(dir.Name, dirName);

            Console.WriteLine($"Parent   : {dir.Parent}");
            Assert.AreEqual(dir.Parent, Global.WindowsWorkingPath);

            Console.WriteLine($"Root     : {dir.Root}");
            Assert.AreEqual(dir.Root, Directory.GetDirectoryRoot(Global.WindowsWorkingPath));

            Console.WriteLine($"Exists   : {dir.Exists}");
            Assert.That(dir.Exists);

            dir.Delete();
        }

        [Test]
        public void WindowsDirectoryCreate()
        {
            String path = Path.Combine(Global.WindowsWorkingPath, $"{Global.RandomWindowsDirectory}\\");
            Console.WriteLine(path);
            ZephyrDirectory dir = new WindowsZephyrDirectory(path);
            dir.Create();
            Assert.That(Utilities.Exists(path));
            dir.Delete();
        }

        [Test]
        public void WindowsDirectoryDelete()
        {
            String path = Path.Combine(Global.WindowsWorkingPath, $"{Global.RandomWindowsDirectory}\\");
            Console.WriteLine(path);
            ZephyrDirectory dir = new WindowsZephyrDirectory(path);
            dir.Create();
            dir.Delete();
            Assert.That(!Utilities.Exists(path, Global.Clients));
        }

        [Test]
        public void WindowsDirectoryCreateFileMethod()
        {
            String path = Path.Combine(Global.WindowsWorkingPath, $"{Global.RandomWindowsFile}");
            Console.WriteLine(path);
            ZephyrFile file = Global.WindowsWorkingDirectory.CreateFile(path);
            Assert.That(!file.Exists);
            file.Create();
            Assert.That(file.Exists);
            file.Delete();
        }

        [Test]
        public void WindowsDirectoryCreateDirectoryMethod()
        {
            String path = Path.Combine(Global.WindowsWorkingPath, $"{Global.RandomWindowsDirectory}\\");
            Console.WriteLine(path);
            ZephyrDirectory dir = Global.WindowsWorkingDirectory.CreateDirectory(path);
            Assert.That(!dir.Exists);
            dir.Create();
            Assert.That(dir.Exists);
            dir.Delete();
        }

        [Test]
        public void WindowsDirectoryGetDirectoriesandGetFiles()
        {
            ZephyrDirectory dir = Global.StageTestFilesToWindows();

            List<ZephyrDirectory> dirs = (List<ZephyrDirectory>)(dir.GetDirectories());
            Console.WriteLine($"Found [{dirs.Count}] Sub-directories.");
            Assert.AreEqual(dirs.Count, 3);

            List<ZephyrFile> files = (List<ZephyrFile>)(dir.GetFiles());
            Console.WriteLine($"Found [{files.Count}] Files.");
            Assert.AreEqual(files.Count, 5);

            dir.Delete(verbose: false);
        }

        [Test]
        public void WindowsDirectoryPathCombine()
        {
            String path = Path.Combine(Global.WindowsWorkingPath, $"{Global.RandomWindowsDirectory}\\");
            Console.WriteLine(path);
            ZephyrDirectory dir = Global.WindowsWorkingDirectory.CreateDirectory(path);

            string testpath = dir.PathCombine(dir.FullName, "michael\\", "j\\", "fox\\");
            Console.WriteLine($"Test Path : {testpath}");
            Assert.AreEqual(testpath, $"{dir.FullName}michael\\j\\fox\\");
        }

        [Test]
        public void WindowsDirectoryCopyToWindowsDirectory()
        {
            ZephyrDirectory source = Global.StageTestFilesToWindows();
            Console.WriteLine($"Source : {source.FullName}");

            String path = Path.Combine(Global.WindowsWorkingPath, $"{Global.RandomWindowsDirectory}\\");
            Console.WriteLine($"Target : {path}");
            ZephyrDirectory target = Global.WindowsWorkingDirectory.CreateDirectory(path);
            target.Create();

            source.CopyTo(target);

            String sourceCount = Global.DirectoryObjectCounts(source);
            Console.WriteLine($">> Source : [{sourceCount}]");
            String targetCount = Global.DirectoryObjectCounts(target);
            Console.WriteLine($">> Target : [{targetCount}]");

            Assert.AreEqual(sourceCount, targetCount);

            target.Delete();
            source.Delete();
        }


        [Test]
        public void WindowsDirectoryMoveToWindowsDirectory()
        {
            ZephyrDirectory source = Global.StageTestFilesToWindows();
            Console.WriteLine($"Source : {source.FullName}");

            String path = Path.Combine(Global.WindowsWorkingPath, $"{Global.RandomWindowsDirectory}\\");
            Console.WriteLine($"Target : {path}");
            ZephyrDirectory target = Global.WindowsWorkingDirectory.CreateDirectory(path);
            target.Create();

            String sourceCount = Global.DirectoryObjectCounts(source);
            Console.WriteLine($">> Source : [{sourceCount}]");

            source.MoveTo(target);

            String targetCount = Global.DirectoryObjectCounts(target);
            Console.WriteLine($">> Target : [{targetCount}]");

            Assert.AreEqual(sourceCount, targetCount);
            Assert.That(source.IsEmpty);

            target.Delete();
            source.Delete();
        }

        [Test]
        public void WindowsDirectoryIsEmpty()
        {
            String path = Path.Combine(Global.WindowsWorkingPath, $"{Global.RandomWindowsDirectory}\\");
            Console.WriteLine($"{path}");
            ZephyrDirectory dir = Global.WindowsWorkingDirectory.CreateDirectory(path);
            dir.Create();
            Assert.That(dir.IsEmpty());

            Global.TestFilesDirectory.CopyTo(dir, verbose: false);
            Assert.That(!dir.IsEmpty());

            dir.Delete();
        }


        [Test]
        public void WindowsDirectoryPurge()
        {
            ZephyrDirectory dir = Global.StageTestFilesToWindows();
            Assert.That(!dir.IsEmpty());

            dir.Purge();
            Assert.That(dir.Exists);
            Assert.That(dir.IsEmpty());

            dir.Delete();
        }


    }
}
