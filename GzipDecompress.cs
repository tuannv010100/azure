
namespace Azure.Gzip
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    public class GzipDecompress
    {
        public string EnCode(string originalText)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(originalText);

            using MemoryStream compressedStream = new();
            using (GZipStream gzipStream = new(compressedStream, CompressionMode.Compress))
            {
                gzipStream.Write(bytes, 0, bytes.Length);
            }

            byte[] compressedBytes = compressedStream.ToArray();
            string compressedText = Convert.ToBase64String(compressedBytes);
            return compressedText;
        }
        // Mã hóa ngược
        public string DeCode(string compressedText)
        {
            if (compressedText == null) return compressedText;
            // Chuyển chuỗi Base64 về dạng mảng byte
            byte[] compressedBytes = Convert.FromBase64String(compressedText);

            using MemoryStream compressedStream = new(compressedBytes);
            using MemoryStream decompressedStream = new();
            using GZipStream gzipStream = new(compressedStream, CompressionMode.Decompress);
            gzipStream.CopyTo(decompressedStream);

            // Chuyển mảng byte giải nén về chuỗi
            byte[] decompressedBytes = decompressedStream.ToArray();
            return Encoding.UTF8.GetString(decompressedBytes);
        }
    }
}
