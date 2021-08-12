using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Application.Services
{
    public interface IFileService
    {
        string ContentRootPath { get; }
        string UploadFile(IFormFile file, string saveLocation);
        /// <summary>
        /// Delete file in local
        /// </summary>
        /// <param name="saveLocation"></param>
        /// <param name="filename"></param>
        void DeleteFile(string saveLocation);
        /// <summary>
        ///<para>Hàm thực hiện việc call api chuyển text thành voice</para>
        ///<para>Input: Text là từ, câu muốn chuyển thành voice, Voice là giọng đọc</para>
        ///<para>Output: Là file âm thanh của từ vựng
        ///</summary>
        Task<string> GetAudioFromWord(string word, string voice);
        string GetAppBaseUrl(string fileName,string type);
        string GetImageFileName(string path);
    }
}