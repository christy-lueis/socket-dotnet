using System;
using System.IO;

namespace WebAPI.Samples.Utility
{
    public class CommonUtility
    {


        public string GetBase64File(MemoryStream stream)
        {
            try
            {
                string base64 = string.Empty;
                byte[] bytes;
                bytes = stream.ToArray();
                base64 = System.Convert.ToBase64String(bytes);
                if (string.IsNullOrEmpty(base64))
                {
                    return null;
                }
                return base64;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
