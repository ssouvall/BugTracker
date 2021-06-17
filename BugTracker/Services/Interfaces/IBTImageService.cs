using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services.Interfaces
{
    public interface IBTImageService
    {
        public string ContentType(IFormFile file);
        public string DecodeFile(byte[] data, string type);
        public Task<byte[]> EncodeFileAsync(IFormFile file);
        public int Size(IFormFile file);
        public bool ValidateFileSize(IFormFile file);
        public bool ValidateFileSize(IFormFile file, int maxSize);
        public bool ValidateFileType(IFormFile file);
        public bool ValidateFileType(IFormFile file, List<string> fileTypes);

    }
}
