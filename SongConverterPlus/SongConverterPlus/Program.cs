using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SongConverterPlus
{
    class Program
    {
        static Item[] required = { new Item("source-directory", 'd'), new Item("output-directory", 'o'), new Item("binary-output-directory", 'b')};
        static void Main(string[] args)
        {
            if (args.Length < required.Length)
            {
                Console.WriteLine("Must provide the following arguments:");
                foreach (Item s in required)
                {
                    Console.WriteLine(s.Full);
                }
                Console.WriteLine("Instead of the following arguments:");
                foreach (string s in args)
                {
                    Console.WriteLine(s);
                }
                return;
            }
            if (args.Length == required.Length)
            {
                for (int i = 0; i < required.Length; i++)
                {
                    Console.WriteLine("Setting: " + required[i].Full + " to: " + args[i]);
                    required[i].Value = args[i];
                }
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    foreach (Item req in required)
                    {
                        if (args[i] == req.Full || args[i][args[i].Length - 1] == req.Alias)
                        {
                            if (i + 1 >= args.Length)
                            {
                                return;
                            }
                            Console.WriteLine("Found value for: " + req.Full + " to: " + args[i + 1]);
                            req.Value = args[i + 1];
                        }
                    }
                }
            }

            Directory.CreateDirectory(required[1].Value);
            Directory.CreateDirectory(required[2].Value);
            // Now we actually do the thing
            foreach (string f in Directory.GetDirectories(required[0].Value))
            {
                // Converts data to 2.0.0
                foreach (string fd in Directory.GetDirectories(Path.Combine(required[0].Value, f)))
                {
                    System.Diagnostics.Process.Start("songe-converter.exe", "-k " + Path.Combine(Path.Combine(required[0].Value, f), fd));
                    foreach (string fs in Directory.GetFiles(Path.Combine(required[0].Value, Path.Combine(f, fd))))
                    {
                        if (!fs.EndsWith(".dat") || fs.Contains("info"))
                        {
                            continue;
                        }
                        // Perform binary conversion and copy this file to output.
                        Console.WriteLine("Found: " + fs);
                        string fileName = Path.Combine(Path.Combine(required[0].Value, f), Path.Combine(fd, fs));
                        File.Copy(fileName, Path.Combine(required[1].Value, fd + "_" + fs), true);
                        SerializeWrite(fd + "_" + fs);
                    }
                }
            }

        }
        static void SerializeWrite(string fs)
        {
            var bs = BeatmapSaveData.DeserializeFromJSONString(File.ReadAllText(Path.Combine(required[1].Value, fs)));
            byte[] bts = bs.SerializeToBinary();
            StreamWriter writer = new StreamWriter(Path.Combine(required[2].Value, fs));
            Console.WriteLine("Writing Binary Array to: " + fs);
            writer.WriteLine("\t\tArray: [");
            foreach (byte b in bts)
            {
                writer.WriteLine("\t\t\t" + b);
            }
            writer.WriteLine("\t\t]");
            writer.Flush();
            writer.Close();
        }
        class Item
        {
            internal string Full { get; }
            internal char Alias { get; }
            internal string Value { get; set; }
            internal Item(string f, char a)
            {
                Full = f;
                Alias = a;
            }
        }
    }
}
