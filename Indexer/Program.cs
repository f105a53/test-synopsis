﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kurukuru;
using MoreLinq;

namespace Indexer
{
    internal class Program
    {
        private static Common.Index indexer;

        private static string path = @"C:\Users\Jacob Hinze\AppData\Local\Temp\d7188c12-4a50-4d94-ba37-4c28840a67be\";
        private static int batchSize = 10000;
        

        private async static Task Main(string[] args)
        {
            indexer = new Common.Index(@"./lucene-index");

            await indexer.Build(path, batchSize, new Progress<string>(message => Console.WriteLine(message)));
        }
    }
}
