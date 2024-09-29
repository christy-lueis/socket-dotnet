using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WebAPI.Samples.Model;
using WebAPI.Samples.Utility;

namespace WebAPI.Samples.BLL
{
    public class AzureOperationBLL
    {
        string connectionString = "";
        string shareName = "munibtestfileshare";
        string dirName = "munibtestfileshare";
        string folderName = "directory1";
        string fileName = "Testfile.txt";
        BlobServiceClient blobServiceClient;

        public AzureOperationBLL()
        {
             connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");

            // Create a BlobServiceClient object which will be used to create a container client
           //  blobServiceClient = new BlobServiceClient(connectionString);

        }





        CommonUtility commonUtility = new CommonUtility();
        internal async Task<string> GetFile(AzureFileDetails file)
        {
            try
            {
               
              
                // Get a reference to the file
                ShareClient share = new ShareClient(connectionString, file.ShareName);
                ShareDirectoryClient directory = share.GetDirectoryClient(file.DirName);
                ShareFileClient filetodownload = directory.GetFileClient(file.FileName);

                // Download the file
                ShareFileDownloadInfo download = filetodownload.Download();
                 MemoryStream  stream= (MemoryStream)await filetodownload.OpenReadAsync();

                string base64 = commonUtility.GetBase64File(stream);
                //using (FileStream stream = File.OpenWrite(localFilePath))
                //{
                //    download.Content.CopyTo(stream);
                //}
                return base64;
            }
            catch (Exception)
            {

                throw;
            }
        }

    
        internal Task<bool> UploadFile(string metadata, List<IFormFile> files)
        {
            
            try
            {
                ShareFileClient shareFileClient;
                
               

                ShareClient share = new(connectionString, shareName);
                var directory = share.GetDirectoryClient(folderName);

              
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        shareFileClient = directory.GetFileClient(file.FileName);
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            //var fileBytes = ms.ToArray();
                            //string s = Convert.ToBase64String(fileBytes);
                            // act on the Base64 data
                            shareFileClient.Create(ms.Length);

                            shareFileClient.UploadRange(new HttpRange(0, ms.Length), ms);

                        }
                    }
                }


              
                return Task.FromResult(true);
            }
            catch (Exception)
            {

                throw;
            }
        }

       

        internal  void TraverseaShare(AzureFileDetails file)
        {
          
            string shareName = "sample-share";
            ShareClient share = new ShareClient(connectionString, shareName);

            // Track the remaining directories to walk, starting from the root
            var remaining = new Queue<ShareDirectoryClient>();
            remaining.Enqueue(share.GetRootDirectoryClient());
            while (remaining.Count > 0)
            {
                // Get all of the next directory's files and subdirectories
                ShareDirectoryClient dir = remaining.Dequeue();
                foreach (ShareFileItem item in dir.GetFilesAndDirectories())
                {
                    // Print the name of the item
                    Console.WriteLine(item.Name);

                    // Keep walking down directories
                    if (item.IsDirectory)
                    {
                        remaining.Enqueue(dir.GetSubdirectoryClient(item.Name));
                    }
                }
            }
        }

        internal Task<bool> CreateaShareandUploadFile(string metadata, List<IFormFile> files)
        {

            try
            {
                
                string shareNameTocreate = "sample-share";
                string dirNameTocreate = "sample-share";
      
             

                // Get a reference to a share and then create it
                ShareClient share = new ShareClient(connectionString, shareNameTocreate);
                share.Create();

                // Get a reference to a directory and create it
                ShareDirectoryClient directory = share.GetDirectoryClient(dirNameTocreate);
                directory.Create();


                ShareFileClient shareFileClient;
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        shareFileClient = directory.GetFileClient(file.FileName);
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            //var fileBytes = ms.ToArray();
                            //string s = Convert.ToBase64String(fileBytes);
                            // act on the Base64 data
                            shareFileClient.Create(ms.Length);

                            shareFileClient.UploadRange(new HttpRange(0, ms.Length), ms);

                        }
                    }
                }
                //// Get a reference to a file and upload it
                //ShareFileClient file = directory.GetFileClient(fileName);
                //using (FileStream stream = File.OpenRead(localFilePath))
                //{
                //    file.Create(stream.Length);
                //    file.UploadRange(
                //        new HttpRange(0, stream.Length),
                //        stream);
                //}



                return Task.FromResult(true);
            }
            catch (Exception)
            {

                throw;
            }
        }

        internal List<string> haotest(int testnumber)
        {
            string result = "";
           List<int> resultlist=new List<int>();
           List<string> strresultlist=new List<string>();

            for (int i = 0;  testnumber!=0; testnumber= testnumber / 10,i++)
            {
                int multi = Convert.ToInt32("1" + "{0:Di }");
                //resultlist.Add(testnumber%10);
                strresultlist.Add((Convert.ToInt32(testnumber % 10)* multi).ToString());
            }

            return strresultlist;
        }

        internal void CreateaShare()
        {
                // Connect to the existing share
              
                string shareNameTocreate = "sample-share";
                ShareClient share = new ShareClient(connectionString, shareNameTocreate);

                try
                {
                    // Try to create the share again
                    share.Create();
                }
                catch (RequestFailedException ex)
                    when (ex.ErrorCode == ShareErrorCode.ShareAlreadyExists)
                {
                    // Ignore any errors if the share already exists
                }

              
        }





        internal async Task<bool> UploadFileBlob(string metadata, List<IFormFile> files)
        {

            //Create a unique name for the container
            string containerName = "quickstartblobs" ;

            // Create the container and return a container client object
            BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);



            // Get a reference to a blob
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            foreach (var file in files)
            {
                if (file.Length > 0)
                {

                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        //var fileBytes = ms.ToArray();
                        //string s = Convert.ToBase64String(fileBytes);
                        // act on the Base64 data
                        await blobClient.UploadAsync(ms, true);

                    }

                }
                // Upload data from the local file
            }
            return true;
        }





        internal async Task GetFileBlob(AzureFileDetails file)
        {

           // // Create a BlobServiceClient object which will be used to create a container client
           // BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);



           // // Get a reference to a blob
           // BlobClient blobClient = blobServiceClient.GetBlobContainerClient(containerName);



           // string downloadFilePath = "";

           // Console.WriteLine("\nDownloading blob to\n\t{0}\n", downloadFilePath);

           // // Download the blob's contents and save it to a file
           //var response=  await blobClient.DownloadToAsync(downloadFilePath);
        }


        internal async Task<List<BlobItem>> ListBlobAsync(AzureFileDetails file,string containerName)
        {
            List<BlobItem> blobItems = new List<BlobItem>();
             BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                blobItems.Add(blobItem);
            }
            return blobItems;
        }


        //  public virtual Azure.Response Delete(Azure.Storage.Files.Shares.Models.ShareFileRequestConditions conditions = default, System.Threading.CancellationToken cancellationToken = default);
    }
}
