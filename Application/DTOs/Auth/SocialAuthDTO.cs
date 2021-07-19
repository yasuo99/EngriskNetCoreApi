using Newtonsoft.Json;

namespace Application.DTOs.Auth
{
    public class SocialAuthDTO
    {
        [JsonProperty("first_name")]
        public string First_name { get; set; }
        [JsonProperty("last_name")]
        public string Last_name { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("picture")]
        public Picture Picture{ get; set; }
    }
    public class Picture{
        [JsonProperty("data")]
        public Data Data { get; set; }
    }
    public class Data{
        [JsonProperty("height")]
        public int Height { get; set; }
        [JsonProperty("is_silhouette")]
        public bool IsSilHouette { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("width")]
        public int Width { get; set; }
    }
}