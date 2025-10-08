using System.IO.MemoryMappedFiles;
using System.Text;

namespace NekoSDKPacker
{
    static class ArchiveWriter
    {
        public static void Create(string folderPath, string filePath)
        {
            // Initialize entry list
            var entries = new List<TEntry>();
            foreach (var _ in Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories))
            {
                entries.Add(new TEntry
                {
                    FullPath = _,
                    RelativePath = Path.GetRelativePath(folderPath, _).Replace("/", "\\")
                });
            }

            var cursor = 0L;

            using (var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Create, null, 4L * 1024 * 1024 * 1024))
            using (var vs = mmf.CreateViewStream())
            using (var va = mmf.CreateViewAccessor())
            {
                // Write signature
                vs.Write(Encoding.ASCII.GetBytes("NEKOPACK4A"));

                // Placeholder for index size
                vs.Write(BitConverter.GetBytes(0));

                // Record index position
                var idxPos = vs.Position;

                // Write index
                var nameEnc = Encoding.GetEncoding("shift_jis");
                foreach (var entry in entries)
                {
                    // Write name
                    var nameBytes = nameEnc.GetBytes(entry.RelativePath);
                    vs.Write(BitConverter.GetBytes(nameBytes.Length + 1));
                    vs.Write(nameBytes);
                    vs.WriteByte(0); // null-terminated

                    // Record position & hash
                    entry.Position = vs.Position;
                    entry.Hash = ComputeHash(nameBytes);

                    // Placeholder for offset & length
                    vs.Write(BitConverter.GetBytes(0));
                    vs.Write(BitConverter.GetBytes(0));
                }
                vs.Write(BitConverter.GetBytes(0)); // end of index

                // Write index size
                va.Write(0xA, Convert.ToUInt32(vs.Position - idxPos));

                // Record cursor
                cursor = vs.Position;

                // Use 2/3 of cpu
                var po = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 / 3 };

                // Compute length (parallel)
                Parallel.ForEach(entries, po, entry =>
                {
                    Console.WriteLine($"Reading \"{entry.RelativePath}\"");

                    entry.Length = Convert.ToUInt32(ZipLib.DeflateFileFake(entry.FullPath));
                });

                // Compute offset & cursor (sequential)
                entries.ForEach(entry => { entry.Offset = Convert.ToUInt32(cursor); cursor += entry.Length; });

                // Write data | Backfill offset & length (parallel)
                Parallel.ForEach(entries, po, entry =>
                {
                    Console.WriteLine($"Writing \"{entry.RelativePath}\"");

                    va.WriteArray(entry.Offset, EncryptData(ZipLib.DeflateFile(entry.FullPath)), 0, Convert.ToInt32(entry.Length)); // data

                    va.Write(entry.Position, entry.Offset ^ entry.Hash); // offset
                    va.Write(entry.Position + sizeof(uint), entry.Length ^ entry.Hash); // length
                });
            }

            // Fix file length
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Write);
            fs.SetLength(cursor);
        }

        static uint ComputeHash(byte[] data)
        {
            int hash = 0;

            foreach (var b in data) hash += (sbyte)b;

            return (uint)hash;
        }

        static byte[] EncryptData(byte[] data)
        {
            byte key = (byte)((data.Length >> 3) + 0x22);

            for (var i = 0; i < 0x20; i++)
            {
                if (i >= data.Length)
                    break;

                data[i] ^= key;
                key *= 8;
            }

            return data;
        }

        class TEntry
        {
            public string FullPath;
            public string RelativePath;
            public long Position;
            public uint Hash;
            public uint Length;
            public uint Offset;
        }
    }
}
