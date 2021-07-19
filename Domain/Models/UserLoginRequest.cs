using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Domain.Models
{
    public class UserLoginRequest
    {
        public const string GoogleProvider = "google"; 
        public const string FacebookProvider = "facebook"; 
        [JsonProperty("token")]
        [Required]
        public string Token { get; set; }
    }
}