using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zephyr.Filesystem
{
    public enum AccessType
    {
        Read,
        Write
    }

    public enum UrlType
    {
        Unknown,
        LocalFile,
        LocalDirectory,
        NetworkFile,
        NetworkDirectory,
        AwsS3File,
        AwsS3Directory
    }
}
