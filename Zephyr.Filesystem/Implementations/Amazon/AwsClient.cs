using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Amazon;
using Amazon.S3;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;

namespace Zephyr.Filesystem
{
    public class AwsClient
    {
        internal AmazonS3Client Client { get; set; }

        public AwsClient(RegionEndpoint endpoint = null) { Initialize(endpoint); }
        public AwsClient(AWSCredentials creds, RegionEndpoint endpoint = null) { Initialize(creds, endpoint); }
        public AwsClient(string accessKey, string secretAccessKey, RegionEndpoint endpoint = null) { Initialize(accessKey, secretAccessKey, endpoint); }
        public AwsClient(string accessKey, string secretAccessKey, string sessionToken, RegionEndpoint endpoint = null) { Initialize(accessKey, secretAccessKey, sessionToken, endpoint); }
        public AwsClient(string profileName, RegionEndpoint endpoint = null) { Initialize(profileName, endpoint); }

        /// <summary>
        /// Initialize S3Client using implicit Credentials from config or profile.
        /// </summary>
        /// <param name="endpoint">The region to connect to.</param>
        private void Initialize(RegionEndpoint endpoint = null)
        {
            if (Client != null)
                Client = null;

            if (endpoint == null)
                Client = new AmazonS3Client();
            else
                Client = new AmazonS3Client(endpoint);
        }

        /// <summary>
        /// Initialize S3Client using AWSCredentials object.
        /// </summary>
        /// <param name="creds">The AWSCredentails object.</param>
        /// <param name="endpoint">The region to connect to.</param>
        private void Initialize(AWSCredentials creds, RegionEndpoint endpoint = null)
        {
            if (Client != null)
                Client = null;

            if (endpoint == null)
                Client = new AmazonS3Client(creds);
            else
                Client = new AmazonS3Client(creds, endpoint);
        }

        /// <summary>
        /// Initialize S3Client using a BasicAWSCredentials object.
        /// </summary>
        /// <param name="accessKey">AWS Access Key Id</param>
        /// <param name="secretAccessKey">AWS Secret Access Key</param>
        /// <param name="endpoint">The region to connect to.</param>
        private void Initialize(string accessKey, string secretAccessKey, RegionEndpoint endpoint = null)
        {
            BasicAWSCredentials creds = new BasicAWSCredentials(accessKey, secretAccessKey);
            Initialize(creds, endpoint);
        }

        private void Initialize(string accessKey, string secretAccessKey, string sessionToken, RegionEndpoint endpoint = null)
        {
            SessionAWSCredentials sessionCreds = new SessionAWSCredentials(accessKey, secretAccessKey, sessionToken);
            Initialize(sessionCreds, endpoint);
        }

        private void Initialize(string profileName, RegionEndpoint endpoint = null)
        {
            CredentialProfileStoreChain chain = new CredentialProfileStoreChain();
            AWSCredentials creds = null;
            if (chain.TryGetAWSCredentials(profileName, out creds))
                Initialize(creds, endpoint);
            else
                throw new Exception($"Unable To Retrieve Credentails For Profile [{profileName}]");
        }

        public void Close()
        {
            Client = null;
        }
    }
}
