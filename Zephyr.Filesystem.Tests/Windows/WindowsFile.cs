using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alphaleonis.Win32.Filesystem;


using NUnit.Framework;

namespace Zephyr.Filesystem.Tests
{
    [TestFixture]
    public class WindowsFile
    {
        [Test]
        public void WindowsFileProperties()
        {
            String fileName = Global.RandomWindowsFile;
            String path = Path.Combine(Global.WindowsWorkingPath, $"{fileName}");
            Console.WriteLine(path);
            ZephyrFile file = new WindowsZephyrFile(path);
            file.Create();

            Console.WriteLine($"FullName : {file.FullName}");
            Assert.AreEqual(file.FullName, path);

            Console.WriteLine($"Name     : {file.Name}");
            Assert.AreEqual(file.Name, fileName);

            Console.WriteLine($"Exists   : {file.Exists}");
            Assert.That(file.Exists);

            Console.WriteLine($"Stream   : {(file.Stream.CanWrite ? "Writable" : "Not Writable")}");
            Assert.That(file.Stream.CanWrite);

            Console.WriteLine($"IsOpen   : {file.IsOpen}");
            Assert.That(file.IsOpen);

            Console.WriteLine($"CanRead  : {file.CanRead}");
            Assert.That(!file.CanRead);

            Console.WriteLine($"CanWrite : {file.CanWrite}");
            Assert.That(file.CanWrite);

            file.Delete();
        }

        [Test]
        public void WindowsFileCreate()
        {
            String path = Path.Combine(Global.WindowsWorkingPath, $"{Global.RandomWindowsFile}");
            Console.WriteLine(path);
            ZephyrFile file = new WindowsZephyrFile(path);
            file.Create();
            Assert.That(Utilities.Exists(path));
            file.Delete();
        }

        [Test]
        public void WindowsFileDelete()
        {
            String path = Path.Combine(Global.WindowsWorkingPath, $"{Global.RandomWindowsFile}");
            Console.WriteLine(path);
            ZephyrFile file = new WindowsZephyrFile(path);
            file.Create();
            file.Delete();
            Assert.That(!Utilities.Exists(path, Global.Clients));
        }

        [Test]
        public void WindowsFileCreateFileMethod()
        {
            String path = Path.Combine(Global.WindowsWorkingPath, $"{Global.RandomWindowsFile}");
            Console.WriteLine(path);
            ZephyrFile file = new WindowsZephyrFile(path);

            ZephyrFile newFile = Global.WindowsWorkingDirectory.CreateFile(path);
            Assert.That(!newFile.Exists);
            newFile.Create();
            Assert.That(newFile.Exists);
            newFile.Delete();
            file.Delete();
        }

        [Test]
        public void WindowsFileCreateDirectoryMethod()
        {
            String path = Path.Combine(Global.WindowsWorkingPath, $"{Global.RandomWindowsFile}");
            Console.WriteLine(path);
            ZephyrFile file = new WindowsZephyrFile(path);

            ZephyrDirectory dir = Global.WindowsWorkingDirectory.CreateDirectory(path);
            Assert.That(!dir.Exists);
            dir.Create();
            Assert.That(dir.Exists);
            dir.Delete();
            file.Delete();
        }

        [Test]
        public void WindowsFileOpen()
        {
            String path = Path.Combine(Global.WindowsWorkingPath, $"{Global.RandomWindowsFile}");
            Console.WriteLine(path);
            ZephyrFile file = new WindowsZephyrFile(path);

            System.IO.Stream stream = file.Open(AccessType.Write);
            Assert.That(stream.CanWrite);

            file.Close();
            file.Delete();
        }

        [Test]
        public void WindowsFileClose()
        {
            String path = Path.Combine(Global.WindowsWorkingPath, Global.RandomWindowsFile);
            Console.WriteLine(path);
            ZephyrFile file = new WindowsZephyrFile(path);

            System.IO.Stream stream = file.Open(AccessType.Write);
            file.Close();
            Assert.That(!stream.CanWrite);
            file.Delete();
        }

        [Test]
        public void WindowsFileCopyToWindowsFile()
        {
            ZephyrDirectory source = Global.StageTestFilesToWindows();

            List<ZephyrFile> files = (List<ZephyrFile>)source.GetFiles();
            Assert.IsNotEmpty(files);
            foreach (ZephyrFile file in files)
            {
                String path = Path.Combine(Global.WindowsWorkingPath, Global.RandomWindowsFile);
                ZephyrFile dest = new WindowsZephyrFile(path);
                file.CopyTo(dest);
                Assert.That(File.Exists(path));
                dest.Delete();
            }

            source.Delete();
        }

        [Test]
        public void WindowsFileCopyToWindowsDirectory()
        {
            ZephyrDirectory source = Global.StageTestFilesToWindows();
            string path = Path.Combine(Global.WindowsWorkingPath, $"{Global.RandomWindowsDirectory}\\");
            ZephyrDirectory target = new WindowsZephyrDirectory(path);

            List<ZephyrFile> files = (List<ZephyrFile>)source.GetFiles();
            Assert.IsNotEmpty(files);
            foreach (ZephyrFile file in files)
            {
                String filePath = Path.Combine(target.FullName, file.Name);
                file.CopyTo(target);
                Assert.That(File.Exists(filePath));
                Utilities.Delete(filePath, Global.Clients);
            }

            source.Delete();
        }

        [Test]
        public void WindowsFileMoveToWindowsFile()
        {
            ZephyrDirectory source = Global.StageTestFilesToWindows();

            List<ZephyrFile> files = (List<ZephyrFile>)source.GetFiles();
            Assert.IsNotEmpty(files);
            foreach (ZephyrFile file in files)
            {
                String path = Path.Combine(Global.WindowsWorkingPath, Global.RandomWindowsFile);
                ZephyrFile dest = new WindowsZephyrFile(path);
                file.MoveTo(dest);
                Assert.That(File.Exists(path));
                Assert.That(!file.Exists);
                dest.Delete();
            }

            source.Delete();
        }

        [Test]
        public void WindowsFileMoveToWindowsDirectory()
        {
            ZephyrDirectory source = Global.StageTestFilesToWindows();
            string path = Path.Combine(Global.WindowsWorkingPath, $"{Global.RandomWindowsDirectory}\\");
            ZephyrDirectory target = new WindowsZephyrDirectory(path);

            List<ZephyrFile> files = (List<ZephyrFile>)source.GetFiles();
            Assert.IsNotEmpty(files);
            foreach (ZephyrFile file in files)
            {
                String filePath = Path.Combine(target.FullName, file.Name);
                file.MoveTo(target);
                Assert.That(File.Exists(filePath));
                Assert.That(!file.Exists);
                Utilities.Delete(filePath, Global.Clients);
            }

            source.Delete();
        }

        [Test]
        public void WindowsFileReopen()
        {
            String path = Path.Combine(Global.WindowsWorkingPath, $"{Global.RandomWindowsFile}");
            Console.WriteLine(path);
            ZephyrFile file = new WindowsZephyrFile(path);

            System.IO.Stream stream = file.Open(AccessType.Read);
            Assert.That(stream.CanRead);
            Assert.That(!stream.CanWrite);

            stream = file.Reopen(AccessType.Write);
            Assert.That(!stream.CanRead);
            Assert.That(stream.CanWrite);

            file.Close();
            file.Delete();
        }

        [Test]
        public void WindowsFileReadAllLines()
        {
            ZephyrDirectory source = Global.StageTestFilesToWindows();
            List<ZephyrFile> files = (List<ZephyrFile>)source.GetFiles();
            Assert.IsNotEmpty(files);
            foreach (ZephyrFile file in files)
            {
                Console.WriteLine($"File: {file.FullName}");
                String[] lines = file.ReadAllLines();
                foreach (string line in lines)
                    Console.WriteLine(line);
                Assert.IsNotEmpty(lines);
            }

            source.Delete();
        }

        [Test]
        public void WindowsFileReadAllText()
        {
            ZephyrDirectory source = Global.StageTestFilesToWindows();
            List<ZephyrFile> files = (List<ZephyrFile>)source.GetFiles();
            Assert.IsNotEmpty(files);
            foreach (ZephyrFile file in files)
            {
                Console.WriteLine($"File: {file.FullName}");
                String content = file.ReadAllText();
                Console.WriteLine(content);
                Assert.IsNotEmpty(content);
            }

            source.Delete();
        }

        [Test]
        public void WindowsFileReadAllBytes()
        {
            ZephyrDirectory source = Global.StageTestFilesToWindows();
            List<ZephyrFile> files = (List<ZephyrFile>)source.GetFiles();
            Assert.IsNotEmpty(files);
            foreach (ZephyrFile file in files)
            {
                Console.WriteLine($"File: {file.FullName}");
                byte[] content = file.ReadAllBytes();
                foreach (byte b in content)
                    Console.Write(b);
                Assert.IsNotEmpty(content);
            }

            source.Delete();
        }

        [Test]
        public void WindowsFileWriteAllLines()
        {
            ZephyrDirectory source = Global.StageTestFilesToWindows();
            List<ZephyrFile> files = (List<ZephyrFile>)source.GetFiles();
            Assert.IsNotEmpty(files);
            foreach (ZephyrFile file in files)
            {
                Console.WriteLine($"File: {file.FullName}");
                string[] lines = file.ReadAllLines();
                string outPath = Path.Combine(source.FullName, Global.RandomWindowsFile);
                ZephyrFile outFile = source.CreateFile(outPath);
                outFile.WriteAllLines(lines);
                string[] outLines = outFile.ReadAllLines();
                Assert.AreEqual(lines.Length, outLines.Length);
                for (int i = 0; i < lines.Length; i++)
                    Assert.AreEqual(lines[i], outLines[i]);

                outFile.Delete();
            }

            source.Delete();
        }

        [Test]
        public void WindowsFileWriteAllText()
        {
            ZephyrDirectory source = Global.StageTestFilesToWindows();
            List<ZephyrFile> files = (List<ZephyrFile>)source.GetFiles();
            Assert.IsNotEmpty(files);
            foreach (ZephyrFile file in files)
            {
                Console.WriteLine($"File: {file.FullName}");
                string content = file.ReadAllText();
                string outPath = Path.Combine(source.FullName, Global.RandomWindowsFile);
                ZephyrFile outFile = source.CreateFile(outPath);
                outFile.WriteAllText(content);
                string outText = outFile.ReadAllText();
                Assert.AreEqual(content, outText);

                outFile.Delete();
            }

            source.Delete();
        }

        [Test]
        public void WindowsFileWriteAllBytes()
        {
            ZephyrDirectory source = Global.StageTestFilesToWindows();
            List<ZephyrFile> files = (List<ZephyrFile>)source.GetFiles();
            Assert.IsNotEmpty(files);
            foreach (ZephyrFile file in files)
            {
                Console.WriteLine($"File: {file.FullName}");
                byte[] bytes = file.ReadAllBytes();
                string outPath = Path.Combine(source.FullName, Global.RandomWindowsFile);
                ZephyrFile outFile = source.CreateFile(outPath);
                outFile.WriteAllBytes(bytes);
                byte[] outBytes = outFile.ReadAllBytes();
                Assert.AreEqual(bytes.Length, outBytes.Length);
                for (int i = 0; i < bytes.Length; i++)
                    Assert.AreEqual(bytes[i], outBytes[i]);

                outFile.Delete();
            }

            source.Delete();
        }


    }
}
