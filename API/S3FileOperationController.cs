using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Samples.BLL;
using WebAPI.Samples.Model;
using WebAPI.Samples.Utility;

namespace WebAPI.Samples.API
{
    [EnableCors("CorsPolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class S3FileOperationController : ControllerBase
    {
        S3FileOperationBLL m_objFileUploadManeger = new S3FileOperationBLL();

        //refernce video link https://www.youtube.com/watch?v=Ii35k0qypFw
        //refernce video link https://www.youtube.com/watch?v=v67NunIp5w8

        


        [Route("UploadFiles")]
        [HttpPost]
        public async Task<ActionResult> MultipartUploadFileAsync([FromHeader] string metadata, [FromForm] List<IFormFile> file)
        {
            // {"folderID": "Test1","docType": "string","saveName": "string"}
            try
            {
                if (file !=null)
                {
                    string upload= await m_objFileUploadManeger.UploadFile(metadata, file);
                    return Ok(upload);
                }
                else
                {
                    return NoContent();
                }

            }
            catch (Exception ex)
            {

                return BadRequest(ex.ToString());
            }
        }

        [Route("ListFiles")]
        [HttpPost]
        public async Task<ActionResult> ListAllFiles(string FolderName)
        {
            try
            {
                if (FolderName != null)
                {
                    FileDetails upload = await m_objFileUploadManeger.ListAllFiles(FolderName);
                    return Ok(upload);
                }
                else
                {
                    return NoContent();
                }

            }
            catch (Exception ex)
            {

                return BadRequest(ex.ToString());
            }
        }

        [Route("GetFileBase64")]
        [HttpPost]
        public async Task<ActionResult> GetFileBase64(FileDetail file)
        {
            try
            {
                if (file != null)
                {
                    var upload = await m_objFileUploadManeger.GetBase64File(file);
                    return Ok(upload);
                }
                else
                {
                    return NoContent();
                }

            }
            catch (Exception ex)
            {

                return BadRequest(ex.ToString());
            }
        }

        [Route("GetDirectoryFilesBase64")]
        [HttpPost]
        public async Task<ActionResult> GetDirectoryFilesBase64(FileDetail file)
        {
            try
            {
                if (file != null)
                {
                    var upload = await m_objFileUploadManeger.GetDirectoryFilesBase64(file);
                    return Ok(upload);
                }
                else
                {
                    return NoContent();
                }

            }
            catch (Exception ex)
            {

                return BadRequest(ex.ToString());
            }
        }
        [Route("DeleteFiles")]
        [HttpPost]
        public async Task<ActionResult> DeleteListofFiles(FileDetails file)
        {
            try
            {
                if (file != null)
                {
                    string upload = await m_objFileUploadManeger.DeleteListofFiles(file);
                    return Ok(upload);
                }
                else
                {
                    return NoContent();
                }

            }
            catch (Exception ex)
            {

                return BadRequest(ex.ToString());
            }
        }

        [Route("DeleteFolder")]
        [HttpPost]
        public async Task<ActionResult> DeleteDirectory(string DirectoryName)
        {
            try
            {
                if (DirectoryName != null)
                {
                    string upload = await m_objFileUploadManeger.DeleteDirectory(DirectoryName);
                    return Ok(upload);
                }
                else
                {
                    return NoContent();
                }

            }
            catch (Exception ex)
            {

                return BadRequest(ex.ToString());
            }
        }

        [Route("test")]
        [HttpPost]
        public ActionResult Test(CommonPostEc data)
        {
           
            try
            {
                if (data != null)
                {
                    //string upload =  m_objFileUploadManeger.test(DirectoryName);
                    string upload =  m_objFileUploadManeger.enc(data);
                    return Ok(upload);
                }
                else
                {
                    return NoContent();
                }

            }
            catch (Exception ex)
            {

                return BadRequest(ex.ToString());
            }
        }
    }


    public class CommonPostEc
    {
        public string Param { get; set; }
    }
}
