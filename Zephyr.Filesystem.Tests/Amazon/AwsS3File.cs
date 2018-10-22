using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace Zephyr.Filesystem.Tests
{
    [TestFixture]
    public class AwsS3File
    {
        [Test]
        public void AwsS3FileProperties()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            String fileName = Global.RandomFile;
            String path = $"{Global.AwsS3WorkingPath}/{fileName}";
            Console.WriteLine(path);
            ZephyrFile file = new AwsS3ZephyrFile(Global.Clients.aws, path);
            file.Create();
            file.Close();
            file.Open(AccessType.Write);

            Console.WriteLine($"FullName : {file.FullName}");
            Assert.AreEqual(file.FullName, path);

            Console.WriteLine($"Name     : {file.Name}");
            Assert.AreEqual(file.Name, fileName);

            Console.WriteLine($"Exists   : {file.Exists}");
            Assert.That(file.Exists);

            Console.WriteLine($"IsOpen   : {file.IsOpen}");
            Assert.That(file.IsOpen);

            Console.WriteLine($"CanRead  : {file.CanRead}");
            Assert.That(file.CanRead);

            Console.WriteLine($"CanWrite : {file.CanWrite}");
            Assert.That(file.CanWrite);

            file.Close();
            file.Delete();
        }

        [Test]
        public void AwsS3FileCreate()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            String path = $"{Global.AwsS3WorkingPath}{Global.RandomFile}";
            Console.WriteLine(path);
            ZephyrFile file = new AwsS3ZephyrFile(Global.Clients.aws, path);
            file.Create();
            file.Close();
            Assert.That(Utilities.Exists(path, Global.Clients));
            file.Delete();
        }

        [Test]
        public void AwsS3FileDelete()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            String path = $"{Global.AwsS3WorkingPath}{Global.RandomFile}";
            Console.WriteLine(path);
            ZephyrFile file = new AwsS3ZephyrFile(Global.Clients.aws, path);
            file.Create();
            file.Close();
            file.Delete();
            Assert.That(!Utilities.Exists(path, Global.Clients));
        }

        [Test]
        public void AwsS3FileCreateFileMethod()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            String path = $"{Global.AwsS3WorkingPath}{Global.RandomFile}";
            Console.WriteLine(path);
            ZephyrFile file = new AwsS3ZephyrFile(Global.Clients.aws, path);

            ZephyrFile newFile = Global.AwsS3WorkingDirectory.CreateFile(path);
            Assert.That(!newFile.Exists);
            newFile.Create();
            newFile.Close();
            Assert.That(newFile.Exists);
            newFile.Delete();
            file.Delete();
        }

        [Test]
        public void AwsS3FileCreateDirectoryMethod()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            String path = $"{Global.AwsS3WorkingPath}{Global.RandomFile}";
            Console.WriteLine(path);
            ZephyrFile file = new AwsS3ZephyrFile(Global.Clients.aws, path);

            ZephyrDirectory dir = Global.AwsS3WorkingDirectory.CreateDirectory(path);
            Assert.That(!dir.Exists);
            dir.Create();
            Assert.That(dir.Exists);
            dir.Delete();
            file.Delete();
        }

        [Test]
        public void AwsS3FileOpen()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            String path = $"{Global.AwsS3WorkingPath}{Global.RandomFile}";
            Console.WriteLine(path);
            ZephyrFile file = new AwsS3ZephyrFile(Global.Clients.aws, path);

            System.IO.Stream stream = file.Open(AccessType.Write);
            Assert.That(stream.CanWrite);

            file.Close();
            file.Delete();
        }

        [Test]
        public void AwsS3FileClose()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            String path = $"{Global.AwsS3WorkingPath}{Global.RandomFile}";
            Console.WriteLine(path);
            ZephyrFile file = new AwsS3ZephyrFile(Global.Clients.aws, path);

            file.Open(AccessType.Write);
            Assert.That(file.Stream.CanWrite);
            file.Close();
            Assert.IsNull(file.Stream);
            file.Delete();
        }

        [Test]
        public void AwsS3FileCopyToAwsS3File()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            ZephyrDirectory source = Global.StageTestFilesToAws();

            List<ZephyrFile> files = (List<ZephyrFile>)source.GetFiles();
            Assert.IsNotEmpty(files);
            foreach (ZephyrFile file in files)
            {
                String path = $"{Global.AwsS3WorkingPath}{Global.RandomFile}";
                ZephyrFile dest = new AwsS3ZephyrFile(Global.Clients.aws, path);
                file.CopyTo(dest);
                Assert.That(Utilities.Exists(path, Global.Clients));
                dest.Delete();
            }

            source.Delete();
        }

        [Test]
        public void AwsS3FileCopyToAwsS3Directory()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            ZephyrDirectory source = Global.StageTestFilesToAws();
            string path = $"{Global.AwsS3WorkingPath}{Global.RandomDirectory}/";
            ZephyrDirectory target = new AwsS3ZephyrDirectory(Global.Clients.aws, path);

            List<ZephyrFile> files = (List<ZephyrFile>)source.GetFiles();
            Assert.IsNotEmpty(files);
            foreach (ZephyrFile file in files)
            {
                String filePath = $"{target.FullName}{file.Name}";
                file.CopyTo(target);
                Assert.That(Utilities.Exists(filePath, Global.Clients));
                Utilities.Delete(filePath, Global.Clients);
            }

            source.Delete();
        }

        [Test]
        public void AwsS3FileMoveToAwsS3File()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            ZephyrDirectory source = Global.StageTestFilesToAws();

            List<ZephyrFile> files = (List<ZephyrFile>)source.GetFiles();
            Assert.IsNotEmpty(files);
            foreach (ZephyrFile file in files)
            {
                String path = $"{Global.AwsS3WorkingPath}{Global.RandomFile}";
                ZephyrFile dest = new AwsS3ZephyrFile(Global.Clients.aws, path);
                file.MoveTo(dest);
                Assert.That(Utilities.Exists(path, Global.Clients));
                Assert.That(!file.Exists);
                dest.Delete();
            }

            source.Delete();
        }

        [Test]
        public void AwsS3FileMoveToAwsS3Directory()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            ZephyrDirectory source = Global.StageTestFilesToAws();
            string path = $"{Global.AwsS3WorkingPath}{Global.RandomDirectory}/";
            ZephyrDirectory target = new AwsS3ZephyrDirectory(Global.Clients.aws, path);

            List<ZephyrFile> files = (List<ZephyrFile>)source.GetFiles();
            Assert.IsNotEmpty(files);
            foreach (ZephyrFile file in files)
            {
                String filePath = $"{target.FullName}{file.Name}";
                file.MoveTo(target);
                Assert.That(Utilities.Exists(filePath, Global.Clients));
                Assert.That(!file.Exists);
                Utilities.Delete(filePath, Global.Clients);
            }

            source.Delete();
        }

        [Test]
        public void AwsS3FileReopen()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            String path = $"{Global.AwsS3WorkingPath}{Global.RandomFile}";
            Console.WriteLine(path);
            ZephyrFile file = new AwsS3ZephyrFile(Global.Clients.aws, path);

            System.IO.Stream stream = file.Open(AccessType.Read);
            Assert.IsTrue(stream.CanRead);
            Assert.IsTrue(stream.CanWrite);

            stream = file.Reopen(AccessType.Write);
            Assert.IsTrue(stream.CanRead);
            Assert.IsTrue(stream.CanWrite);

            file.Close();
            file.Delete();
        }

        [Test]
        public void AwsS3FileReadAllLines()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            ZephyrDirectory source = Global.StageTestFilesToAws();
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
        public void AwsS3FileReadAllText()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            ZephyrDirectory source = Global.StageTestFilesToAws();
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
        public void AwsS3FileReadAllBytes()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            ZephyrDirectory source = Global.StageTestFilesToAws();
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
        public void AwsS3FileWriteAllLines()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            ZephyrDirectory source = Global.StageTestFilesToAws();
            List<ZephyrFile> files = (List<ZephyrFile>)source.GetFiles();
            Assert.IsNotEmpty(files);
            foreach (ZephyrFile file in files)
            {
                Console.WriteLine($"File: {file.FullName}");
                string[] lines = file.ReadAllLines();
                string outPath = $"{source.FullName}{Global.RandomFile}";
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
        public void AwsS3FileWriteAllText()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            ZephyrDirectory source = Global.StageTestFilesToAws();
            List<ZephyrFile> files = (List<ZephyrFile>)source.GetFiles();
            Assert.IsNotEmpty(files);
            foreach (ZephyrFile file in files)
            {
                Console.WriteLine($"File: {file.FullName}");
                string content = file.ReadAllText();
                string outPath = $"{source.FullName}{Global.RandomFile}";
                ZephyrFile outFile = source.CreateFile(outPath);
                outFile.WriteAllText(content);
                string outText = outFile.ReadAllText();
                Assert.AreEqual(content, outText);

                outFile.Delete();
            }

            source.Delete();
        }

        [Test]
        public void AwsS3FileWriteAllBytes()
        {
            if (!Global.TestAws)
                throw new Exception("Amazon S3 Tests Are Not Enabled.  Set Global.TestAws To True To Enable.");

            ZephyrDirectory source = Global.StageTestFilesToAws();
            List<ZephyrFile> files = (List<ZephyrFile>)source.GetFiles();
            Assert.IsNotEmpty(files);
            foreach (ZephyrFile file in files)
            {
                Console.WriteLine($"File: {file.FullName}");
                byte[] bytes = file.ReadAllBytes();
                string outPath = $"{source.FullName}{Global.RandomFile}";
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
