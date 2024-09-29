using System;
using System.Collections.Generic;

namespace WebAPI.Samples.Model
{
    public class AzureFileDetails
    {
        public string ContainerName { get; set; }
        public string ShareName { get; set; }
        public string DirName { get; set; }
        public string FileName { get; set; }
        public List<AzureFileFullDetails> FileDetailsList { get; set; }
        public AzureFileDetails()
        {
            FileDetailsList = new List<AzureFileFullDetails>();
        }
    }

    public class AzureFileFullDetails
    {
        public string ContainerName { get; set; }
       // public DateTime LastModified { get; set; }
      //  public long Size { get; set; }
        public string FileName { get; set; }

    }

}
