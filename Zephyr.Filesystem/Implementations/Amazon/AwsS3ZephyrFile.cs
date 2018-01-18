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
    public class AwsS3ZephyrFile : ZephyrFile
    {
         
        public static string UrlPattern = @"^(s3:\/\/)(.*?)\/(.*)$";        // Gets Root, Bucket Name and Object Key

        private AwsClient _client = null;

        public override string Name { get { return FullName.Substring( FullName.LastIndexOf( @"/" ) + 1 ); } }
        public override string FullName {
            get { return _fullName; }
            set
            {
                _fullName = value;
                Match match = Regex.Match( value, UrlPattern, RegexOptions.IgnoreCase );
                if (match.Success)
                {
                    BucketName = match.Groups[2].Value;
                    ObjectKey = match.Groups[3].Value;
                }
            }
        }

        private string _fullName;

        public string BucketName { get; internal set; }
        public string ObjectKey { get; internal set; }

        public AwsS3ZephyrFile(AwsClient client) { _client = client; }
        public AwsS3ZephyrFile(AwsClient client, string fullName)
        {
            _client = client;
            FullName = fullName;
        }


        public override System.IO.Stream OpenStream(AccessType access, string callbackLabel = null, Action<string, string> callback = null)
        {
            if ( !IsOpen )
            {
                if (_client == null)
                    throw new Exception($"AWSClient Not Set.");

                S3FileInfo file = new S3FileInfo( _client.Client, BucketName, ObjectKey );
                if ( access == AccessType.Read )
                    this.Stream = file.OpenRead();
                else if ( access == AccessType.Write )
                    this.Stream = file.OpenWrite();
                else
                    throw new Exception( $"Unknown AccessType [{access}] Received." );
            }
            return this.Stream;
        }

        public override void CloseStream(string callbackLabel = null, Action<string, string> callback = null)
        {
            if (IsOpen)
            {
                this.Stream.Close();
            }
            this.Stream = null;
        }

        public override ZephyrFile Create(bool overwrite = true, string callbackLabel = null, Action<string, string> callback = null)
        {
            try
            {
                if (this.Exists() && !overwrite)
                    throw new Exception($"File [{this.FullName}] Already Exists.");

                if (_client == null)
                    throw new Exception($"AWSClient Not Set.");

                S3FileInfo fileInfo = new S3FileInfo(_client.Client, BucketName, ObjectKey);
                this.Stream = fileInfo.Create();
                callback?.Invoke(callbackLabel, $"File [{FullName}] Was Created.");
                return this;
            }
            catch (Exception e)
            {
                Logger.Log($"ERROR - {e.Message}", callbackLabel, callback);
                throw;
            }
        }

        public override ZephyrDirectory CreateDirectory(string dirName, String callbackLabel = null, Action<string, string> callback = null)
        {
            return new AwsS3ZephyrDirectory(_client, dirName);
        }

        public override void Delete(bool stopOnError = true, bool verbose = true, string callbackLabel = null, Action<string, string> callback = null)
        {
            try
            {
                if (_client == null)
                    throw new Exception($"AWSClient Not Set.");

                S3FileInfo fileInfo = new S3FileInfo(_client.Client, BucketName, ObjectKey);

                if (fileInfo.Exists)
                    fileInfo.Delete();

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

        public override bool Exists()
        {
            if (_client == null)
                throw new Exception($"AWSClient Not Set.");

            String key = ObjectKey;
            key = key.Replace('/', '\\');
            S3FileInfo fileInfo = new S3FileInfo(_client.Client, BucketName, key);
            return fileInfo.Exists;
        }

    }
}
