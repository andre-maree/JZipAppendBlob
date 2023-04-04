using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System.IO.Compression;
using System.Text;

namespace JZipBlob
{
    public static class JZipAppendBlob
    {
        /// <summary>
        /// Compress JSON data as a string of List<object[]> and append the blob
        /// </summary>
        /// <param name="appendBlobClient"></param>
        /// <param name="data"></param>
        /// <param name="encodingCode"></param>
        /// <returns></returns>
        public static async Task CompressAndAppendBlobAsync(AppendBlobClient appendBlobClient, string data, int encodingCode = 65001)
        {
            byte[] bytes = Encoding.GetEncoding(encodingCode).GetBytes(data);

            using Stream stream = await appendBlobClient.OpenWriteAsync(false);

            using var gzipStream = new GZipStream(stream, CompressionLevel.Optimal);

            await gzipStream.WriteAsync(bytes);
        }

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
    }
}