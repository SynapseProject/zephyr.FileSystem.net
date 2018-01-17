using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Amazon;

namespace Zephyr.Filesystem
{
    public class Sandbox
    {
        public static void Main(string[] args)
        {
            AwsClient awsClient = new AwsClient(RegionEndpoint.EUWest1);
            AwsS3ZephyrDirectory dir = new AwsS3ZephyrDirectory(awsClient, @"s3://wagug0-test/");

            foreach (ZephyrDirectory d in dir.GetDirectories())
                Console.WriteLine(d.FullName);

            foreach (ZephyrFile f in dir.GetFiles())
                Console.WriteLine(f.FullName);

            Console.WriteLine( "Press <ENTER> To Continue..." );
            Console.ReadLine();
        }

        private static void ConsoleWriter(string label, string message)
        {
            Console.WriteLine( $"{label} - {message}" );
        }
    }
}
