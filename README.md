# JZipAppendBlob

Stream appendable JSON data to and from Azure append blobs. GZip compression is used to minimize the size.

### JZipAppendBlob JSON data format:

It is needed to check if the destination blob exists (or any other way to determine the 1st write to the blob). If it does not exist then the 1st data row write to the blob must start with an added [ character. After the 1st data row write the '[' character should not be added. The ',' (comma) character is always added to the end of each data row. For example, when getting data from Sql Server in C# and it is the 1st write to a blob, make sure that the data is prefixed with '[':

```cs
using JZipBlob;

// the 1st row write to a new blob, after checking blob existance and creation
await dataReader.ReadAsync();

dataReader.GetValues(values);

stringBuilder.Append('['); // only the 1st ever row gets this
stringBuilder.Append(JsonConvert.SerializeObject(values));
stringBuilder.Append(',');

// all subsequent rows can then leave out the prefixed [
await dataReader.ReadAsync();

dataReader.GetValues(values);

stringBuilder.Append(JsonConvert.SerializeObject(values));
stringBuilder.Append(',');
```

After the StringBuilder is populated, the JSON data can be compressed/uncompressed and streamed up/down like this:

```cs
// stream up
await JZipAppendBlob.CompressAndAppendBlobAsync(appendBlobClient, stringBuilder.ToString());

// stream down
string decompressed = await JZipAppendBlob.DecompressAndDownloadAppendBlobAsync(blocClient);
List<object[]> dataObject = JsonConvert.DeserializeObject<List<object[]>>(decompressed);
```

It is possible to manually unzip the blob data with a common zipping tool. To make the JSON valid, simply replace the last ',' character with a closing ']' character.
