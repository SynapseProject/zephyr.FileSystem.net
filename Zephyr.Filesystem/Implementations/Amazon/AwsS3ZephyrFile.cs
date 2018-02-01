using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.IO;
using Amazon.S3.Transfer;

namespace Zephyr.Filesystem
{
    /// <summary>
    /// The implementation of ZephyrFile using Amazon S3 storage.
    /// </summary>
    public class AwsS3ZephyrFile : ZephyrFile
    {

        private string UrlPattern = @"^(s3:\/\/)(.*?)\/(.*)$";        // Gets Root, Bucket Name and Object Key

        private AwsClient _client = null;

        /// <summary>
        /// The name of the file in Amazon S3.
        /// </summary>
        public override string Name { get { return FullName.Substring(FullName.LastIndexOf(@"/") + 1); } }

        /// <summary>
        /// The Fullname / URL of the file in Amazon S3.
        /// </summary>
        public override string FullName {
            get { return _fullName; }
            set
            {
                _fullName = value;
                Match match = Regex.Match(value, UrlPattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    BucketName = match.Groups[2].Value;
                    ObjectKey = match.Groups[3].Value;
                }
            }
        }

        private string _fullName;

        /// <summary>
        /// The Amazon S3 Bucket Name.
        /// </summary>
        public string BucketName { get; internal set; }

        /// <summary>
        /// The Amazon S3 Object Key.
        /// </summary>
        public string ObjectKey { get; internal set; }

        /// <summary>
        /// Does the file exist.
        /// </summary>
        public override bool Exists
        {
            get
            {
                if (_client == null)
                    throw new Exception($"AWSClient Not Set.");

                String key = ObjectKey;
                key = key.Replace('/', '\\');
                S3FileInfo fileInfo = new S3FileInfo(_client.Client, BucketName, key);
                return fileInfo.Exists;
            }
        }

        /// <summary>
        /// Creates an empty AmazonS3ZephyrFile
        /// </summary>
        /// <param name="client">The client class used to connect to Amazon.</param>
        public AwsS3ZephyrFile(AwsClient client) { _client = client; }

        /// <summary>
        /// Creates an AmazonS3ZephyrFile representing the url passed in.
        /// </summary>
        /// <param name="client">The client class used to connect to Amazon.</param>
        /// <param name="fullName">The Fullname or URL for the Amazon S3 file.</param>
        public AwsS3ZephyrFile(AwsClient client, string fullName)
        {
            _client = client;
            FullName = fullName;
        }

        /// <summary>
        /// Implementation of the ZephyrFile Open method in Amazon S3 Storage.
        /// </summary>
        /// <param name="access">Specifies to open Stream with "Read" or "Write" access.</param>
        /// <param name="callbackLabel">Optional "label" to be passed into the callback method.</param>
        /// <param name="callback">Optional method that is called for logging purposes.</param>
        /// <returns>The open Stream for the AmazonS3ZephyrFile.</returns>
        public override System.IO.Stream Open(AccessType access, bool verbose = true, string callbackLabel = null, Action<string, string> callback = null)
        {
            if (!Exists)
            {
                Create(verbose: false);
                Close(false);
            }

            if (!IsOpen)
            {
                if (_client == null)
                    throw new Exception($"AWSClient Not Set.");

                S3FileInfo file = new S3FileInfo(_client.Client, BucketName, ObjectKey);
                if (access == AccessType.Read)
                    this.Stream = file.OpenRead();
                else if (access == AccessType.Write)
                    this.Stream = file.OpenWrite();
                else
                    throw new Exception($"Unknown AccessType [{access}] Received.");
            }

            return this.Stream;
        }

        /// <summary>
        /// Implementation of the ZephyrFile Open method in Amazon S3 Storage.
        /// </summary>
        /// <param name="callbackLabel">Optional "label" to be passed into the callback method.</param>
        /// <param name="callback">Optional method that is called for logging purposes.</param>
        public override void Close(bool verbose = true, string callbackLabel = null, Action<string, string> callback = null)
        {
            if (IsOpen)
            {
                this.Stream.Close();
            }
            this.Stream = null;
        }

        /// <summary>
        /// Implementation of the ZephyrFile Create method in Amazon S3 Storage.
        /// </summary>
        /// <param name="overwrite">Will overwrite the file if it already exists.</param>
        /// <param name="callbackLabel">Optional "label" to be passed into the callback method.</param>
        /// <param name="callback">Optional method that is called for logging purposes.</param>
        /// <returns>An instance of a AmazonS3ZephyrFile.</returns>
        public override ZephyrFile Create(bool overwrite = true, bool verbose = true, string callbackLabel = null, Action<string, string> callback = null)
        {
            try
            {
                if (this.Exists && !overwrite)
                    throw new Exception($"File [{this.FullName}] Already Exists.");

                if (_client == null)
                    throw new Exception($"AWSClient Not Set.");

                S3FileInfo fileInfo = new S3FileInfo(_client.Client, BucketName, ObjectKey);
                this.Stream = fileInfo.Create();
                // File isn't written to S3 Bucket Until The Stream Is Closed.  Force Write By Closing Stream.
                this.Close(false);
                this.Open(AccessType.Write, false);
                if (verbose)
                    Logger.Log($"File [{FullName}] Was Created.", callbackLabel, callback);
                return this;
            }
            catch (Exception e)
            {
                Logger.Log($"ERROR - {e.Message}", callbackLabel, callback);
                throw;
            }
        }

        /// <summary>
        /// Implementation of the ZephyrFile CreateFile method in Amazon S3 Storage.
        /// </summary>
        /// <param name="fullName">Full name or URL of the file to be created.</param>
        /// <param name="callbackLabel">Optional "label" to be passed into the callback method.</param>
        /// <param name="callback">Optional method that is called for logging purposes.</param>
        /// <returns>An AmazonS3ZephyrFile implementation.</returns>
        public override ZephyrFile CreateFile(string fileName, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            return new AwsS3ZephyrFile(_client, fileName);
        }

        /// <summary>
        /// Implementation of the ZephyrFile CreateDirectory method in Amazon S3 Storage.
        /// </summary>
        /// <param name="dirName">Full name or URL of the directory to be created.</param>
        /// <param name="callbackLabel">Optional "label" to be passed into the callback method.</param>
        /// <param name="callback">Optional method that is called for logging purposes.</param>
        /// <returns>An AmazonS3ZephyrDirectory implementation.</returns>
        public override ZephyrDirectory CreateDirectory(string dirName, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            return new AwsS3ZephyrDirectory(_client, dirName);
        }

        /// <summary>
        /// Implementation of the ZephyrFile Delete method in Amazon S3 Storage.
        /// </summary>
        /// <param name="stopOnError">Throw an exception when an error occurs.</param>
        /// <param name="verbose">Log details of file deleted.</param>
        /// <param name="callbackLabel">Optional "label" to be passed into the callback method.</param>
        /// <param name="callback">Optional method that is called for logging purposes.</param>
        public override void Delete(bool stopOnError = true, bool verbose = true, string callbackLabel = null, Action<string, string> callback = null)
        {
            try
            {
                if (_client == null)
                    throw new Exception($"AWSClient Not Set.");

                S3FileInfo fileInfo = new S3FileInfo(_client.Client, BucketName, ObjectKey);

                if (fileInfo.Exists)
                {
                    if (IsOpen)
                        Close();
                    fileInfo.Delete();
                }

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
    }
}
