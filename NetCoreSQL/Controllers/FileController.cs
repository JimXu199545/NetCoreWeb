using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.File;
using NetCoreSQL.Models;

namespace NetCoreSQL.Controllers
{
    public class FileController : Controller
    {
        public async Task<IActionResult> Index()
        {
            string accountName = "blobstorage0516";
            string key = "eGier5YJBzr5z3xgOJUb+snTGDKhwPBJRFqb2nL5lcacmKZXHgY+LjmYapIHL7Csvgx75NwiOZE7kYLJfLqWBg==";
            var storageAccount = new CloudStorageAccount(new StorageCredentials(accountName, key), true);
            var share = storageAccount.CreateCloudFileClient().GetShareReference("mainshare");
            var dir =share.GetRootDirectoryReference();
            //list all files in the directory
            var fileData = await list_subdir(dir);
            return View(fileData);
        }



        private static async Task<List<FileShareData>> list_subdir(CloudFileDirectory fileDirectory)
        {
            var fileData = new List<FileShareData>();
            FileContinuationToken token = null;
            do
            {
                FileResultSegment resultSegment = await fileDirectory.ListFilesAndDirectoriesSegmentedAsync(token);
                foreach (var fileItem in resultSegment.Results) {

                    if (fileItem is CloudFile) {
                        var cloudFile = (CloudFile)fileItem;
                        //get the cloudfile's propertities and metadata 
                        await cloudFile.FetchAttributesAsync();  

                        // Add properties to FileShareData List
                        fileData.Add(new FileShareData()
                        {
                            FileName = cloudFile.Name,
                            LastModified = DateTime.Parse(cloudFile.Properties.LastModified.ToString()).ToLocalTime().ToString(),
                            // get file size as kb
                            Size = Math.Round((cloudFile.Properties.Length / 1024f), 2).ToString()
                        });

                    }

                    if (fileItem is CloudFileDirectory)
                    {
                        var cloudFileDirectory = (CloudFileDirectory)fileItem;
                        await cloudFileDirectory.FetchAttributesAsync();

                        //list files in the directory
                        var result = await list_subdir(cloudFileDirectory);
                        fileData.AddRange(result);
                    }
                }
                // get the FileContinuationToken to check if we need to stop the loop
                token = resultSegment.ContinuationToken;
            }
            while (token != null);

            return fileData;
            

        }
    }
}