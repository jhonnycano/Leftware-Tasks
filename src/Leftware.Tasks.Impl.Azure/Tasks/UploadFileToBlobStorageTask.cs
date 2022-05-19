using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.Model;
using Leftware.Tasks.Core.TaskParameters;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Spectre.Console;

namespace Leftware.Tasks.Impl.Azure.Tasks;

[Descriptor("Azure Storage - Upload file to blob")]
public class UploadFileToBlobStorageTask : CommonTaskBase
{
    private const string CONNECTION = "connection";
    private const string CONTAINER = "container";
    private const string FILE = "file";

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new SelectFromCollectionTaskParameter(CONNECTION, "connection", Defs.Collections.AZURE_STORAGE_CONNECTION),
            new SelectFromCollectionTaskParameter(CONTAINER, "database", Defs.Collections.AZURE_STORAGE_BLOB_CONTAINER, true)
                .WithDefaultValue($"->{Defs.Collections.AZURE_STORAGE_CONNECTION}|{CONNECTION}|$.Container"),
            new ReadFileTaskParameter(FILE, "file"),
        };
    }

    public async override Task Execute(IDictionary<string, object> input)
    {
        var connectionKey = input.Get(CONNECTION, "");
        var container = GetCollectionValue<string>(input, CONTAINER, Defs.Collections.AZURE_STORAGE_BLOB_CONTAINER);
        var file = input.Get(FILE, "");

        var connection = Context.CollectionProvider!.GetItemContentAs<StorageConnection>(Defs.Collections.AZURE_STORAGE_CONNECTION, connectionKey!);
        if (connection == null)
        {
            UtilConsole.WriteError($"Source connection not found: {connectionKey}");
            return;
        }

        connection.Container = container;

        await UploadFileToBlob(connection, file!);
    }

    public async Task UploadFileToBlob(StorageConnection connection, string filePath)
    {
        WriteStatus("Openning connection");
        
        using Stream file = new FileStream(filePath, FileMode.Open);
        var storageAccount = CloudStorageAccount.Parse(connection.ConnectionString);
        var blobClient = storageAccount.CreateCloudBlobClient();
        var container = blobClient.GetContainerReference(connection.Container);

        WriteStatus("Creating container if not exists");
        var createContainerTask = container.CreateIfNotExistsAsync();
        createContainerTask.Wait();
        var createContainer = createContainerTask.Result;
        if (createContainer)
        {
            var permissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            };
            var permissionsTask = container.SetPermissionsAsync(permissions);
            permissionsTask.Wait();
        }

        var extension = Path.GetExtension(filePath);
        var fileNameWithExtension = Path.GetFileName(filePath);
        var cloudBlockBlob = container.GetBlockBlobReference(fileNameWithExtension);
        cloudBlockBlob.Properties.ContentType = GetMimeTypeForFileExtension(extension);
        WriteStatus("Uploading file to container");
        var uploadTask = cloudBlockBlob.UploadFromStreamAsync(file);
        uploadTask.Wait();

        WriteStatus("Upload completed");
    }

    private void WriteStatus(string msg)
    {
        Context.StatusContext!.Status = msg;
        AnsiConsole.WriteLine(msg);
    }

    private static string GetMimeTypeForFileExtension(string filePath)
    {
        const string DefaultContentType = "application/octet-stream";
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(filePath, out var contentType))
        {
            contentType = DefaultContentType;
        }

        return contentType;
    }
}
