using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using NUnit.Framework;

using Alphaleonis.Win32.Filesystem;

namespace Zephyr.Filesystem.Tests
{
    [TestFixture]
    public class AwsS3Directory
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
        public void AwsS3DirectoryProperties()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            String dirName = Global.RandomDirectory;
            String path = $"{Global.AwsS3WorkingPath}{dirName}/";
            Console.WriteLine(path);
            ZephyrDirectory dir = new AwsS3ZephyrDirectory(Global.Clients.aws, path);
            dir.Create();

            Console.WriteLine($"FullName : {dir.FullName}");
            Assert.AreEqual(dir.FullName, path);

            Console.WriteLine($"Name     : {dir.Name}");
            Assert.AreEqual(dir.Name, dirName);

            Console.WriteLine($"Parent   : {dir.Parent}");
            Assert.AreEqual(dir.Parent, Global.AwsS3WorkingPath);

            Console.WriteLine($"Root     : {dir.Root}");
            Assert.AreEqual(dir.Root, Global.AwsS3WorkingDirectory.Root);

            Console.WriteLine($"Exists   : {dir.Exists}");
            Assert.That(dir.Exists);

            dir.Delete();
        }

        [Test]
        public void AwsS3DirectoryCreate()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            String path = $"{Global.AwsS3WorkingPath}{Global.RandomDirectory}/";
            Console.WriteLine(path);
            ZephyrDirectory dir = new AwsS3ZephyrDirectory(Global.Clients.aws, path);
            dir.Create();
            Assert.IsTrue(Utilities.Exists(path, Global.Clients));
            dir.Delete();
        }

        [Test]
        public void AwsS3DirectoryDelete()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            String path = $"{Global.AwsS3WorkingPath}{Global.RandomDirectory}/";
            Console.WriteLine(path);
            ZephyrDirectory dir = new AwsS3ZephyrDirectory(Global.Clients.aws, path);
            dir.Create();
            Assert.IsTrue(Utilities.Exists(path, Global.Clients));
            dir.Delete();
            Assert.IsFalse(Utilities.Exists(path, Global.Clients));
        }

        [Test]
        public void AwsS3DirectoryCreateFileMethod()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            String path = $"{Global.AwsS3WorkingPath}{Global.RandomFile}";
            Console.WriteLine(path);
            ZephyrFile file = Global.AwsS3WorkingDirectory.CreateFile(path);
            Assert.IsFalse(file.Exists);
            file.Create();
            Assert.IsTrue(file.Exists);
            file.Delete();
        }

        [Test]
        public void AwsS3DirectoryCreateDirectoryMethod()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            String path = $"{Global.AwsS3WorkingPath}{Global.RandomDirectory}/";
            Console.WriteLine(path);
            ZephyrDirectory dir = Global.AwsS3WorkingDirectory.CreateDirectory(path);
            Assert.IsFalse(dir.Exists);
            dir.Create();
            Assert.IsTrue(dir.Exists);
            dir.Delete();
        }

        [Test]
        public void AwsS3DirectoryGetDirectoriesandGetFiles()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            ZephyrDirectory dir = Global.StageTestFilesToAws();

            List<ZephyrDirectory> dirs = (List<ZephyrDirectory>)(dir.GetDirectories());
            Console.WriteLine($"Found [{dirs.Count}] Sub-directories.");
            Assert.AreEqual(dirs.Count, 3);

            List<ZephyrFile> files = (List<ZephyrFile>)(dir.GetFiles());
            Console.WriteLine($"Found [{files.Count}] Files.");
            Assert.AreEqual(files.Count, 5);

            dir.Delete(verbose: false);
        }

        [Test]
        public void AwsS3DirectoryPathCombine()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            String path = $"{Global.AwsS3WorkingPath}{Global.RandomDirectory}/";
            Console.WriteLine(path);
            ZephyrDirectory dir = Global.AwsS3WorkingDirectory.CreateDirectory(path);

            string testpath = dir.PathCombine(dir.FullName, "michael/", "j/", "fox/");
            Console.WriteLine($"Test Path : {testpath}");
            Assert.AreEqual(testpath, $"{dir.FullName}michael/j/fox/");
        }

        [Test]
        public void AwsS3DirectoryCopyToAwsS3Directory()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            ZephyrDirectory source = Global.StageTestFilesToAws();
            Console.WriteLine($"Source : {source.FullName}");

            String path = $"{Global.AwsS3WorkingPath}{Global.RandomDirectory}/";
            Console.WriteLine($"Target : {path}");
            ZephyrDirectory target = Global.AwsS3WorkingDirectory.CreateDirectory(path);
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
        public void AwsS3DirectoryMoveToWindowsDirectory()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            ZephyrDirectory source = Global.StageTestFilesToAws();
            Console.WriteLine($"Source : {source.FullName}");

            String path = $"{Global.AwsS3WorkingPath}{Global.RandomDirectory}/";
            Console.WriteLine($"Target : {path}");
            ZephyrDirectory target = Global.AwsS3WorkingDirectory.CreateDirectory(path);
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
        public void AwsS3DirectoryIsEmpty()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            String path = $"{Global.AwsS3WorkingPath}{Global.RandomDirectory}/";
            Console.WriteLine($"{path}");
            ZephyrDirectory dir = Global.AwsS3WorkingDirectory.CreateDirectory(path);
            dir.Create();
            Assert.IsTrue(dir.IsEmpty);

            Global.TestFilesDirectory.CopyTo(dir, verbose: false);
            Assert.IsFalse(dir.IsEmpty);

            dir.Delete();
        }

        [Test]
        public void AwsS3DirectoryPurge()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            ZephyrDirectory dir = Global.StageTestFilesToAws();
            Assert.IsFalse(dir.IsEmpty);

            dir.Purge();
            Assert.IsTrue(dir.Exists);
            Assert.IsTrue(dir.IsEmpty);

            dir.Delete();
        }
    }
}
