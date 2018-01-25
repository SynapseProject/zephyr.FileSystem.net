using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Amazon.S3.IO;
using Amazon.S3.Model;

namespace Zephyr.Filesystem
{
    /// <summary>
    /// The implementation of ZephyrDirectory using Amazon S3 storage.
    /// </summary>
    public class AwsS3ZephyrDirectory : ZephyrDirectory
    {
        private string UrlPattern = @"^(s3:\/\/)(.*?)\/(.*)$";        // Gets Root, Bucket Name and Object Key
        private string NamePattern = @"^(s3:\/\/.*\/)(.*?)\/$";       // Gets Parent Name and Name

        private AwsClient _client = null;

        private string _fullName;

        /// <summary>
        /// The Amazon S3 Bucket Name
        /// </summary>
        public string BucketName { get; internal set; }

        /// <summary>
        /// The Amazon S3 ObjectKey
        /// </summary>
        public string ObjectKey { get; internal set; }

        /// <summary>
        /// The fullname / url of the directory in Amazon S3.
        /// </summary>
        public override string FullName
        {
            get { return _fullName; }
            set
            {
                _fullName = value;
                Match match = Regex.Match( value, UrlPattern, RegexOptions.IgnoreCase );
                if ( match.Success )
                {
                    BucketName = match.Groups[2].Value;
                    ObjectKey = match.Groups[3].Value;
                }
            }
        }

        /// <summary>
        /// The name of the directory in Amazon S3.
        /// </summary>
        public override string Name {
            get
            {
                String name = null;
                Match match = Regex.Match( FullName, NamePattern, RegexOptions.IgnoreCase );
                if ( match.Success )
                    name = match.Groups[2].Value;
                return name;
            }
        }

        /// <summary>
        /// The full path / url of the parent directory in Amazon S3.
        /// </summary>
        public override string Parent {
            get
            {
                String parent = null;
                Match match = Regex.Match( FullName, NamePattern, RegexOptions.IgnoreCase );
                if ( match.Success )
                    parent = match.Groups[1].Value;
                return parent;
            }
        }

        /// <summary>
        /// The root or protocol for the Amazon S3 directory.
        /// </summary>
        public override string Root {
            get
            {
                String name = null;
                Match match = Regex.Match( FullName, UrlPattern, RegexOptions.IgnoreCase );
                if ( match.Success )
                    name = match.Groups[1].Value;
                return name;
            }
        }

        /// <summary>
        /// Implementation of the ZephyrDirectory Exists method in Amazon S3 Storage.
        /// </summary>
        public override bool Exists
        {
            get
            {
                if (_client == null)
                    throw new Exception($"AWSClient Not Set.");

                string dirInfoKey = ObjectKey.Replace('/', '\\');
                S3DirectoryInfo dirInfo = new S3DirectoryInfo(_client.Client, BucketName, dirInfoKey);
                return dirInfo.Exists;
            }
        }


        /// <summary>
        /// Creates an empty AmazonS3ZephyrDirectory
        /// </summary>
        /// <param name="client">The client class used to connect to Amazon.</param>
        public AwsS3ZephyrDirectory(AwsClient client) { _client = client; }

        /// <summary>
        /// Creates an AmazonS3ZephyrDirectory representing the url passed in.
        /// </summary>
        /// <param name="client">The client class used to connect to Amazon.</param>
        /// <param name="fullName">The Fullname or URL for the Amazon S3 directory.</param>
        public AwsS3ZephyrDirectory(AwsClient client, string fullName)
        {
            _client = client;
            FullName = fullName;
        }

        /// <summary>
        /// Implementation of the ZephyrDirectory Create method in Amazon S3 Storage.
        /// </summary>
        /// <param name="failIfExists">Throws an error if the directory already exists.</param>
        /// <param name="callbackLabel">Optional "label" to be passed into the callback method.</param>
        /// <param name="callback">Optional method that is called for logging purposes.</param>
        /// <returns>An AmazonS3ZephyrDictionary Instance.</returns>
        public override ZephyrDirectory Create(bool failIfExists = false, bool verbose = true, string callbackLabel = null, Action<string, string> callback = null)
        {
            if (_client == null)
                throw new Exception($"AWSClient Not Set.");

            if (this.Exists && failIfExists)
                throw new Exception($"Directory [{FullName}] Already Exists.");

            String key = ObjectKey;
            if (key.EndsWith("/"))
                key = key.Substring(0, key.Length - 1);
            S3DirectoryInfo dirInfo = new S3DirectoryInfo(_client.Client, BucketName, key);
            dirInfo.Create();
            if (verbose)
                Logger.Log($"Directory [{FullName}] Was Created.", callbackLabel, callback);
            return this;
        }

        /// <summary>
        /// Creates an AmazonS3ZephyrFile implementation using the Fullname / URL passed in.
        /// </summary>
        /// <param name="fullName">Full name or URL of the file to be created.</param>
        /// <param name="callbackLabel">Optional "label" to be passed into the callback method.</param>
        /// <param name="callback">Optional method that is called for logging purposes.</param>
        /// <returns>An AmazonS3ZephyrFile implementation.</returns>
        public override ZephyrFile CreateFile(string fullName, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            return new AwsS3ZephyrFile(_client, fullName);
        }

        /// <summary>
        /// Creates an AmazonS3ZephyrDirectory implementation using the Fullname / URL passed in.
        /// </summary>
        /// <param name="fullName">Full name or URL of the directory to be created.</param>
        /// <param name="callbackLabel">Optional "label" to be passed into the callback method.</param>
        /// <param name="callback">Optional method that is called for logging purposes.</param>
        /// <returns>An AmazonS3ZephyrDirectory implementation.</returns>
        public override ZephyrDirectory CreateDirectory(string fullName, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            return new AwsS3ZephyrDirectory(_client, fullName);
        }

        /// <summary>
        /// Implementation of the ZephyrDirectory Delete method in Amazon S3 Storage.
        /// </summary>
        /// <param name="recurse">Remove all objects in the directory as well.  If set to "false", directory must be empty or an exception will be thrown.</param>
        /// <param name="stopOnError">Stop deleting objects in the directory if an error is encountered.</param>
        /// <param name="verbose">Log each object that is deleted from the directory.</param>
        /// <param name="callbackLabel">Optional "label" to be passed into the callback method.</param>
        /// <param name="callback">Optional method that is called for logging purposes.</param>
        public override void Delete(bool recurse = true, bool stopOnError = true, bool verbose = true, string callbackLabel = null, Action<string, string> callback = null)
        {
            try
            {
                if (_client == null)
                    throw new Exception($"AWSClient Not Set.");

                String key = ObjectKey;
                key = key.Replace('/', '\\');
                if (key.EndsWith("\\"))
                    key = key.Substring(0, key.Length - 1);
                S3DirectoryInfo dirInfo = new S3DirectoryInfo(_client.Client, BucketName, key);

                if (dirInfo.Exists)
                {
                    if (!recurse)
                    {
                        int dirs = dirInfo.GetDirectories().Length;
                        int files = dirInfo.GetFiles().Length;
                        if (dirs > 0 || files > 0)
                            throw new Exception($"Directory [{FullName}] is not empty.");
                    }

                    dirInfo.Delete(recurse);
                }

                if (verbose)
                    Logger.Log($"Directory [{FullName}] Was Deleted.", callbackLabel, callback);
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, callbackLabel, callback);
                if (stopOnError)
                    throw;
            }
        }

        /// <summary>
        /// Implementation of the ZephyrDirectory GetDirectories method in AmazonS3Storage.
        /// </summary>
        /// <returns>An enumeration of AmazonS3ZephyrDirectory objects.</returns>
        public override IEnumerable<ZephyrDirectory> GetDirectories()
        {
            if (_client == null)
                throw new Exception($"AWSClient Not Set.");

            List<ZephyrDirectory> dirs = new List<ZephyrDirectory>();
            S3DirectoryInfo dInfo = new S3DirectoryInfo(this._client.Client, this.BucketName, ObjectKey.Replace('/', '\\'));
            S3DirectoryInfo[] children = dInfo.GetDirectories();

            foreach (S3DirectoryInfo child in children)
                dirs.Add(new AwsS3ZephyrDirectory(_client, PathCombine(this.FullName, $"{child.Name}/")));

            return dirs;
        }

        /// <summary>
        /// Implementation of the ZephyrDirectory GetFiles method in AmazonS3Storage.
        /// </summary>
        /// <returns>An enumeration of AmazonS3ZephyrFile objects.</returns>
        public override IEnumerable<ZephyrFile> GetFiles()
        {
            if (_client == null)
                throw new Exception($"AWSClient Not Set.");

            List<ZephyrFile> files = new List<ZephyrFile>();
            S3DirectoryInfo dInfo = new S3DirectoryInfo(this._client.Client, this.BucketName, ObjectKey.Replace('/', '\\'));
            S3FileInfo[] children = dInfo.GetFiles();

            foreach (S3FileInfo child in children)
                files.Add(new AwsS3ZephyrFile(_client, PathCombine(this.FullName, child.Name)));

            return files;
        }

        /// <summary>
        /// Implementation of the ZephyrDirectory PathCombine method in AmazonS3Storage.
        /// </summary>
        /// <param name="paths">An array of strings to combine.</param>
        /// <returns>The combined paths.</returns>
        public override string PathCombine(params string[] paths)
        {
            StringBuilder sb = new StringBuilder();
            for (int i=0; i<paths.Length; i++)
            {
                string path = paths[i]?.Trim();
                if ( path == null )
                    continue;
                else if ( path.EndsWith( "/" ) )
                    sb.Append( path );
                else if ( i == paths.Length - 1)
                    sb.Append( path );
                else
                    sb.Append( $"{path}/" );
            }

            return sb.ToString();
        }
    }
}
