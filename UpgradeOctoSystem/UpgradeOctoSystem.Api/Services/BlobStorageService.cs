using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using UpgradeOctoSystem.Abstractions.Enums;
using UpgradeOctoSystem.Abstractions.Services;

namespace UpgradeOctoSystem.Api.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        public BlobStorageService()
        {
        }

        public async Task<Uri> UploadFileAsync(string fileName, Stream content, BlobContainers containers, PublicAccessType accessType = PublicAccessType.None)
        {
            if (containers == BlobContainers.None)
                throw new ArgumentException("Containers can not be NONE");

            // TODO: get this from config
            var container = new BlobContainerClient("UseDevelopmentStorage=true", containers.ToString().ToLowerInvariant());
            await container.CreateIfNotExistsAsync(accessType);

            // Get a reference to a blob
            BlobClient blob = container.GetBlobClient(fileName);

            // Open the file and upload its data
            await blob.UploadAsync(content);

            // Verify we uploaded some content
            //BlobProperties properties = await blob.GetPropertiesAsync();
            return blob.Uri;
        }

        public async Task<Uri> GetFileAsync(string fileName, Stream content, BlobContainers containers, PublicAccessType accessType = PublicAccessType.None)
        {
            // TODO: implement get via sas https://docs.microsoft.com/en-us/azure/storage/blobs/sas-service-create?tabs=dotnet
            if (containers == BlobContainers.None)
                throw new ArgumentException("Containers can not be NONE");
            throw new NotImplementedException();
        }
    }
}