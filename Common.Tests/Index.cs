using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Xunit;

namespace Common.Tests
{
    public class Index
    {
        // All edges and nodes covered
        [Fact]
        public void Crawl_NodeCoverage()
        {
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                {"a/b.txt", new MockFileData("")}
            });
            var result = Common.Index.Crawl(fs.DirectoryInfo.FromDirectoryName("/")).ToList();
        }

        // Zero iterations for both loop
        [Fact]
        public void Crawl_LoopCoverage_Zero()
        {
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>()
            {
               
            });
            var result = Common.Index.Crawl(fs.DirectoryInfo.FromDirectoryName("/")).ToList();
        }

        // One iteration for both loops, and zero for the inner one
        [Fact]
        public void Crawl_LoopCoverage_One_ZeroInner()
        {
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                {"b.txt", new MockFileData("")},
                {"a",new MockDirectoryData() }
            });
            var result = Common.Index.Crawl(fs.DirectoryInfo.FromDirectoryName("/")).ToList();
        }

        // One iteration for all loops
        [Fact]
        public void Crawl_LoopCoverage_One()
        {
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                {"b.txt", new MockFileData("")},
                {"a/b.txt", new MockFileData("")}
            });
            var result = Common.Index.Crawl(fs.DirectoryInfo.FromDirectoryName("/")).ToList();
        }
    }
}
