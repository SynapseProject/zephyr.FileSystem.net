using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zephyr.Filesystem
{
    public class Logger
    {
        public static void Log(string message, string callbackLabel, Action<string, string> callbackFunction)
        {
            if (callbackFunction != null)
                callbackFunction(callbackLabel, message);
            else
                Console.WriteLine(message);
        }
    }
}
