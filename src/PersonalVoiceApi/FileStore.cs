using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;

public class FileStore {

    private readonly BlobServiceClient BlobServiceClient;

    private readonly BlobContainerClient ContainerClient;

    public FileStore(string storageEndpoint, string containerName) {

        this.BlobServiceClient = new BlobServiceClient(
            new Uri(storageEndpoint),
            new DefaultAzureCredential(false));

        this.ContainerClient = this.BlobServiceClient.GetBlobContainerClient(containerName);
        // await containerClient.CreateIfNotExistsAsync();
    }
    
    public async Task<string> SaveFileAsync(byte[] data) {

        string blobName = $"output_{DateTime.UtcNow:yyyyMMddhhmmss}_{Guid.NewGuid().ToString().Substring(0,4)}.wav";

        // Upload the byte[] into the target blob (overwrite if it exists)
        var blobClient = this.ContainerClient.GetBlobClient(blobName);
        using var ms = new MemoryStream(data, writable: false);
        await blobClient.UploadAsync(ms, overwrite: true);

        // 3. Build the SAS token
        var sasBuilder = new BlobSasBuilder {
            BlobContainerName = this.ContainerClient.Name,
            BlobName = blobName,
            Resource = "b",                              // "b" = blob
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5), // clock skew tolerance
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(15) // 15 minutes validity
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        // 4. Sign the SAS with your account key
        var userDelegationKey =  this.BlobServiceClient.GetUserDelegationKey(
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(15));

        var sasToken = sasBuilder.ToSasQueryParameters(userDelegationKey, this.ContainerClient.AccountName).ToString();

        // 5. Return the full URI with SAS
        return $"{blobClient.Uri}?{sasToken}";
    }
}