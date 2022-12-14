using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ImageResizeWebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.AzureAppServices.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageResizeWebApp.Helpers
{
    public static class StorageHelper
    {

        public static bool IsImage(IFormFile file)
        {
            if (file.ContentType.Contains("image"))
            {
                return true;
            }

            string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg" };

            return formats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<bool> UploadFileToStorage(Stream fileStream, string fileName,
                                                            AzureStorageConfig _storageConfig)
        {
            // Create a URI to the blob
            Uri blobUri = new Uri($"https://{_storageConfig.AccountName}.blob.core.windows.net/{_storageConfig.ImageContainer}/{fileName}");

            // Create StorageSharedKeyCredentials object by reading
            // the values from the configuration (appsettings.json)
            StorageSharedKeyCredential storageCredentials =
                new StorageSharedKeyCredential(_storageConfig.AccountName, _storageConfig.AccountKey);

            // Create the blob client.
            BlobClient blobClient = new BlobClient(blobUri, storageCredentials);

            // Upload the file
            await blobClient.UploadAsync(fileStream, overwrite: true);
            return await Task.FromResult(true);
        }

        public static async Task<List<string>> GetBlurredUrls(AzureStorageConfig _storageConfig, bool getOrg = false)
        {
            List<string> blurredUrls = new List<string>();

            // Create a URI to the storage account
            Uri accountUri = new Uri("https://" + _storageConfig.AccountName + ".blob.core.windows.net/");

            // Create BlobServiceClient from the account URI
            BlobServiceClient blobServiceClient = new BlobServiceClient(accountUri);

            // Get reference to the container
            BlobContainerClient container = blobServiceClient.GetBlobContainerClient(getOrg ? _storageConfig.ImageContainer : _storageConfig.ThumbnailContainer);
            System.Diagnostics.Trace.TraceInformation($"Trying to access {container.Uri}");
            if (container.Exists())
            {
                foreach (BlobItem blobItem in container.GetBlobs())
                {
                        blurredUrls.Add(container.Uri + "/" + blobItem.Name);
                }
            }

            return await Task.FromResult(blurredUrls);
        }
    }
}
