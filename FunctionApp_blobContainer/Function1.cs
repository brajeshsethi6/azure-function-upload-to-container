using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Blobs;
using System.Collections.Generic;

namespace FunctionApp_blobContainer
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string connectionString = "connectionString";
            string containerName = "test";

            List<string> filePaths = new List<string>();

            if (req.Query.ContainsKey("filePath"))
            {
                filePaths.AddRange(req.Query["filePath"].ToString().Split(','));
            }

            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            //if (data?.filePaths != null)
            //{
            //    foreach (var path in data.filePaths)
            //    {
            //        filePaths.Add((string)path);
            //    }
            //}

            //if (filePaths.Count == 0)
            //{
            //    return new BadRequestObjectResult("No file paths provided in the request.");
            //}

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            List<string> uploadedFiles = new List<string>();

            foreach (var filePath in filePaths)
            {
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
               
                    string fileName = Path.GetFileName(filePath);
                    string folderName = "folder/subfolder";
                    string blobName = $"{folderName}/{fileName}";
                    BlobClient blobClient = containerClient.GetBlobClient(blobName);
                    using (var stream = File.OpenRead(filePath))
                    {
                        await blobClient.UploadAsync(stream, true);
                    }
                    uploadedFiles.Add(blobName); 
                }
                else
                {
                    log.LogWarning($"File {filePath} does not exist or invalid path.");
                }
            }
            if (uploadedFiles.Count == 0)
            {
                return new BadRequestObjectResult("None of the provided file paths were valid.");
            }

            return new OkObjectResult($"Uploaded files: {string.Join(", ", uploadedFiles)}");
        }
    }
}
