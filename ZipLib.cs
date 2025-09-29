using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace NekoSDKPacker
{
    static class ZipLib
    {
        public static long DeflateFileFake(FileStream fs)
        {
            using var cs = new FakeStream();
            DeflateFile(fs, cs);

            return cs.Length;
        }

        public static byte[] DeflateFile(FileStream fs)
        {
            using var ms = new MemoryStream((int)fs.Length);
            DeflateFile(fs, ms);

            return ms.ToArray();
        }

        static void DeflateFile(FileStream fs, Stream bos)
        {
            using var dos = new DeflaterOutputStream(bos, new Deflater(Deflater.BEST_COMPRESSION));
            fs.CopyTo(dos);
            dos.Flush();
            dos.Finish();

            bos.Write(BitConverter.GetBytes((int)fs.Length));
        }

        sealed class FakeStream : Stream
        {
            private long _bytesWritten;
            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => _bytesWritten;
            public override long Position { get => _bytesWritten; set => throw new NotSupportedException(); }
            public override void Flush() { }
            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => _bytesWritten += count;
        }
    }
}
