using InsuranceClient.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace InsuranceClient.Helpers
{
    public class StorageHelper
    {
        private CloudStorageAccount cloudStorageAccount;
        private CloudBlobClient cloudBlobClient;
        private CloudTableClient cloudTableClient;
        private CloudQueueClient cloudQueueClient;

        public string ConnectionString
        {
            set
            {
                cloudStorageAccount = CloudStorageAccount.Parse(value);
                cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
                cloudQueueClient = cloudStorageAccount.CreateCloudQueueClient();
            }
        }

        public async Task<string> UploadCustomerImage(string containerName, string imagePath)
        {
            var container = cloudBlobClient.GetContainerReference(containerName);
            await container.CreateIfNotExistsAsync();
            var imageName = Path.GetFileName(imagePath);
            var blob = container.GetBlockBlobReference(imageName);
            await blob.DeleteIfExistsAsync();
            await blob.UploadFromFileAsync(imagePath);
            return blob.Uri.AbsoluteUri;
        }

        public async Task<Customer> InsertCustomerAsync(string tableName, Customer customer)
        {
            var table = cloudTableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();
            TableOperation tableOperation = TableOperation.Insert(customer);
            var result = await table.ExecuteAsync(tableOperation);
            return result.Result as Customer;
        }

        public async Task AddMessageAsync(string queueName, Customer customer)
        {
            var queue = cloudQueueClient.GetQueueReference(queueName);
            await queue.CreateIfNotExistsAsync();
            var msgBody = JsonConvert.SerializeObject(customer);
            CloudQueueMessage cloudQueueMessage = new CloudQueueMessage(msgBody);
            await queue.AddMessageAsync(cloudQueueMessage, TimeSpan.FromDays(3), TimeSpan.Zero, null, null);
        }
    }
}
