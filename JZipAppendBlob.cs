using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System.IO.Compression;
using System.Runtime.InteropServices.ObjectiveC;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace JZipBlob
{
    public static class JZipAppendBlob
    {
        /// <summary>
        /// Compress JSON data as a string of List<object[]> and append the blob
        /// </summary>
        /// <param name="appendBlobClient">Azure.Storage.Blobs.AppendBlobClient</param>
        /// <param name="data"></param>
        /// <param name="encodingCode">Any valid ecoding code, 65001 = utf8; 1200 = Unicode; 12000 = utf32</param>
        /// <returns></returns>
        public static async Task CompressAndAppendBlobAsync(AppendBlobClient appendBlobClient, string data, int encodingCode = 65001)
        {
            byte[] bytes = Encoding.GetEncoding(encodingCode).GetBytes(data);

            using Stream stream = await appendBlobClient.OpenWriteAsync(false);

            using var gzipStream = new GZipStream(stream, CompressionLevel.Optimal);

            await gzipStream.WriteAsync(bytes);
        }

        //public static T FromByteArray<T>(byte[] data)
        //{
        //    if (data == null)
        //        return default(T);
        //    BinaryFormatter bf = new BinaryFormatter();
        //    using (MemoryStream ms = new MemoryStream(data))
        //    {
        //        object obj = bf.Deserialize(ms);
        //        return (T)obj;
        //    }
        //}

        //public byte[] ToByteArray<T>(T obj)
        //{
        //    if (obj == null)
        //        return null;
        //    BinaryFormatter bf = new BinaryFormatter();
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        bf.Serialize(ms, obj);
        //        return ms.ToArray();
        //    }
        //}

        /// <summary>
        /// Decompress JSON data as a string of List<object[]> from an append blob
        /// </summary>
        /// <param name="blocClient">Azure.Storage.Blobs.BlobClient</param>
        /// <param name="isSealed">Not yet implemented - Use true to get data from an open append blob.
        /// Use false to get data from a closed or sealed append blob, else use null and a check will be done</param>
        /// <param name="encodingCode">Any valid ecoding code, 65001 = utf8; 1200 = Unicode; 12000 = utf32</param>
        /// <returns></returns>
        public static async Task<string> DecompressAndDownloadAppendBlobAsync(BlobClient blocClient, int encodingCode = 65001)//bool? isSealed = null)
        {
            using var memoryStream = new MemoryStream();

            using (var decompressStream = new GZipStream(await blocClient.OpenReadAsync(new BlobOpenReadOptions(false)), CompressionMode.Decompress))
            {
                await decompressStream.CopyToAsync(memoryStream);
            }

            byte[] arr = memoryStream.ToArray();

                //if (!isSealed.HasValue)
                //{
                //    throw new NotImplementedException();
                //}

            //if (!isSealed.Value)
            //{
                arr[^1] = (byte)']';
            //}

            return Encoding.GetEncoding(encodingCode).GetString(arr);
        }

        public static void AddStartRow(StringBuilder sb, string values)
        {
            sb.Append('[');
            sb.Append(values);
            sb.Append(',');
        }

        public static void AddBodyRow(StringBuilder sb, string values)
        {
            sb.Append(values);
            sb.Append(',');
        }
    }
}