using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Samples.API;
using WebAPI.Samples.Model;
using WebAPI.Samples.Utility;

namespace WebAPI.Samples.BLL
{
    public class S3FileOperationBLL : BaseBLL
    {
        AWSS3Utils aWSS3Utils = new AWSS3Utils();
        CommonUtility commonUtility = new CommonUtility();
        internal async Task<string> UploadFile(string metadata, List<IFormFile> FormFilelist)
        {
            try
            {
                //Path.GetExtension(obj.FileName);
                string ext = Path.GetExtension(FormFilelist[0].FileName);

                if (string.IsNullOrEmpty(metadata))
                {
                    return "folder id could not be null";
                }
                UploadFileMetaDetails fileMetaDetails = new UploadFileMetaDetails();
                fileMetaDetails = JsonConvert.DeserializeObject<UploadFileMetaDetails>(metadata);
                if (string.IsNullOrEmpty(fileMetaDetails.FolderID))
                {
                    return "folder id could not be null";
                }
                if (FormFilelist.Count == 0)
                {
                    return "At least one file must be thr";
                }
                bool foldercreted = await aWSS3Utils.CreateFoldersAsync(fileMetaDetails.FolderID);
                if (!foldercreted)
                {
                    return "Coundt create the dirictory";
                }
                bool uploded = false;
                foreach (IFormFile file in FormFilelist)
                {
                    uploded = await aWSS3Utils.UploadFile(file, fileMetaDetails.FolderID);
                }
                if (uploded)
                {
                    return "files hase been uploded";
                }
                return "not uploded";
            }
            catch (Exception)
            {

                throw;
            }
        }

        internal async Task<string> DeleteListofFiles(FileDetails FileMetaDetails)
        {
            try
            {

                if (string.IsNullOrEmpty(FileMetaDetails.FolderID))
                {
                    return "folder id could not be null";
                }
                if (FileMetaDetails.FileDetailsList.Count == 0)
                {
                    return "At least one file must be thr";
                }
                bool uploded = false;
                foreach (FileFullDetails obj in FileMetaDetails.FileDetailsList)
                {
                    uploded = await aWSS3Utils.DeleteFile( FileMetaDetails.FolderID, obj.FileName);
                }
                if (uploded)
                {
                    return "files hase been deleted";
                }
                return "not deleted";
            }
            catch (Exception)
            {

                throw;
            }
        }

        internal async Task<string> GetBase64File(FileDetail file)
        {
            try
            {
             //   { "folderID": "Test1",  "fileName": "Second.png"}
                if (string.IsNullOrEmpty(file.FolderID))
                {
                    return "folder id could not be null";
                }
                if (string.IsNullOrEmpty(file.FileName))
                {
                    return "File name could not be null";
                }
                var stream = await aWSS3Utils.GetFile(file.FolderID, file.FileName);
#if false //this part to save the image into folder
                var filename = "Images" + "\\" + file.FileName;
                var path = AppContext.BaseDirectory;
                var filepath = Path.Combine(path, filename);
                var fileStream = File.Create(filepath);
                stream.CopyTo(fileStream);
                fileStream.Close(); 
#endif

                if (stream != null)
                {
                    string base64file = commonUtility.GetBase64File(stream);
                    string src = "data:image/png;base64,";
                    return src+base64file;
                }
                return "no file found";
            }
            catch (Exception)
            {

                throw;
            }
        }

        internal async Task<FileInfoResp> GetDirectoryFilesBase64(FileDetail file)
        {

            try
            {
                //   { "folderID": "Test1",  "fileName": "Second.png"}
                FileInfoResp fileInfoResp = new FileInfoResp();
                if (string.IsNullOrEmpty(file.FolderID))
                {
                    return null;
                }
                FileDetails fileDetails = await ListAllFiles(file.FolderID);
                fileInfoResp.FolderID = file.FolderID;
                foreach (FileFullDetails obj in fileDetails.FileDetailsList)
                {
                    FileInfoDetails fileInfo = new FileInfoDetails();
                    fileInfo.FileName = obj.FileName;
                    fileInfo.Extention = Path.GetExtension(obj.FileName);
                    string etension = fileInfo.Extention.Replace(".", "");
                    if (!string.IsNullOrEmpty(fileInfo.Extention))
                    {
                        var stream = await aWSS3Utils.GetFile(file.FolderID, obj.FileName);
                        if (stream != null)
                        {

                            string src = "data:image/" + etension + ";base64,";
                            fileInfo.Base64src = src + commonUtility.GetBase64File(stream);
                            fileInfoResp.FileList.Add(fileInfo);
                        }

                    }
                }
                return fileInfoResp;
            }
            catch (Exception)
            {

                throw;
            }
           
        }

      
        internal async Task<string> DeleteDirectory(string DirectoryName)
        {
            try
            {
                FileDetails filelist = new FileDetails();
                if (string.IsNullOrEmpty(DirectoryName))
                {
                    return "folder id could not be null";
                }
                bool DirectoryDeleted = await aWSS3Utils.DeleteDirectoryNameAsync(DirectoryName);
                if (DirectoryDeleted)
                {
                    return "Folder deleted";
                }
                return "Folder could not deleted";
            }
            catch (Exception)
            {

                throw;
            }
        }

        internal async Task<FileDetails> ListAllFiles(string FolderID)
        {
            try
            {
                FileDetails filelist = new FileDetails();
                if (string.IsNullOrEmpty(FolderID))
                {
                    return filelist;
                }
                List<S3ObjectVersion> s3Objects= await aWSS3Utils.FilesList(FolderID);
                if(s3Objects!=null)
                {
                    filelist.FolderID = FolderID;
                    foreach (S3ObjectVersion file in s3Objects)
                    {
                        FileFullDetails details = new FileFullDetails();
                        string filename = file.Key;
                        filename = filename.Replace(FolderID + "/", "");
                        details.FileName = filename;
                        details.FolderPath = file.Key;
                        details.LastModified = file.LastModified;
                        details.Size = file.Size;
                        filelist.FileDetailsList.Add(details);
                    }
                }
                return filelist;
            }
            catch (Exception)
            {

                throw;
            }
        }

        internal string test(string directoryName)
        {
            List<string> chars = new List<string>();
            chars = directoryName.Split(" ").ToList();
            int length = chars.Count;
            string result = "";
            for(int i=0;i<length;i++)
            {
                string trailingzeros = "";
                for(int j=i+1;j<length;j++)
                {
                    trailingzeros +=  "0";
                }
                result = result + "," + chars[i] + trailingzeros;

                if (i == 0)
                {
                    result = "";
                    result =  chars[i] + trailingzeros;
                }

            }
            return result;
        }

      

        internal string enc(CommonPostEc directoryName)
        {
          string d= DecryptString(directoryName.Param);
          //  dep dp = (dep) d;
            d = d + "approved";
         string r= Encrypt(d);
           return r;
        }






        public string Encrypt(string plainText)
        {
            byte[] encrypted;

            using (AesManaged aesM = new AesManaged())
            {
                Aes aess = GetEncryptionAlgorithm();
                ICryptoTransform encryptor = aesM.CreateEncryptor(aess.Key, aess.IV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                            sw.Write(plainText);
                        encrypted = ms.ToArray();
                    }
                }
            }
            string bs64 = Convert.ToBase64String(encrypted);
            return bs64;
        }

        private string DecryptString(string cipherText)
        {
            try
            {
                Aes aes = GetEncryptionAlgorithm();
                byte[] buffer = Convert.FromBase64String(cipherText);
                MemoryStream memoryStream = new MemoryStream(buffer);
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                StreamReader streamReader = new StreamReader(cryptoStream);
                return streamReader.ReadToEnd();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private Aes GetEncryptionAlgorithm()
        {
            try
            {
                Aes aes = Aes.Create();
                string key = "6v9y$B&E)H@McQfTjWnZq4t7w!z%C*F-";
                var secret_key = Encoding.UTF8.GetBytes(key);
                string keyticks = key.Substring(key.Length - 16);
                var initialization_vector = Encoding.UTF8.GetBytes(keyticks);
                //aes.KeySize = 128 / 8;
                aes.Key = secret_key;
                aes.IV = initialization_vector;
                return aes;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}




    

    public class dep
    {
        public string key { get; set; }
        public string data { get; set; }
    }

