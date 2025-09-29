using System.IO.MemoryMappedFiles;
using System.Text;

namespace NekoSDKPacker
{
    static class ArchiveWriter
    {
        // Initialize entry list
        public static void Create(string folderPath, string filePath)
        {
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

                    // Record hash & position
                    entry.Hash = ComputeHash(nameBytes);
                    entry.Position = vs.Position;

                    // Placeholder for offset & length
                    vs.Write(BitConverter.GetBytes(0));
                    vs.Write(BitConverter.GetBytes(0));
                }
                vs.Write(BitConverter.GetBytes(0)); // end of index

                // Write index size
                va.Write(0xA, (uint)(vs.Position - idxPos));

                // Record cursor
                cursor = vs.Position;

                // Use half of cpu
                var po = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 / 3 };

                // Compute length (parallel)
                var lengths = new long[entries.Count];
                Parallel.ForEach(Enumerable.Range(0, entries.Count), po, i =>
                {
                    var entry = entries[i];

                    Console.WriteLine($"Reading \"{entry.RelativePath}\"");

                    using var fs = File.OpenRead(entry.FullPath);
                    lengths[i] = ZipLib.DeflateFileFake(fs);
                });

                // Compute offset & cursor (sequential)
                var offsets = new long[entries.Count];
                for (int i = 0; i < entries.Count; i++) { offsets[i] = cursor; cursor += lengths[i]; }

                // Write data | Backfill offset & length (parallel)
                Parallel.ForEach(Enumerable.Range(0, entries.Count), po, i =>
                {
                    var entry = entries[i];

                    Console.WriteLine($"Writing \"{entry.RelativePath}\"");

                    using var fs = File.OpenRead(entry.FullPath);
                    var data = EncryptData(ZipLib.DeflateFile(fs));

                    va.WriteArray(offsets[i], data, 0, data.Length); // data

                    va.Write(entry.Position, (uint)offsets[i] ^ entry.Hash); // offset
                    va.Write(entry.Position + sizeof(uint), (uint)data.Length ^ entry.Hash); // length
                });
            }

            // Fix file length
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Write);
            fs.SetLength(cursor);
        }

        static uint ComputeHash(byte[] data)
        {
            int hash = 0;

            for (var i = 0; i < data.Length; i++)
            {
                hash += (sbyte)data[i];
            }

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
        }
    }
}
