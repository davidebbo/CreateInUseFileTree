using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CreateInUseFileTree
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine(@"Syntax: CreateInUseFileTree procmon.csv c:\some\root\folder d:\targetfolder");
                return 1;
            }

            var csvReader = new CsvReader(File.OpenText(args[0]));

            string rootPath = args[1];
            if (!rootPath.EndsWith(Path.DirectorySeparatorChar))
            {
                rootPath += Path.DirectorySeparatorChar;
            }

            string targetFolder = args[2];

            var pathsInUse = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var entry in csvReader.GetRecords<ProcMonEntry>())
            {
                if (entry.Path.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
                {
                    pathsInUse.Add(entry.Path);
                }
            }

            foreach (var path in pathsInUse.OrderBy(p => p))
            {
                string relativePath = path.Substring(rootPath.Length);
                string targetPath = Path.Combine(targetFolder, relativePath);

                if (File.Exists(path))
                {
                    string folder = Path.GetDirectoryName(targetPath);
                    Directory.CreateDirectory(folder);
                    File.Copy(path, targetPath, overwrite: true);
                    Console.WriteLine($"Copying file {relativePath}");
                }
                else if (Directory.Exists(path))
                {
                    Directory.CreateDirectory(targetPath);
                    Console.WriteLine($"Creating directory {relativePath}");
                }
            }

            return 0;
        }
    }

    class ProcMonEntry
    {
        public string Operation { get; set; }
        public string Path { get; set; }
    }
}
