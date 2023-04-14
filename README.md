# JZipAppendBlob

Effeciently stream appendable JSON data to and from Azure append blobs. GZip compression is used to minimize the size. The data result is compatible with Azure Data Factory after a seal is done on the blob. If the blob is still appendable and not sealed, then the DecompressAndDownloadAppendBlobAsync method can be called to get a JSON data set. Just beware of the size of the blob and the memory available on the VM that receives the blob data.

```javascript
[
	{"Item1": "value1", "Item2": "value2"},
	{"Item1": "value3", "Item2": "value4"}
```

### JZipAppendBlob JSON data format:

First it is needed to check if the destination blob exists (or any other way to determine the 1st write to the blob). If it does not exist then the 1st data row write to the blob must be prefixed with the '[' character. After the 1st data row write the '[' character should not be added ever again. The ',' (comma) character is always added to the start of each data row, except on the very first row write. For example, when getting data from Sql Server in C#: If it is the 1st row to write to a blob, make sure that the data is prefixed with '[':

```cs
using JZipBlob;
//----------------------------------------------------------------------------

// the 1st row write to a new blob, after checking blob existence and creation
JZipAppendBlob.InitialAppend(stringBuilder);

await dataReader.ReadAsync(); // Sql Client DataReader

object[] values = new object[dataReader.FieldCount];

dataReader.GetValues(values);

// use StartRow(StringBuilder stringBuilder)

// all subsequent rows must be prefixed with ','
JZipAppendBlob.StartRow(stringBuilder);

await dataReader.ReadAsync();
```

After the StringBuilder is populated, the JSON data can be compressed/uncompressed and streamed up/down like this:

```cs
// stream up
await JZipAppendBlob.CompressAndAppendBlobAsync(appendBlobClient, stringBuilder.ToString());

// stream down
string decompressed = await JZipAppendBlob.DecompressAndDownloadAppendBlobAsync(blocClient);
List<object[]> dataObject = JsonConvert.DeserializeObject<List<object[]>>(decompressed);
```

It is also possible to manually unzip the blob data with a common zipping tool. To make the JSON valid, simply append the blob with a closing ']' character, but this should be done by a seal operation or else postfixed to the downloaded stream. A sealed blob has zipped JSON data that can be imported to Azure Data Factory directly. Unsealed blobs are not compatible with Azure Data Factory but can be retrieved as valid JSON from JZipAppendBlob.

### Sealing the blob:

Append the blob data with it`s last ']' character (zipped) using the normal blob append and blob seal techniques when no more data should be written.
