using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace NBTParser
{
    public class NBT
    {
        public NBT(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Couldn't find the specified NBT file. Please provide a correct path.");
            }
            this.AbsolutePath = Path.GetFullPath(path);
            this.FileName = Path.GetFileName(path);

            using (var fstream = File.OpenRead(this.AbsolutePath))
            {
                using (var stream = new GZipStream(fstream, CompressionMode.Decompress))
                {
                    this.RootTag = (TagCompund)NBTHelpers.GetTag(stream, 0);
                }
            }
        }

        public override string ToString()
        {
            return $"NBT file. Name: {this.FileName}, Path: {this.AbsolutePath}";
        }

        public string AbsolutePath { get; set; }
        public string FileName { get; set; }
        public TagCompund RootTag { get; set; }
    }
}
