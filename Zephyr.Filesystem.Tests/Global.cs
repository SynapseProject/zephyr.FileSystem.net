﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NUnit.Framework;

using Zephyr.Filesystem;

namespace Zephyr.Filesystem.Tests
{

    [SetUpFixture]
    public class Global
    {
        public static string RandomDirectory { get { return Path.GetRandomFileName().Replace(".", ""); } }
        public static string RandomFile { get { return Path.GetRandomFileName(); } }

        [OneTimeSetUp]
        public void Init()
        {
        }

        [OneTimeTearDown]
        public void Teardown()
        {
        }
    }
}