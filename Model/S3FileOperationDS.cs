
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Samples.Model
{
   
    public class UploadFileMetaDetails
    {
        public string FolderID { get; set; }
        public string DocType { get; set; }
    }
    public class FileDetail
    {
        public string FolderID { get; set; }
        public string FileName { get; set; }
    }
    public class FileDetails
    {
        public string FolderID { get; set; }
        public List<FileFullDetails> FileDetailsList { get; set; }
        public FileDetails()
        {
            FileDetailsList = new List<FileFullDetails>();
        }        

    }
    public class FileFullDetails
    {
        public string FolderPath { get; set; }
        public DateTime LastModified { get; set; }
        public long Size { get; set; }
        public string  FileName { get; set; }

        //also list all details like file or folder ,date last edited or created ,size ,attib properties
        
    }
    public class FileInfoResp
    {
        public string FolderID { get; set; }
        public List<FileInfoDetails> FileList { get; set; }
        public FileInfoResp()
        {
            FileList = new List<FileInfoDetails>();
        }
    }
    public class FileInfoDetails
    {
        public string FileName { get; set; }
        //included //data:image/png;base64, so it can directly assing to a img src
        public string Base64src { get; set; }
        public string Extention { get; set; } 
    }

}
