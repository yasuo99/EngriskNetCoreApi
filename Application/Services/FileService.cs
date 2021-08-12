using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Word;
using Application.Utilities;
using Application.DTOs.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Web;

namespace Application.Services
{
    public class FileService : IFileService
    {
        private string contentRootPath = "";
        private readonly IHttpClientFactory _httpClient;
        private readonly IConfiguration _config;
        private IWebHostEnvironment hostEnv;
        private IHttpContextAccessor _httpContext;
        public FileService(IWebHostEnvironment hostEnv, IHttpClientFactory httpClient, IConfiguration config, IHttpContextAccessor httpContext)
        {
            this.hostEnv = hostEnv;
            _config = config;
            contentRootPath = hostEnv.ContentRootPath;
            _httpClient = httpClient;
            _httpContext = httpContext;
        }

        public string ContentRootPath { get => contentRootPath; }

        public void DeleteFile(string saveLocation)
        {
            Uri uri = new Uri(saveLocation);
            var filepath = Path.Combine(contentRootPath, uri.AbsolutePath);
            System.IO.File.Delete(filepath);
        }
        public async Task<string> GetAudioFromWord(string word, string voice)
        {
            var content = new WordToSpeechDTO()
            {
                Engine = "Google",
                Data = new Application.DTOs.Word.Data()
                {
                    Text = word,
                    Voice = voice
                }
            };

            var camelCasePropertiesName = new JsonSerializerSettings();
            camelCasePropertiesName.ContractResolver = new CamelCasePropertyNamesContractResolver();
            var body = JsonConvert.SerializeObject(content, camelCasePropertiesName);
            try
            {
                var textToSpeechRequest = await _httpClient.CreateClient().PostAsync(_config.GetSection("TextToSpeech:ApiUrl").Value, new StringContent(JsonConvert.SerializeObject(content, camelCasePropertiesName), Encoding.UTF8, "application/json"));
                if (textToSpeechRequest.IsSuccessStatusCode)
                {
                    var response = textToSpeechRequest.Content;
                    var responseContent = JsonConvert.DeserializeObject<TextToSpeechResponse>(await response.ReadAsStringAsync());
                    using (var webClient = new WebClient())
                    {
                        var filepath = Path.Combine(contentRootPath, SD.AudioPath);
                        var uri = new Uri(String.Concat(_config.GetSection("TextToSpeech:FileDownloadUrl").Value, responseContent.Id + ".mp3"));
                        webClient.DownloadFileAsync(uri, Path.Combine(filepath, word + ".mp3"));
                        return Path.Combine(GetAppBaseUrl(Path.Combine(SD.AudioPath, word + ".mp3"), "audio"));
                    }
                }
                return null;
            }
            catch (HttpRequestException e)
            {
                throw e;
            }


        }

        public string UploadFile(IFormFile file, string saveLocation)
        {
            var upload = Path.Combine(contentRootPath, saveLocation);
            var extension = Path.GetExtension(file.FileName);
            using (var fileStream = new FileStream(Path.Combine(upload, file.FileName), FileMode.Create))
            {
                file.CopyTo(fileStream);
            }
            if (saveLocation.Equals(SD.ImagePath))
            {
                return Path.Combine(GetAppBaseUrl(Path.Combine(saveLocation, file.FileName), "image"));
            }
            return Path.Combine(GetAppBaseUrl(Path.Combine(saveLocation, file.FileName), "audio"));
        }
        public string GetAppBaseUrl(string saveLocation, string type)
        {

            return string.Format("{0}://{1}/{2}{3}", _httpContext.HttpContext.Request.Scheme, _httpContext.HttpContext.Request.Host, _httpContext.HttpContext.Request.PathBase, saveLocation);
        }
        public string GetStreamBaseUrl(string saveLocation, string type)
        {

            return string.Format("{0}://{1}/{2}/{3}{4}", _httpContext.HttpContext.Request.Scheme, _httpContext.HttpContext.Request.Host, _httpContext.HttpContext.Request.PathBase, "api/v2/streaming/audio?audio=",saveLocation);
        }
        public string GetImageFileName(string path)
        {
            Uri uri = new Uri(GetAppBaseUrl(path, "image"));
            var location = HttpUtility.ParseQueryString(uri.Query).Get("image");
            return location;
        }
    }
}