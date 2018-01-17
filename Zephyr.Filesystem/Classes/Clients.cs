using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Amazon;
using Amazon.Runtime;

namespace Zephyr.Filesystem
{
    public class Clients
    {
        public AwsClient aws { get; set; } = null;

        public AwsClient AwsInitialize(RegionEndpoint endpoint = null)
        {
            aws = new AwsClient(endpoint);
            return aws;
        }

        public AwsClient AwsInitialize(AWSCredentials creds, RegionEndpoint endpoint = null)
        {
            aws = new AwsClient(creds, endpoint);
            return aws;
        }

        public AwsClient AwsInitialize(string accessKey, string secretAccessKey, RegionEndpoint endpoint = null)
        {
            aws = new AwsClient(accessKey, secretAccessKey, endpoint);
            return aws;
        }

        public AwsClient AwsInitialize(string accessKey, string secretAccessKey, string sessionToken, RegionEndpoint endpoint = null)
        {
            aws = new AwsClient(accessKey, secretAccessKey, sessionToken, endpoint);
            return aws;
        }

        public AwsClient AwsInitialize(string profileName, RegionEndpoint endpoint = null)
        {
            aws = new AwsClient(profileName, endpoint);
            return aws;
        }

    }
}
