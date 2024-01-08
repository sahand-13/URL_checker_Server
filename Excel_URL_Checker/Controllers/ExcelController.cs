﻿using Excel_URL_Checker.Interfaces;
using Excel_URL_Checker.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Text;

namespace Excel_URL_Checker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExcelController : ControllerBase
    {
        private readonly IDatasourceService _datasourceService;
        private readonly ICompareService _compareService;
        private readonly ICreateExcelService _createExcelService;
        public ExcelController(IDatasourceService datasourceService, ICompareService compareService, ICreateExcelService createExcelService)
        {
            _datasourceService = datasourceService;
            _compareService = compareService;
            _createExcelService = createExcelService;
        }
        private static byte[] CompressData(string data)
        {
            using var compressedStream = new MemoryStream();
            using var compressionStream = new GZipStream(compressedStream, CompressionMode.Compress);
            using var writer = new StreamWriter(compressionStream);

            writer.Write(data);
            writer.Flush();

            return compressedStream.ToArray();
        }
        [HttpGet("CreateExcel")]
        public async Task<IActionResult> GetExcelData(int Similarity, string DBNames)
        {
            try
            {
                var DBList = JsonConvert.DeserializeObject<List<string>>(DBNames);


                var ExcelData = await _datasourceService.LoadDataSource(Similarity, DBList);

                var result = await _compareService.CompareData(ExcelData, Similarity);

                var Response = await _createExcelService.createExcel(result, DBList, Similarity);
                //var BytesOfData = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(result);

                return Ok(Response);
                //return File(BytesOfData, "application/octet-stream");
            }
            catch (Exception e)
            {

                throw;
            }




        }
        [HttpGet("Exports")]
        public async Task<IActionResult> Getexports()
        {
            var filesList = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Exports"));
            var filesNames = filesList.Select(i => Path.GetFileName(i)).ToList();
            return Ok(new Response<List<string>>()
            {
                Succeeded = true,
                Data = filesNames
            });
        }
        [HttpGet]
        public async Task<IActionResult> GetImports()
        {
            var filesList = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Imports"));
            var filesNames = filesList.Select(i => Path.GetFileName(i)).ToList();
            return Ok(new Response<List<string>>()
            {
                Succeeded = true,
                Data = filesNames
            });
        }
        [HttpPost]
        public async Task<IActionResult> PostExcels(List<IFormFile> Files)
        {
            if (Files?.Count > 0)
            {
                foreach (var file in Files)
                {
                    var SavePath = Path.Combine(Directory.GetCurrentDirectory(), "Imports", file.FileName);

                    if (System.IO.File.Exists(SavePath))
                    {
                        var Extention = Path.GetExtension(file.FileName);
                        var FileName = Path.GetFileNameWithoutExtension(file.FileName);
                        Random rnd = new Random();
                        int roundom = rnd.Next(int.MinValue, int.MaxValue);
                        SavePath = Path.Combine(Directory.GetCurrentDirectory(), "Imports", FileName + roundom + Extention);
                    };
                    using (var stream = new FileStream(SavePath, FileMode.Create))
                    {

                        await file.CopyToAsync(stream);
                    }
                }

            }

            return Ok(new Response<string>()
            {
                Succeeded = true,
                Message = "file uploaded successfull"
            });
        }
        [HttpDelete("{FileName}")]
        public async Task<IActionResult> Delete(string FileName)
        {
            if (FileName != null)
            {

                var SavePath = Path.Combine(Directory.GetCurrentDirectory(), "Imports", FileName);

                if (System.IO.File.Exists(SavePath))
                {
                    System.IO.File.Delete(SavePath);
                };

            }
            return Ok(new Response<string>()
            {
                Succeeded = true,
                Message = "file deleted successfull"
            });
        }
    }
}
