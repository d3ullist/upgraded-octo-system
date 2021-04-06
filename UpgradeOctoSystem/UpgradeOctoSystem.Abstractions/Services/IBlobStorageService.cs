using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using UpgradeOctoSystem.Abstractions.Enums;

namespace UpgradeOctoSystem.Abstractions.Services
{
    public interface IBlobStorageService
    {
        Task<Uri> UploadFileAsync(string fileName, Stream content, BlobContainers containers, PublicAccessType blob);
    }
}