using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Engrisk.Helper
{
    public class CloudinaryHelper
    {
        private readonly Cloudinary _cloud;
        public CloudinaryHelper(Account account)
        {
            _cloud = new Cloudinary(account);
        }
        public CloudinaryDTO UploadImage(IFormFile file)
        {
            if(file == null){
                return null;
            }
            var uploadResult = new ImageUploadResult();
            if (file.Length > 0)
            {
                using (var fileStream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, fileStream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };
                    uploadResult = _cloud.Upload(uploadParams);
                };
            }
            else{
                return null;
            }
            if(uploadResult.StatusCode == System.Net.HttpStatusCode.OK){
                return new CloudinaryDTO{
                    PublicId = uploadResult.PublicId,
                    PublicUrl = uploadResult.Url.ToString()
                };
            }
            return null;
        }
        public bool DeleteImage(string publicId){
            var deletionParams = new DeletionParams(publicId);
            var result = _cloud.Destroy(deletionParams);
            if(result.Result != "OK"){
                return false;
            }
            return true;
        }
    }
}