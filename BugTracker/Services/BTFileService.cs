using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    //public class BTFileService : IBTFileService
    //{
    //    private const int DefaultMaxFileSize = (2 * 1024 * 1024);
    //    public string ContentType(IFormFile file)
    //    {
    //        return file?.ContentType.Split("/")[1];
    //    }

    //    public string DecodeFile(byte[] data, string type)
    //    {
    //        if (data is null || type is null) return null;
    //        return $"data:image/:{type};base64,{Convert.ToBase64String(data)}";
    //    }

    //    public async Task<byte[]> EncodeFileAsync(IFormFile file)
    //    {
    //        if (file is null) return null;

    //        using var ms = new MemoryStream();
    //        await file.CopyToAsync(ms);
    //        return ms.ToArray();
    //    }

    //    public async Task<byte[]> EncodeFileAsync(string fileName)
    //    {
    //        var file = $"{Directory.GetCurrentDirectory()}/wwwroot/img/{fileName}";
    //        return await File.ReadAllBytesAsync(file);
    //    }

    //    public int Size(IFormFile file)
    //    {
    //        return Convert.ToInt32(file?.Length);
    //    }

    //    public bool ValidateFileSize(IFormFile file)
    //    {
    //        return Size(file) < DefaultMaxFileSize;
    //    }

    //    public bool ValidateFileSize(IFormFile file, int maxSize)
    //    {
    //        return Size(file) < maxSize;
    //    }

    //    public bool ValidateFileType(IFormFile file)
    //    {
    //        var authorizedTypes = new List<string> { ".jpg", ".jpeg", ".png", ".gif" };
    //        var validExt = authorizedTypes.Contains(ContentType(file));
    //        return validExt;

    //    }

    //    public bool ValidateFileType(IFormFile file, List<string> fileTypes)
    //    {
    //        var validExt = fileTypes.Contains(ContentType(file));
    //        return validExt;
    //    }
    //}

    public class BTFileService : IBTFileService
    {
        private readonly string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" };
        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            MemoryStream memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var byteFile = memoryStream.ToArray();
            memoryStream.Close();
            memoryStream.Dispose();


            return byteFile;


        }


        public string ConvertByteArrayToFile(byte[] fileData, string extension)
        {
            string imageBase64Data = Convert.ToBase64String(fileData);
            return string.Format($"data:image/{extension};base64,{imageBase64Data}");


        }


        public string GetFileIcon(string file)
        {
            string ext = Path.GetExtension(file).Replace(".", "");
            return $"/assets/images/png/{ext}.png";
        }


        public string FormatFileSize(long bytes)
        {
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return string.Format("{0:n1}{1}", number, suffixes[counter]);
        }
    }
}
