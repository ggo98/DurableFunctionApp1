using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Retrieve the connection string for the Azure Storage Emulator
            string connectionString = "UseDevelopmentStorage=true";

            // Create a CloudStorageAccount object from the connection string
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create a CloudBlobClient object from the storage account
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get a reference to the container
            CloudBlobContainer blobContainer = blobClient.GetContainerReference("mycontainer");

            // Create the container if it doesn't exist
            await blobContainer.CreateIfNotExistsAsync();

            // Set permissions on the container to allow public access to blobs
            BlobContainerPermissions containerPermissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            };
            await blobContainer.SetPermissionsAsync(containerPermissions);

            // Upload a sample blob
            CloudBlockBlob blob = blobContainer.GetBlockBlobReference("sampleblob.txt");
            DateTime now = DateTime.Now;
            string strDT = $"{now.ToShortDateString()} {now.ToLongTimeString()}";
            await Console.Out.WriteLineAsync(strDT);
            string content = $"This is a sample blob at {strDT}.";
            await blob.UploadTextAsync(content);

            Console.WriteLine("Blob uploaded successfully.");

            // Download the blob
            string downloadedContent = await blob.DownloadTextAsync();

            Console.WriteLine($"Blob content: {downloadedContent}");

            await Console.Out.WriteAsync("Enter to delete Blob or CTRL+C");
            Console.ReadLine();
            // Delete the blob
            await blob.DeleteAsync();

            Console.WriteLine("Blob deleted successfully.");

            // Delete the container
            await blobContainer.DeleteAsync();

            Console.WriteLine("Container deleted successfully.");
        } 
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync(ex.Message);
        }

        Console.ReadLine();
    }
}