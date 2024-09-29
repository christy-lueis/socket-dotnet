
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Samples.Model;

namespace WebAPI.Samples.Utility
{
    public class AWSS3Utils
    {
        public static  IAmazonS3 m_S3Client { get; set; }
        public async Task GetBucketListAsync()
        {
            try
            {             
                var bucket = await m_S3Client.ListBucketsAsync();
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public static void SetIAmazonS3Connection(BasicAWSCredentials credentials, RegionEndpoint region)
        {
            m_S3Client= new AmazonS3Client(credentials, region);
        }
        public async Task<bool>  CreateFoldersAsync(string path)
        {
            string bucketName = "minsure-pdf-merger";
            if (!path.EndsWith('/'))
            {
                path = path + "/";
            }

            var findFolderRequest = new ListObjectsV2Request();
            findFolderRequest.BucketName = bucketName;
            findFolderRequest.Prefix = path;
            findFolderRequest.MaxKeys = 1;

            ListObjectsV2Response findFolderResponse =
               await m_S3Client.ListObjectsV2Async(findFolderRequest);


            if (findFolderResponse.S3Objects.Any())
            {
                return true;
            }

            PutObjectRequest request = new PutObjectRequest()
            {
                BucketName = bucketName,
                StorageClass = S3StorageClass.Standard,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.None,
                Key = path,
                ContentBody = string.Empty
            };

            // add try catch in case you have exceptions shield/ handling here
            PutObjectResponse response = await m_S3Client.PutObjectAsync(request);
            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                return true;
            else
                return false;
        }
        public async Task<bool> DeleteDirectoryNameAsync(string path)
        {
            string bucketName = "minsure-pdf-merger";          

            var findFolderRequest = new ListVersionsRequest();
            findFolderRequest.BucketName = bucketName;
            findFolderRequest.Prefix = path;

            ListVersionsResponse findFolderResponse =await m_S3Client.ListVersionsAsync(findFolderRequest);


            if (!findFolderResponse.Versions.Any())
            {
                return true;
            }

            DeleteObjectsRequest dltrequest = new DeleteObjectsRequest();

            dltrequest.BucketName = bucketName;
            
            foreach (S3ObjectVersion entry in findFolderResponse.Versions)
            {

                dltrequest.AddKey(entry.Key);
            }
            DeleteObjectsResponse response = await m_S3Client.DeleteObjectsAsync(dltrequest);
            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                return true;
            else
                return false;
        }

        public async Task<bool> UploadFile(IFormFile file, string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    if (!path.EndsWith('/'))
                    {
                        path = path + "/";
                    }
                }

                PutObjectRequest request = new PutObjectRequest();
                request.InputStream = file.OpenReadStream();
                request.BucketName = "minsure-pdf-merger";
                request.Key = path + file.FileName;

                PutObjectResponse response = await m_S3Client.PutObjectAsync(request);
                var json = response.ResponseMetadata;
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    return true;
                else
                    return false;
            }
            catch (Exception )
            {

                throw ;
            }
        }
        public async Task<bool> DeleteFile(string path, string fileName)
        {
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    if (!path.EndsWith('/'))
                    {
                        path = path + "/";
                    }
                }
                string key = path + fileName;
                DeleteObjectRequest request = new DeleteObjectRequest();
                request.BucketName = "minsure-pdf-merger";
                request.Key = key;
                DeleteObjectResponse response = await m_S3Client.DeleteObjectAsync(request);
                if (response.HttpStatusCode == System.Net.HttpStatusCode.NoContent)
                    return true;
                else
                    return false;
            }
            catch (Exception )
            {
                throw ;
            }
        }
        public async Task<List<S3ObjectVersion>> FilesList(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (!path.EndsWith('/'))
                {
                    path = path + "/";
                }
            }
            ListVersionsRequest request = new ListVersionsRequest();
            request.BucketName = "minsure-pdf-merger";
            request.Prefix = path;
            ListVersionsResponse response = await m_S3Client.ListVersionsAsync(request);
            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                return response.Versions;
            else
                return null;
        }

        public async Task<MemoryStream> GetFile(string path, string fileName)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (!path.EndsWith('/'))
                {
                    path = path + "/";
                }
            }
            GetObjectRequest request = new GetObjectRequest();
            request.BucketName = "minsure-pdf-merger";
            request.Key = path + fileName; 
            GetObjectResponse response = await m_S3Client.GetObjectAsync(request);
            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                MemoryStream memoryStream = new MemoryStream();

                using (Stream responseStream = response.ResponseStream)
                {
                    responseStream.CopyTo(memoryStream);
                }
                return memoryStream;
            }

            else
                return null;
        }

    }
}
