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
        public System.IO.Stream Stream { get; internal set; }
        public bool IsOpen { get { return this.Stream == null ? false : (this.Stream.CanRead || this.Stream.CanWrite); } }
        public bool CanRead { get { return this.Stream == null ? false : this.Stream.CanRead; } }
        public bool CanWrite { get { return this.Stream == null ? false : this.Stream.CanWrite; } }

        public ZephyrFile() { }

        public ZephyrFile(string fullName)
        {
            FullName = fullName;
        }

        public abstract ZephyrFile Create(bool overwrite = true, String callbackLabel = null, Action<string, string> callback = null);
        public abstract void Delete(bool stopOnError = true, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null);
        public abstract bool Exists();

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

        public Stream ResetStream(AccessType access, String callbackLabel = null, Action<string, string> callback = null)
        {
            if (IsOpen)
                CloseStream(callbackLabel, callback);

            return OpenStream(access, callbackLabel, callback);
        }

        public string[] ReadAllLines(String callbackLabel = null, Action<string, string> callback = null)
        {
            List<string> lines = new List<string>();

            ResetStream(AccessType.Read, callbackLabel, callback);

            using (StreamReader reader = new StreamReader(this.Stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    lines.Add(line);
            }

            CloseStream(callbackLabel, callback);

            return lines.ToArray();
        }

        public string ReadAllText(String callbackLabel = null, Action<string, string> callback = null)
        {
            string text = null;
            ResetStream(AccessType.Read, callbackLabel, callback);
            StreamReader reader = new StreamReader(this.Stream);
            text = reader.ReadToEnd();

            CloseStream(callbackLabel, callback);
            return text;
        }

        public byte[] ReadAllBytes(String callbackLabel = null, Action<string, string> callback = null)
        {
            ResetStream(AccessType.Read, callbackLabel, callback);

            // Logic From : https://stackoverflow.com/questions/1080442/how-to-convert-an-stream-into-a-byte-in-c
            byte[] readBuffer = new byte[4096];
            int totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = this.Stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
            {
                totalBytesRead += bytesRead;
                if (totalBytesRead == readBuffer.Length)
                {
                    int nextByte = Stream.ReadByte();
                    if (nextByte != -1)
                    {
                        byte[] temp = new byte[readBuffer.Length * 2];
                        Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                        Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                        readBuffer = temp;
                        totalBytesRead++;
                    }
                }
            }

            byte[] buffer = readBuffer;
            if (readBuffer.Length != totalBytesRead)
            {
                buffer = new byte[totalBytesRead];
                Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
            }

            return buffer;
        }

        public void WriteAllLines(string[] lines, String callbackLabel = null, Action<string, string> callback = null)
        {
            ResetStream(AccessType.Write, callbackLabel, callback);

            StreamWriter writer = new StreamWriter(this.Stream);
            foreach (string line in lines)
                writer.WriteLine(line);

            writer.Flush();
            writer.Close();
            CloseStream(callbackLabel, callback);
        }

        public void WriteAllText(string text, String callbackLabel = null, Action<string, string> callback = null)
        {
            ResetStream(AccessType.Write, callbackLabel, callback);

            StreamWriter writer = new StreamWriter(this.Stream);
            writer.Write(text);
            writer.Flush();
            writer.Close();
            CloseStream(callbackLabel, callback);
        }

        public void WriteAllBytes(byte[] bytes, String callbackLabel = null, Action<string, string> callback = null)
        {
            ResetStream(AccessType.Write, callbackLabel, callback);
            this.Stream.Write(bytes, 0, bytes.Length);
            this.Stream.Flush();
            CloseStream(callbackLabel, callback);
        }
    }
}

